using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CartorioTcc.Business.Builders;
using CartorioTcc.Business.Core.ValueObjects;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Entities;
using CartorioTcc.Business.Enums;
using CartorioTcc.Infra.Data.Context;

namespace CartorioTcc.Infra.Data.Seed;

public static class CartorioTccSeeder
{
    public static async Task SeedAsync(CartorioTccContext context, ILogger logger)
    {
        try
        {
            if (context.Database.IsRelational())
            {
                var conn = context.Database.GetDbConnection();
                logger.LogInformation(
                    "Conectando ao SQL Server ({DataSource} / Base: {Database}) e aplicando Migrations...",
                    conn.DataSource,
                    conn.Database);

                await context.Database.MigrateAsync();

                // Auto-correção resiliente caso Historico_Auditoria tenha sido criado anteriormente com BIGINT
                await context.Database.ExecuteSqlRawAsync(@"
IF EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.types t ON c.system_type_id = t.system_type_id
    WHERE c.object_id = OBJECT_ID(N'dbo.Historico_Auditoria')
      AND c.name = N'Id_Auditoria'
      AND t.name = N'bigint'
)
BEGIN
    DROP TABLE [dbo].[Historico_Auditoria];
    CREATE TABLE [dbo].[Historico_Auditoria] (
        [Id_Auditoria] INT NOT NULL IDENTITY(1,1),
        [Tabela] NVARCHAR(100) NOT NULL,
        [Chave_Primaria] NVARCHAR(60) NOT NULL,
        [Acao_Auditoria] INT NOT NULL,
        [Login_Usuario] NVARCHAR(80) NOT NULL,
        [Data_Ocorrencia] DATETIME2 NOT NULL,
        [Valores_Antigos_Json] NVARCHAR(MAX) NULL,
        [Valores_Novos_Json] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Historico_Auditoria] PRIMARY KEY ([Id_Auditoria])
    );
END");
            }
            else
            {
                logger.LogInformation("Utilizando provedor EF Core InMemory ({Provider}).", context.Database.ProviderName);
                await context.Database.EnsureCreatedAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao executar MigrateAsync; tentando EnsureCreatedAsync.");
            await context.Database.EnsureCreatedAsync();
        }

        if (await context.Perfis.AnyAsync())
            return;

        logger.LogInformation("Populando base de dados inicial do SAEC (2º Tabelionato de Notas de Campinas - TCC 2015)...");

        // 1. Cartório
        var cartorio = new Cartorio(
            "2º Tabelionato de Notas de Campinas",
            "46017488000185",
            "Daniel Luiz Granado (Escrevente Autorizado) / Wemeson Carvalho (Tabelião)",
            "Rua General Osório, 971 - Centro, Campinas/SP - CEP 13010-111",
            "1932314455",
            "contato@2notas-campinas.not.br",
            "11.542-8");
        context.Cartorios.Add(cartorio);

        // 2. Perfis (UC-01)
        var perfilAdmin = new Perfil("Administrador", "Acesso irrestrito a todas as funcionalidades e configurações do SAEC");
        var perfilTabeliao = new Perfil("Tabelião", "Gestão notarial completa, lavratura, relatórios gerenciais e DOI");
        var perfilEscrevente = new Perfil("Escrevente", "Conferência, lavratura de escrituras, clientes, orçamentos e certidões");
        var perfilAuxiliar = new Perfil("Auxiliar de Cartório", "Recepção, protocolos, atendimento inicial, orçamentos e digitalizações");
        context.Perfis.AddRange(perfilAdmin, perfilTabeliao, perfilEscrevente, perfilAuxiliar);

        // 3. Páginas (UC-01)
        var paginas = new List<Pagina>
        {
            new("Dashboard Notarial", "/app/dashboard", "Geral"),
            new("Gerenciar Permissões (UC-01)", "/app/permissoes", "Administração"),
            new("Gerenciar Funcionários (UC-02)", "/app/funcionarios", "Administração"),
            new("Backups Criptografados (UC-03 / UC-05)", "/app/backups", "Administração"),
            new("Relatórios Gerenciais (UC-04)", "/app/relatorios", "Gerencial"),
            new("Gerar Orçamento CNB/SP (UC-06)", "/app/orcamentos", "Atendimento"),
            new("Gerenciar Protocolos (UC-07)", "/app/protocolos", "Atendimento"),
            new("Gerenciar Clientes (UC-08)", "/app/clientes", "Escrituração"),
            new("Gerenciar Escrituras (UC-09)", "/app/escrituras", "Escrituração"),
            new("Índice de Livros Notariais", "/app/indice-livros", "Escrituração"),
            new("Exportar Arquivo DOI - RFB (UC-10)", "/app/envio-doi", "Obrigações Fiscais"),
            new("Emitir Certidões (UC-11)", "/app/certidoes", "Atendimento"),
            new("Consultar Andamento (UC-12)", "/consulta-andamento", "Público")
        };
        context.Paginas.AddRange(paginas);

        // 4. Atos Notariais
        var atoCompraVenda = new Ato("Compra e Venda", "Escritura Pública de Compra e Venda de Imóvel Urbano", null, true, true);
        var atoDoacao = new Ato("Doação", "Escritura Pública de Doação de Bem Imóvel", null, true, true);
        var atoPermuta = new Ato("Permuta", "Escritura Pública de Permuta entre Bens Imóveis", null, true, true);
        var atoInventario = new Ato("Inventário e Partilha", "Escritura Pública de Inventário e Partilha Extrajudicial", null, true, true);
        var atoHipoteca = new Ato("Confissão de Dívida com Hipoteca", "Escritura Pública de Confissão de Dívida com Garantia Hipotecária", null, true, true);
        var atoDeclaratoria = new Ato("Declaratória", "Escritura Pública Declaratória sem Valor Declarado", null, false, false);
        context.Atos.AddRange(atoCompraVenda, atoDoacao, atoPermuta, atoInventario, atoHipoteca, atoDeclaratoria);

        // 5. Catálogo de Tipos de Relatórios (UC-04)
        context.TiposRelatorios.AddRange(
            new TipoRelatorio(TipoRelatorioEnum.EscriturasCriadas, "Relatório de Escrituras Criadas", "Listagem analítica de escrituras lavradas e em andamento no período"),
            new TipoRelatorio(TipoRelatorioEnum.ProtocolosAtendimento, "Relatório de Protocolos de Atendimento", "Controle de protocolos abertos, em escrituração e concluídos"),
            new TipoRelatorio(TipoRelatorioEnum.IndiceLivroNotas, "Relatório de Índice de Livros e Notas", "Índice cronológico e alfabético de livros e folhas lavradas"),
            new TipoRelatorio(TipoRelatorioEnum.ClientesCadastrados, "Relatório de Clientes Cadastrados", "Relação de clientes, cônjuges e estado civil cadastrados no SAEC"),
            new TipoRelatorio(TipoRelatorioEnum.OrcamentosRealizados, "Relatório Financeiro de Orçamentos CNB/SP", "Demonstrativo de emolumentos e orçamentos simulados/emitidos"),
            new TipoRelatorio(TipoRelatorioEnum.AuditoriaSistema, "Relatório de Auditoria e Backups Criptografados", "Trilha de auditoria e histórico de backups AES-256")
        );

        await context.SaveChangesAsync();

        // 6. Permissões por Perfil (UC-01)
        foreach (var pag in paginas)
        {
            context.PaginasPerfis.Add(new PaginaPerfil(perfilAdmin.Id, pag.Id, true, true, true, true));
            var tabPermitido = pag.Modulo != "Administração" || pag.NomePagina.Contains("Funcionários");
            context.PaginasPerfis.Add(new PaginaPerfil(perfilTabeliao.Id, pag.Id, tabPermitido, tabPermitido, tabPermitido, tabPermitido));
            var escPermitido = pag.Modulo is "Geral" or "Atendimento" or "Escrituração" or "Obrigações Fiscais" or "Público";
            context.PaginasPerfis.Add(new PaginaPerfil(perfilEscrevente.Id, pag.Id, escPermitido, escPermitido, escPermitido, false));
            var auxPermitido = pag.Modulo is "Geral" or "Atendimento" or "Público" || pag.NomePagina.Contains("Clientes");
            context.PaginasPerfis.Add(new PaginaPerfil(perfilAuxiliar.Id, pag.Id, auxPermitido, auxPermitido, false, false));
        }

        // 7. Funcionários (Seção 4.4 e Seção 6.3 do TCC)
        static string HashSenha(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));

