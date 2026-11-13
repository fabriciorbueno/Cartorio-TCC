# SAEC — Sistema Automatizador de Escrituração para Cartórios (CartorioTcc)

Reconstrução e modernização completa do sistema apresentado no Trabalho de Conclusão de Curso de 2015 (**SAEC — Sistema Automatizador de Escrituração para Cartórios**, desenvolvido para o **2º Tabelionato de Notas de Campinas/SP**), utilizando a stack corporativa de 2026:

- **Backend (`sistema/api`)**: **.NET 10 (`net10.0`)** + C# 13+ + **Entity Framework Core + SQL Server** em **Clean Architecture / DDD Pragmático e CQRS**.
- **Frontend (`sistema/app`)**: **Angular `21.2.23`** + TypeScript 5.9 + **Angular Material 21 (Material Design 3)** com Standalone Components, Lazy Loading e Signals.

---

## 1. Estrutura da Solução Backend (`sistema/api`)

```text
CartorioTcc.sln
├── src/CartorioTcc.Api                   [Apresentação REST API, BaseApiController, OpenAPI/Swagger, Auth Bypass]
├── src/CartorioTcc.Business              [Domínio, AuditableEntity, DTOs, Commands, Validador, Builders CNB/SP, Services]
├── src/CartorioTcc.Infra                 [EF Core, CartorioTccContext, Mapeamentos, SqlServerMigrationSql, QueryServices, Refit, QuestPDF]
├── src/CartorioTcc.CrossCutting.IoC      [Injeção de Dependências centralizada & MockUserContext]
├── src/CartorioTcc.CrossCutting.Logging  [Logging Estruturado com Serilog]
├── src/CartorioTcc.WorkerService         [Processamento em Segundo Plano de Backups Criptografados (UC-03) e Fila DOI (UC-10)]
├── tests/CartorioTcc.UnitTests           [Testes Unitários: xUnit + NSubstitute + Bogus + FluentAssertions]
└── tests/CartorioTcc.IntegrationTests    [Testes de Integração: WebApplicationFactory + EF Core InMemory]
```

### Executando o Backend (.NET 10)
```powershell
cd C:\_GIT\Cartorio-TCC\sistema\api
dotnet run --project src/CartorioTcc.Api --urls "http://localhost:5000"
```
- **Swagger UI**: `http://localhost:5000/swagger`
- **Banco de Dados**: Por padrão em desenvolvimento inicia com seed automático dos dados reais dos casos de teste do TCC (*2º Tabelionato de Notas de Campinas*, *Daniel Luiz Granado*, *Vanderlei Minussi*, *Wemeson Carvalho*, *João Azevedo*, *Fabricio Lima dos Santos*, *Maria José* e protocolo `4119914266`). Para alternar para uma instância SQL Server dedicada, altere `"Database:UseInMemory": false` e configure `ConnectionStrings:DefaultConnection` em `src/CartorioTcc.Api/appsettings.json`.

---

## 2. Estrutura do Projeto Frontend (`sistema/app`)

```text
sistema/app/src/app/
├── core/               [RequestInterceptor, ErrorInterceptor, AuthService, TokenService, UsuarioService, authGuard (Bypass)]
├── layout/             [MainLayoutComponent: Header, Sidenav por Domínio/UC, Breadcrumb, Footer]
├── modules/
│   ├── area-publica/   [Login (preparado p/ JWT) e UC-12 Consulta Pública/Mobile de Andamento por Protocolo]
│   ├── area-restrita/  [Dashboard + Chart.js, UC-01 a UC-11 completos, Índice de Livros]
│   └── central-ajuda/  [Fluxos Antes vs. Depois do TCC, Manuais e FAQ]
└── shared/             [DTOs/Interfaces, Pipes CPF/CNPJ e Telefone, ConfirmDialogComponent]
```

### Executando o Frontend (Angular 21.2.23)
```powershell
cd C:\_GIT\Cartorio-TCC\sistema\app
npm start
```
- Acesse em `http://localhost:4200`.
