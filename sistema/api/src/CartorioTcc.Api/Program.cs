using System.Text.Json.Serialization;
using Serilog;
using CartorioTcc.Api.Authorization;
using CartorioTcc.Api.Middlewares;
using CartorioTcc.CrossCutting.IoC;
using CartorioTcc.CrossCutting.Logging;
using CartorioTcc.Infra.Data.Context;
using CartorioTcc.Infra.Data.Seed;

var builder = WebApplication.CreateBuilder(args);

// Logging estruturado com Serilog
builder.Host.UseCartorioTccSerilog();

// Controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// OpenAPI & Swagger UI
builder.Services.AddOpenApi();

// CORS liberado para o Frontend Angular (http://localhost:4200)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CartorioTccCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Content-Disposition");
    });
});

// Estrutura de Autenticação e Autorização (Ativa em modo Bypass conforme diretriz arquitetural)
builder.Services.AddCartorioSecurityStructure();

// Injeção de Dependências centralizada (CrossCutting.IoC)
builder.Services.RegisterCartorioTccServices(builder.Configuration);

var app = builder.Build();

// Seed inicial dos dados do TCC (2º Tabelionato de Notas de Campinas)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CartorioTccContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await CartorioTccSeeder.SeedAsync(context, logger);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("CartorioTccCors");

app.MapOpenApi("/openapi/v1.json");
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "CartorioTcc SAEC API v1 (.NET 10)");
    options.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