        var funcAdmin = new Funcionario(
            perfilAdmin.Id,
            "Administrador SAEC",
            new DateOnly(1985, 5, 10),
            new DateOnly(2015, 1, 10),
            "admin@saec.com",
            HashSenha("Asd@123"),
            "52998224725",
            "34123456X",
            "1932314455",
            "Administrador de Sistemas",
            new Endereco { Cep = "13010111", Pais = "Brasil", Estado = "SP", Cidade = "Campinas", Bairro = "Centro", Logradouro = "Rua General Osório", Numero = "971", Complemento = "Sala 1" });

        var funcDaniel = new Funcionario(
            perfilEscrevente.Id,
            "Daniel Luiz Granado",
            new DateOnly(1982, 8, 22),
            new DateOnly(2012, 3, 15),
            "daniel.granado@saec.com",
            HashSenha("Asd@123"),
            "11144477735",
            "289911223",
            "1932314455",
            "Escrevente Autorizado",
            new Endereco { Cep = "13015900", Pais = "Brasil", Estado = "SP", Cidade = "Campinas", Bairro = "Centro", Logradouro = "Avenida Anchieta", Numero = "510", Complemento = "Apto 42" });

        var funcVanderlei = new Funcionario(
            perfilAuxiliar.Id,
            "Vanderlei Minussi",
            new DateOnly(1986, 10, 12),
            new DateOnly(2015, 2, 1),
            "vanderlei@saec.com",
            HashSenha("123456"),
            "12345678909",
            "41122233X",
            "1938871122",
            "Auxiliar de Cartório",
            new Endereco { Cep = "13183091", Pais = "Brasil", Estado = "SP", Cidade = "Hortolândia", Bairro = "Jardim Nova Hortolândia", Logradouro = "Rua Antonio Carlos", Numero = "120" });

        var funcWemeson = new Funcionario(
            perfilTabeliao.Id,
            "Wemeson Carvalho",
            new DateOnly(1979, 4, 18),
            new DateOnly(2010, 6, 1),
            "wemeson@saec.com",
            HashSenha("123456"),
            "98765432100",
            "254448881",
            "1932314455",
            "Tabelião",
            new Endereco { Cep = "13010111", Pais = "Brasil", Estado = "SP", Cidade = "Campinas", Bairro = "Centro", Logradouro = "Rua General Osório", Numero = "971", Complemento = "2º Andar" });

        context.Funcionarios.AddRange(funcAdmin, funcDaniel, funcVanderlei, funcWemeson);
        await context.SaveChangesAsync();

        // 8. Clientes dos Casos de Teste do TCC (Seção 4.4 - CT-08 e CT-09)
        var endJoao = new Endereco { Cep = "13060245", Pais = "Brasil", Estado = "SP", Cidade = "Campinas", Bairro = "Vila União", Logradouro = "Rua 2", Numero = "110", Complemento = "Casa" };
        var cliJoaoAzevedo = new Cliente(
            "João Azevedo",
            "41223344",
            "SSP",
            "Brasileiro(a)",
            "SP",
            "39053344705",
            new DateOnly(1980, 3, 14),
            "joao.azevedo@email.com",
            "M",
            "Casado(a)",
            "Comunhão Parcial de Bens",
            "Engenheiro Civil",
            "1932214455",
            endJoao);

        var endFabricio = new Endereco { Cep = "13060725", Pais = "Brasil", Estado = "SP", Cidade = "Campinas", Bairro = "Jardim Yeda", Logradouro = "Rua Moacyr Lacerda", Numero = "350" };
        var cliFabricioLima = new Cliente(
            "Fabricio Lima dos Santos",
            "47889900",
            "SSP",
            "Brasileiro(a)",
            "SP",
            "85267617420",
            new DateOnly(1991, 7, 25),
            "fabricio.lima@email.com",
            "M",
            "Casado(a)",
            "Comunhão Parcial de Bens",
            "Analista de Sistemas",
            "1932667788",
            endFabricio);

        var endMaria = new Endereco { Cep = "13183091", Pais = "Brasil", Estado = "SP", Cidade = "Hortolândia", Bairro = "Jardim Nova Hortolândia", Logradouro = "Rua Antonio Carlos", Numero = "85" };
        var cliMariaJose = new Cliente(
            "Maria José",
            "19887766",
            "SSP",
            "Brasileiro(a)",
            "SP",
            "15350946056",
            new DateOnly(1975, 1, 30),
            "maria.jose@email.com",
            "F",
            "Casado(a)",
            "Comunhão Universal de Bens",
            "Professora",
            "1938874411",
            endMaria);

        context.Clientes.AddRange(cliJoaoAzevedo, cliFabricioLima, cliMariaJose);
        await context.SaveChangesAsync();

        // Cônjuges e Certidões de Casamento
        var conjJoao = new Conjuge(cliJoaoAzevedo.Id, "Maria Joana Azevedo", "42334455", "SSP", "Brasileiro(a)", "SP", "71428793860", new DateOnly(1982, 6, 10), "maria.joana@email.com", "F", "Casado(a)", "Arquiteta", "1932214455", endJoao.Clonar());
        var certJoao = new CertidaoCasamento(cliJoaoAzevedo.Id, null, "2º Cartório de Registro Civil de Campinas", "12", "45", "1089", new DateOnly(2008, 5, 20), "Comunhão Parcial de Bens");
        cliJoaoAzevedo.DefinirConjuge(conjJoao);
        cliJoaoAzevedo.DefinirCertidaoCasamento(certJoao);

        var conjFabricio = new Conjuge(cliFabricioLima.Id, "Jessica Correa Lima", "48990011", "SSP", "Brasileiro(a)", "SP", "28625587887", new DateOnly(1992, 9, 12), "jessica.lima@email.com", "F", "Casado(a)", "Administradora", "1932667788", endFabricio.Clonar());
        var certFabricio = new CertidaoCasamento(cliFabricioLima.Id, null, "1º Cartório de Registro Civil de Campinas", "8", "112", "3402", new DateOnly(2014, 11, 15), "Comunhão Parcial de Bens");
        cliFabricioLima.DefinirConjuge(conjFabricio);
        cliFabricioLima.DefinirCertidaoCasamento(certFabricio);

        var conjMaria = new Conjuge(cliMariaJose.Id, "Raimundo José", "18776655", "SSP", "Brasileiro(a)", "SP", "60516604016", new DateOnly(1973, 4, 22), "raimundo.jose@email.com", "M", "Casado(a)", "Comerciante", "1938874411", endMaria.Clonar());
        var certMaria = new CertidaoCasamento(cliMariaJose.Id, null, "2º Cartório de Notas de Campinas", "1", "23", "4512", new DateOnly(2001, 9, 10), "Comunhão Universal de Bens");
        cliMariaJose.DefinirConjuge(conjMaria);
        cliMariaJose.DefinirCertidaoCasamento(certMaria);

        await context.SaveChangesAsync();

        // 9. Orçamentos (UC-06 - CT-06: Fabricio Lima, R$ 500.000,00)
        var orcamento1 = TabelaCustasNotariaisCalculator.Calcular(
            new CalcularOrcamentoRequest(500_000m, cliFabricioLima.Id, true, false, true, false, true, 0m));

        var orcamento2 = TabelaCustasNotariaisCalculator.Calcular(
            new CalcularOrcamentoRequest(320_000m, cliMariaJose.Id, true, false, false, false, true, 120m));

        context.Orcamentos.AddRange(orcamento1, orcamento2);
        await context.SaveChangesAsync();

        // 10. Protocolos (UC-07 / UC-12 - incluindo protocolo oficial do TCC: 4119914266)
        var prot1 = new Protocolo(
            "4119914266",
            cliFabricioLima.Id,
            atoCompraVenda.Id,
            funcDaniel.Id,
            orcamento1.Id,
            "Compra e Venda Residencial",
            "RG, CPF, Certidão de Casamento, Certidão de Matrícula nº 2235, Guia ITBI quitada",
            null,
            orcamento1.ValorTotalOrcamento,
            "Documentação completa conferida. Escritura lavrada no Livro 1, Folhas 1 a 3.");
        prot1.AlterarStatusAndamento(StatusEscritura.Finalizada);

        var prot2 = new Protocolo(
            "2026000002",
            cliMariaJose.Id,
            atoCompraVenda.Id,
            funcDaniel.Id,
            orcamento2.Id,
            "Compra e Venda de Apartamento",
            "RG, CPF, Certidão de Casamento, Matrícula nº 18940",
            "Certidão Negativa de Tributos Municipais",
            orcamento2.ValorTotalOrcamento,
            "Minuta em elaboração pelo escrevente responsável.");
        prot2.AlterarStatusAndamento(StatusEscritura.EmProcessoDeEscrituracao);

        var prot3 = new Protocolo(
            "2026000003",
            cliJoaoAzevedo.Id,
            atoDoacao.Id,
            funcVanderlei.Id,
            null,
            "Escritura de Doação com Reserva de Usufruto",
            "RG e CPF das partes",
            "Certidão atualizada do Registro de Imóveis (30 dias)",
            2_850.00m,
            "Aguardando apresentação de certidão atualizada do imóvel.");
        prot3.AlterarStatusAndamento(StatusEscritura.AguardandoDocumentacao);

        context.Protocolos.AddRange(prot1, prot2, prot3);
        await context.SaveChangesAsync();

        // 11. Imóveis e Escrituras (UC-09 - CT-09: Livro 1, Folha 1, Av. Prestes Maia 241)
        var imovel1 = new Imovel(
            cliJoaoAzevedo.Id,
            "2235",
            400_000m,
            "Prefeitura Municipal de Campinas",
            "Um prédio residencial situado na Avenida Prestes Maia nº 241, Jardim do Trevo, Campinas/SP, com área construída de 185,00m² e terreno de 300,00m².",
            500_000m,
            new Endereco { Cep = "13040000", Pais = "Brasil", Estado = "SP", Cidade = "Campinas", Bairro = "Jardim do Trevo", Logradouro = "Avenida Prestes Maia", Numero = "241" },
            300m,
            "3411220099",
            "2º Cartório de Registro de Imóveis de Campinas");

        var imovel2 = new Imovel(
            cliMariaJose.Id,
            "18940",
            280_000m,
            "Prefeitura Municipal de Campinas",
            "Apartamento nº 81 do Edifício Solar do Centro, com 98,00m² de área útil e 1 vaga de garagem.",
            320_000m,
            new Endereco { Cep = "13010111", Pais = "Brasil", Estado = "SP", Cidade = "Campinas", Bairro = "Centro", Logradouro = "Rua General Osório", Numero = "550", Complemento = "Apto 81" },
            98m,
            "1100445588",
            "1º Cartório de Registro de Imóveis de Campinas");

        var certidaoOutro1 = new CertidaoOutroOrgao(
            "2º Cartório de Registro de Imóveis de Campinas",
            "2",
            "2235",
            "8891",
            DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));

        context.Imoveis.AddRange(imovel1, imovel2);
        context.CertidoesOutrosOrgaos.Add(certidaoOutro1);
        await context.SaveChangesAsync();

        var minuta1 = EscrituraMinutaBuilder.GerarTextoMinuta(
            cartorio, atoCompraVenda, prot1, funcDaniel,
            cliJoaoAzevedo, cliFabricioLima, imovel1, certidaoOutro1,
            1, 1, 3, 500_000m, DateTime.Now.AddDays(-3));

        var escritura1 = new Escritura(
            cartorio.Id,
            cliJoaoAzevedo.Id,
            cliFabricioLima.Id,
            funcDaniel.Id,
            prot1.Id,
            atoCompraVenda.Id,
            imovel1.Id,
            certidaoOutro1.Id,
            "2026-L0001-F001",
            1,
            1,
            3,
            500_000m,
            minuta1,
            DateTime.Now.AddDays(-5),
            StatusEscritura.Finalizada);
        escritura1.FinalizarLavratura(DateTime.Now.AddDays(-2), minuta1);
        escritura1.RegistrarEnvioDoi(DateTime.Now.AddDays(-1));

        var minuta2 = EscrituraMinutaBuilder.GerarTextoMinuta(
            cartorio, atoCompraVenda, prot2, funcDaniel,
            cliMariaJose, cliFabricioLima, imovel2, null,
            1, 4, 6, 320_000m, DateTime.Now.AddDays(-1));

        var escritura2 = new Escritura(
            cartorio.Id,
            cliMariaJose.Id,
            cliFabricioLima.Id,
            funcDaniel.Id,
            prot2.Id,
            atoCompraVenda.Id,
            imovel2.Id,
            null,
            "2026-L0001-F004",
            1,
            4,
            6,
            320_000m,
            minuta2,
            DateTime.Now.AddDays(-2),
            StatusEscritura.EmProcessoDeEscrituracao);

        context.Escrituras.AddRange(escritura1, escritura2);
        await context.SaveChangesAsync();

        // 12. Documento Digitalizado PDF de exemplo (UC-08 / UC-09)
        var samplePdfBytes = Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj<<>>endobj\ntrailer<<>>\n%%EOF");
        var sampleHash = Convert.ToHexString(SHA256.HashData(samplePdfBytes));
        var docDigitalizado = new DocumentoDigitalizado(
            escritura1.Id,
            cliFabricioLima.Id,
            TipoDocumentoDigitalizado.DocumentosApresentados,
            "Certidao_Matricula_2235_RG_CPF.pdf",
            "application/pdf",
            samplePdfBytes,
            sampleHash,
            "Matrícula 2235 e documentos pessoais digitalizados dos outorgantes e outorgados");
        context.DocumentosDigitalizados.Add(docDigitalizado);

        // 13. Registro de Backup Inicial Criptografado (UC-03 / UC-05)
        var backupInicial = new BackupRegistro(
            DateTime.Now.AddHours(-6),
            DateTime.Now.AddHours(-6),
            "Backup automático diário criptografado AES-256 (Acervo SAEC)",
            funcAdmin.Id,
            false,
            true,
            "SAEC_BACKUP_INICIAL_20261113.saec.enc",
            samplePdfBytes.LongLength,
            sampleHash,
            samplePdfBytes);
        context.Backups.Add(backupInicial);

        await context.SaveChangesAsync();
        logger.LogInformation("Seed inicial do SAEC concluído com sucesso.");
    }
}
