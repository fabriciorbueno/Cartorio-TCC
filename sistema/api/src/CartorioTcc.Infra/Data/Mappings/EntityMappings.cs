using CartorioTcc.Business.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartorioTcc.Infra.Data.Mappings;

public sealed class AtoMapping : AuditableEntityMapping<Ato>
{
    public override void Configure(EntityTypeBuilder<Ato> builder)
    {
        base.Configure(builder);
        builder.ToTable("Atos");
        builder.Property(x => x.Id).HasColumnName("Id_Ato");
        builder.Property(x => x.TipoAto).HasColumnName("Tipo_Ato").HasMaxLength(80).IsRequired();
        builder.Property(x => x.DescricaoAto).HasColumnName("Descricao_Ato").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ModeloMinutaPadrao).HasColumnName("Modelo_Minuta_Padrao").HasMaxLength(4000);
        builder.Property(x => x.ExigeImovel).HasColumnName("Exige_Imovel").IsRequired();
        builder.Property(x => x.GeraDoi).HasColumnName("Gera_Doi").IsRequired();
        builder.Property(x => x.Ativo).HasColumnName("Ativo").IsRequired();
    }
}

public sealed class CartorioMapping : AuditableEntityMapping<Cartorio>
{
    public override void Configure(EntityTypeBuilder<Cartorio> builder)
    {
        base.Configure(builder);
        builder.ToTable("Cartorios");
        builder.Property(x => x.Id).HasColumnName("Id_Cartorio");
        builder.Property(x => x.NomeCartorio).HasColumnName("Nome_Cartorio").HasMaxLength(120).IsRequired();
        builder.Property(x => x.CnpjCartorio).HasColumnName("CNPJ_Cartorio").HasMaxLength(18).IsRequired();
        builder.Property(x => x.TabeliaoResponsavel).HasColumnName("Tabeliao_Responsavel").HasMaxLength(120);
        builder.Property(x => x.EnderecoCompleto).HasColumnName("Endereco_Completo").HasMaxLength(250);
        builder.Property(x => x.Telefone).HasColumnName("Telefone").HasMaxLength(25);
        builder.Property(x => x.Email).HasColumnName("Email").HasMaxLength(100);
        builder.Property(x => x.CodigoCns).HasColumnName("Codigo_CNS").HasMaxLength(20);
    }
}

public sealed class PerfilMapping : AuditableEntityMapping<Perfil>
{
    public override void Configure(EntityTypeBuilder<Perfil> builder)
    {
        base.Configure(builder);
        builder.ToTable("Perfis");
        builder.Property(x => x.Id).HasColumnName("Id_Perfil");
        builder.Property(x => x.NomePerfil).HasColumnName("Nome_Perfil").HasMaxLength(60).IsRequired();
        builder.Property(x => x.Descricao).HasColumnName("Descricao_Perfil").HasMaxLength(250);
        builder.Property(x => x.Status).HasColumnName("Id_Status_Perfil").HasConversion<int>();
    }
}

public sealed class PaginaMapping : AuditableEntityMapping<Pagina>
{
    public override void Configure(EntityTypeBuilder<Pagina> builder)
    {
        base.Configure(builder);
        builder.ToTable("Paginas");
        builder.Property(x => x.Id).HasColumnName("Id_Pagina");
        builder.Property(x => x.NomePagina).HasColumnName("Nome_Pagina").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Url).HasColumnName("URL").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Modulo).HasColumnName("Modulo").HasMaxLength(60);
    }
}

public sealed class PaginaPerfilMapping : AuditableEntityMapping<PaginaPerfil>
{
    public override void Configure(EntityTypeBuilder<PaginaPerfil> builder)
    {
        base.Configure(builder);
        builder.ToTable("Paginas_Perfis");
        builder.Property(x => x.Id).HasColumnName("Id_Pagina_Perfil");
        builder.Property(x => x.IdPerfil).HasColumnName("Id_Perfil").IsRequired();
        builder.Property(x => x.IdPagina).HasColumnName("Id_Pagina").IsRequired();
        builder.Property(x => x.PodeVisualizar).HasColumnName("Pode_Visualizar").IsRequired();
        builder.Property(x => x.PodeIncluir).HasColumnName("Pode_Incluir").IsRequired();
        builder.Property(x => x.PodeEditar).HasColumnName("Pode_Editar").IsRequired();
        builder.Property(x => x.PodeExcluir).HasColumnName("Pode_Excluir").IsRequired();
        builder.Property(x => x.Status).HasColumnName("Id_Status_Perfil").HasConversion<int>();

        builder.HasOne(x => x.Perfil)
            .WithMany(p => p.PaginasPerfis)
            .HasForeignKey(x => x.IdPerfil)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Pagina)
            .WithMany()
            .HasForeignKey(x => x.IdPagina)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.IdPerfil, x.IdPagina }).IsUnique();
    }
}

public sealed class FuncionarioMapping : AuditableEntityMapping<Funcionario>
{
    public override void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        base.Configure(builder);
        builder.ToTable("Funcionarios");
        builder.Property(x => x.Id).HasColumnName("Id_Funcionario");
        builder.Property(x => x.IdPerfil).HasColumnName("Id_Perfil").IsRequired();
        builder.Property(x => x.NomeFuncionario).HasColumnName("Nome_Funcionario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DataNascFuncionario).HasColumnName("Data_Nasc_Funcionario");
        builder.Property(x => x.DataAdmissao).HasColumnName("Data_Admissao_Funcionario");
        builder.Property(x => x.EmailFuncionario).HasColumnName("Email_Funcionario").HasMaxLength(100).IsRequired();
        builder.Property(x => x.SenhaHash).HasColumnName("Senha_Funcionario").HasMaxLength(128).IsRequired();
        builder.Property(x => x.CpfFuncionario).HasColumnName("CPF_Funcionario").HasMaxLength(14).IsRequired();
        builder.Property(x => x.RgFuncionario).HasColumnName("RG_Funcionario").HasMaxLength(30).IsRequired();
        builder.Property(x => x.TelefoneFuncionario).HasColumnName("Telefone_Funcionario").HasMaxLength(20);
        builder.Property(x => x.Cargo).HasColumnName("Cargo_Funcionario").HasMaxLength(60);
        builder.Property(x => x.StatusFuncionario).HasColumnName("Id_Status_Funcionario").HasConversion<int>();

        builder.OwnsOne(x => x.Endereco, end =>
        {
            end.Property(p => p.Cep).HasColumnName("CEP_Funcionario").HasMaxLength(8);
            end.Property(p => p.Pais).HasColumnName("Pais_Funcionario").HasMaxLength(40);
            end.Property(p => p.Estado).HasColumnName("Estado_Funcionario").HasMaxLength(2);
            end.Property(p => p.Cidade).HasColumnName("Cidade_Funcionario").HasMaxLength(60);
            end.Property(p => p.Bairro).HasColumnName("Bairro_Funcionario").HasMaxLength(60);
            end.Property(p => p.Logradouro).HasColumnName("Logradouro_Funcionario").HasMaxLength(120);
            end.Property(p => p.Numero).HasColumnName("Numero_Funcionario").HasMaxLength(20);
            end.Property(p => p.Complemento).HasColumnName("Complemento_Funcionario").HasMaxLength(60);
        });

        builder.HasOne(x => x.Perfil)
            .WithMany()
            .HasForeignKey(x => x.IdPerfil)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CpfFuncionario).IsUnique();
    }
}

public sealed class ClienteMapping : AuditableEntityMapping<Cliente>
{
    public override void Configure(EntityTypeBuilder<Cliente> builder)
    {
        base.Configure(builder);
        builder.ToTable("Clientes");
        builder.Property(x => x.Id).HasColumnName("Id_Cliente");
        builder.Property(x => x.NomeCliente).HasColumnName("Nome_Cliente").HasMaxLength(100).IsRequired();
        builder.Property(x => x.RgCliente).HasColumnName("RG_Clientes").HasMaxLength(30).IsRequired();
        builder.Property(x => x.OrgaoExpedidorCliente).HasColumnName("Orgao_Expedidor_Cliente").HasMaxLength(20).IsRequired();
        builder.Property(x => x.NacionalidadeCliente).HasColumnName("Nacionalidade_Cliente").HasMaxLength(40).IsRequired();
        builder.Property(x => x.EstadoOrgaoExpedidorCliente).HasColumnName("Estado_Orgao_Expedidor_Cliente").HasMaxLength(2).IsRequired();
        builder.Property(x => x.CpfCliente).HasColumnName("CPF_Cliente").HasMaxLength(14).IsRequired();
        builder.Property(x => x.DataNascCliente).HasColumnName("Data_Nasc_Cliente").IsRequired();
        builder.Property(x => x.EmailCliente).HasColumnName("Email_Cliente").HasMaxLength(100).IsRequired();
        builder.Property(x => x.GeneroCliente).HasColumnName("Genero_Cliente").HasMaxLength(1).IsRequired();
        builder.Property(x => x.EstadoCivilCliente).HasColumnName("Estado_Civil_Cliente").HasMaxLength(30).IsRequired();
        builder.Property(x => x.RegimeBens).HasColumnName("Regime_Bens_Cliente").HasMaxLength(60);
        builder.Property(x => x.ProfissaoCliente).HasColumnName("Profissao_Cliente").HasMaxLength(60).IsRequired();
        builder.Property(x => x.TelefoneCliente).HasColumnName("Telefone_Cliente").HasMaxLength(20).IsRequired();
        builder.Property(x => x.StatusCliente).HasColumnName("Id_Status_Cliente").HasConversion<int>();

        builder.OwnsOne(x => x.Endereco, end =>
        {
            end.Property(p => p.Cep).HasColumnName("CEP_Cliente").HasMaxLength(8);
            end.Property(p => p.Pais).HasColumnName("Pais_Cliente").HasMaxLength(40);
            end.Property(p => p.Estado).HasColumnName("Estado_Cliente").HasMaxLength(2);
            end.Property(p => p.Cidade).HasColumnName("Cidade_Cliente").HasMaxLength(60);
            end.Property(p => p.Bairro).HasColumnName("Bairro_Cliente").HasMaxLength(60);
            end.Property(p => p.Logradouro).HasColumnName("Logradouro_Cliente").HasMaxLength(120);
            end.Property(p => p.Numero).HasColumnName("Numero_Cliente").HasMaxLength(20);
            end.Property(p => p.Complemento).HasColumnName("Complemento_Cliente").HasMaxLength(60);
        });

        builder.HasOne(x => x.Conjuge)
            .WithOne(c => c.Cliente)
            .HasForeignKey<Conjuge>(c => c.IdCliente)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CertidaoCasamento)
            .WithOne(cc => cc.Cliente)
            .HasForeignKey<CertidaoCasamento>(cc => cc.IdCliente)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CpfCliente).IsUnique();
    }
}

public sealed class ConjugeMapping : AuditableEntityMapping<Conjuge>
{
    public override void Configure(EntityTypeBuilder<Conjuge> builder)
    {
        base.Configure(builder);
        builder.ToTable("Conjuges");
        builder.Property(x => x.Id).HasColumnName("Id_Conjuge");
        builder.Property(x => x.IdCliente).HasColumnName("Id_Cliente").IsRequired();
        builder.Property(x => x.NomeConjuge).HasColumnName("Nome_Conjuge").HasMaxLength(100).IsRequired();
        builder.Property(x => x.RgConjuge).HasColumnName("RG_Conjuge").HasMaxLength(30).IsRequired();
        builder.Property(x => x.OrgaoExpedidorConjuge).HasColumnName("Orgao_Expedidor_Conjuge").HasMaxLength(20).IsRequired();
        builder.Property(x => x.NacionalidadeConjuge).HasColumnName("Nacionalidade_Conjuge").HasMaxLength(40).IsRequired();
        builder.Property(x => x.EstadoOrgaoExpedidorConjuge).HasColumnName("Estado_Orgao_Expedidor_Conjuge").HasMaxLength(2).IsRequired();
        builder.Property(x => x.CpfConjuge).HasColumnName("CPF_Conjuge").HasMaxLength(14).IsRequired();
        builder.Property(x => x.DataNascConjuge).HasColumnName("Data_Nasc_Conjuge").IsRequired();
        builder.Property(x => x.EmailConjuge).HasColumnName("Email_Conjuge").HasMaxLength(100);
        builder.Property(x => x.GeneroConjuge).HasColumnName("Genero_Conjuge").HasMaxLength(1);
        builder.Property(x => x.EstadoCivilConjuge).HasColumnName("Estado_Civil_Conjuge").HasMaxLength(30);
        builder.Property(x => x.ProfissaoConjuge).HasColumnName("Profissao_Conjuge").HasMaxLength(60);
        builder.Property(x => x.TelefoneConjuge).HasColumnName("Telefone_Conjuge").HasMaxLength(20);
        builder.Property(x => x.StatusConjuge).HasColumnName("Id_Status_Cliente").HasConversion<int>();

        builder.OwnsOne(x => x.Endereco, end =>
        {
            end.Property(p => p.Cep).HasColumnName("CEP_Conjuge").HasMaxLength(8);
            end.Property(p => p.Pais).HasColumnName("Pais_Conjuge").HasMaxLength(40);
            end.Property(p => p.Estado).HasColumnName("Estado_Conjuge").HasMaxLength(2);
            end.Property(p => p.Cidade).HasColumnName("Cidade_Conjuge").HasMaxLength(60);
            end.Property(p => p.Bairro).HasColumnName("Bairro_Conjuge").HasMaxLength(60);
            end.Property(p => p.Logradouro).HasColumnName("Logradouro_Conjuge").HasMaxLength(120);
            end.Property(p => p.Numero).HasColumnName("Numero_Conjuge").HasMaxLength(20);
            end.Property(p => p.Complemento).HasColumnName("Complemento_Conjuge").HasMaxLength(60);
        });
    }
}

public sealed class CertidaoCasamentoMapping : AuditableEntityMapping<CertidaoCasamento>
{
    public override void Configure(EntityTypeBuilder<CertidaoCasamento> builder)
    {
        base.Configure(builder);
        builder.ToTable("Certidoes_Casamentos");
        builder.Property(x => x.Id).HasColumnName("Id_Certidao_Casamento");
        builder.Property(x => x.IdCliente).HasColumnName("Id_Cliente").IsRequired();
        builder.Property(x => x.IdConjuge).HasColumnName("Id_Conjuge");
        builder.Property(x => x.CartorioCertidaoCasamento).HasColumnName("Cartorio_Certidao_Casamento").HasMaxLength(120).IsRequired();
        builder.Property(x => x.LivroCertidaoCasamento).HasColumnName("Livro_Certidao_Casamento").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FolhasCertidaoCasamento).HasColumnName("Folhas_Certidao_Casamento").HasMaxLength(50).IsRequired();
        builder.Property(x => x.TermoCertidaoCasamento).HasColumnName("Termo_Certidao_Casamento").HasMaxLength(50).IsRequired();
        builder.Property(x => x.DataCertidaoCasamento).HasColumnName("Data_Certidao_Casamento").IsRequired();
        builder.Property(x => x.Averbacoes).HasColumnName("Averbacoes").HasMaxLength(500);

        builder.HasOne(x => x.Conjuge)
            .WithMany()
            .HasForeignKey(x => x.IdConjuge)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CertidaoOutroOrgaoMapping : AuditableEntityMapping<CertidaoOutroOrgao>
{
    public override void Configure(EntityTypeBuilder<CertidaoOutroOrgao> builder)
    {
        base.Configure(builder);
        builder.ToTable("Certidoes_Outros_Orgaos");
        builder.Property(x => x.Id).HasColumnName("Id_Certidao_Outro_Orgao");
        builder.Property(x => x.CartorioCertidaoOutroOrgao).HasColumnName("Cartorio_Certidao_Outro_Orgao").HasMaxLength(120).IsRequired();
        builder.Property(x => x.LivroCertidaoOutroOrgao).HasColumnName("Livro_Certidao_Outro_Orgao").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FolhasCertidaoOutroOrgao).HasColumnName("Folhas_Certidao_Outro_Orgao").HasMaxLength(50).IsRequired();
        builder.Property(x => x.TermoCertidaoOutroOrgao).HasColumnName("Termo_Certidao_Outro_Orgao").HasMaxLength(50).IsRequired();
        builder.Property(x => x.DataCertidaoOutroOrgao).HasColumnName("Data_Certidao_Outro_Orgao").IsRequired();
    }
}

public sealed class ImovelMapping : AuditableEntityMapping<Imovel>
{
    public override void Configure(EntityTypeBuilder<Imovel> builder)
    {
        base.Configure(builder);
        builder.ToTable("Imoveis");
        builder.Property(x => x.Id).HasColumnName("Id_Imovel");
        builder.Property(x => x.IdCliente).HasColumnName("Id_Cliente");
        builder.Property(x => x.NumeroRegistro).HasColumnName("Numero_Registro").HasMaxLength(40).IsRequired();
        builder.Property(x => x.ValorVenal).HasColumnName("Valor_Venal").HasPrecision(17, 2);
        builder.Property(x => x.NomePrefeitura).HasColumnName("Nome_Prefeitura").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DescricaoImovel).HasColumnName("Descricao_Imovel").HasMaxLength(2000);
        builder.Property(x => x.ValorImovel).HasColumnName("Valor_Imovel").HasPrecision(17, 2);
        builder.Property(x => x.AreaMetrosQuadrados).HasColumnName("Area_M2").HasPrecision(14, 2);
        builder.Property(x => x.ValorPorMetroQuadrado).HasColumnName("Valor_Por_M2").HasPrecision(17, 2);
        builder.Property(x => x.InscricaoMunicipal).HasColumnName("Inscricao_Municipal").HasMaxLength(50);
        builder.Property(x => x.CartorioRegistroImoveis).HasColumnName("Cartorio_Registro_Imoveis").HasMaxLength(120);

        builder.OwnsOne(x => x.Endereco, end =>
        {
            end.Property(p => p.Cep).HasColumnName("CEP_Imovel").HasMaxLength(8);
            end.Property(p => p.Pais).HasColumnName("Pais_Imovel").HasMaxLength(40);
            end.Property(p => p.Estado).HasColumnName("Estado_Imovel").HasMaxLength(2);
            end.Property(p => p.Cidade).HasColumnName("Cidade_Imovel").HasMaxLength(60);
            end.Property(p => p.Bairro).HasColumnName("Bairro_Imovel").HasMaxLength(60);
            end.Property(p => p.Logradouro).HasColumnName("Logradouro_Imovel").HasMaxLength(120);
            end.Property(p => p.Numero).HasColumnName("Numero_Imovel").HasMaxLength(20);
            end.Property(p => p.Complemento).HasColumnName("Complemento_Imovel").HasMaxLength(60);
        });

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OrcamentoMapping : AuditableEntityMapping<Orcamento>
{
    public override void Configure(EntityTypeBuilder<Orcamento> builder)
    {
        base.Configure(builder);
        builder.ToTable("Orcamentos");
        builder.Property(x => x.Id).HasColumnName("Id_Orcamento");
        builder.Property(x => x.IdCliente).HasColumnName("Id_Cliente");
        builder.Property(x => x.ValorBaseAto).HasColumnName("Valor_Orcamento").HasPrecision(17, 2);
        builder.Property(x => x.Oneroso).HasColumnName("Oneroso_Orcamento");
        builder.Property(x => x.Hipoteca).HasColumnName("Hipoteca_Orcamento");
        builder.Property(x => x.Ufesp2500).HasColumnName("UFESP2500_Orcamento");
        builder.Property(x => x.Salarios30).HasColumnName("Salarios30_Orcamento");
        builder.Property(x => x.CertidaoInclusa).HasColumnName("Certidao_Orcamento");
        builder.Property(x => x.OutrosValores).HasColumnName("Outros_Orcamento").HasPrecision(17, 2);
        builder.Property(x => x.FaixaDescricao).HasColumnName("Faixa_Descricao").HasMaxLength(120);
        builder.Property(x => x.EmolumentosTabelionato).HasColumnName("Emolumentos_Tabelionato").HasPrecision(17, 2);
        builder.Property(x => x.EmolumentosEstado).HasColumnName("Emolumentos_Estado").HasPrecision(17, 2);
        builder.Property(x => x.ContribuicaoIpesp).HasColumnName("Contribuicao_IPESP").HasPrecision(17, 2);
        builder.Property(x => x.FundoRegistroCivil).HasColumnName("Fundo_Registro_Civil").HasPrecision(17, 2);
        builder.Property(x => x.TaxaTribunalJustica).HasColumnName("Taxa_Tribunal_Justica").HasPrecision(17, 2);
        builder.Property(x => x.FundoMinisterioPublico).HasColumnName("Fundo_Ministerio_Publico").HasPrecision(17, 2);
        builder.Property(x => x.IssqnEstimado).HasColumnName("ISSQN_Estimado").HasPrecision(17, 2);
        builder.Property(x => x.ValorCertidao).HasColumnName("Valor_Certidao").HasPrecision(17, 2);
        builder.Property(x => x.PrevisaoRegistroImoveis).HasColumnName("Previsao_Registro_Imoveis").HasPrecision(17, 2);
        builder.Property(x => x.ValorTotalOrcamento).HasColumnName("Valor_Total_Orcamento").HasPrecision(17, 2);

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProtocoloMapping : AuditableEntityMapping<Protocolo>
{
    public override void Configure(EntityTypeBuilder<Protocolo> builder)
    {
        base.Configure(builder);
        builder.ToTable("Protocolos");
        builder.Property(x => x.Id).HasColumnName("Id_Protocolo");
        builder.Property(x => x.NumeroProtocolo).HasColumnName("Numero_Protocolo").HasMaxLength(20).IsRequired();
        builder.Property(x => x.IdCliente).HasColumnName("Id_Cliente").IsRequired();
        builder.Property(x => x.IdAto).HasColumnName("Id_Ato");
        builder.Property(x => x.IdFuncionario).HasColumnName("Id_Funcionario");
        builder.Property(x => x.IdOrcamento).HasColumnName("Id_Orcamento");
        builder.Property(x => x.NaturezaPedido).HasColumnName("Natureza_Pedido").HasMaxLength(150);
        builder.Property(x => x.DocumentosApresentados).HasColumnName("Documentos_Apresentados").HasMaxLength(1000);
        builder.Property(x => x.DocumentosFaltantes).HasColumnName("Documentos_Faltantes").HasMaxLength(1000);
        builder.Property(x => x.ValorOrcamentoEstimado).HasColumnName("Valor_Orcamento_Estimado").HasPrecision(17, 2);
        builder.Property(x => x.Observacoes).HasColumnName("Observacoes").HasMaxLength(1000);
        builder.Property(x => x.StatusAndamento).HasColumnName("Id_Status_Escritura").HasConversion<int>();
        builder.Property(x => x.StatusProtocolo).HasColumnName("Id_Status_Protocolo").HasConversion<int>();

        builder.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Ato).WithMany().HasForeignKey(x => x.IdAto).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Funcionario).WithMany().HasForeignKey(x => x.IdFuncionario).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Orcamento).WithMany().HasForeignKey(x => x.IdOrcamento).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.NumeroProtocolo).IsUnique();
    }
}

public sealed class EscrituraMapping : AuditableEntityMapping<Escritura>
{
    public override void Configure(EntityTypeBuilder<Escritura> builder)
    {
        base.Configure(builder);
        builder.ToTable("Escrituras");
        builder.Property(x => x.Id).HasColumnName("Id_Escritura");
        builder.Property(x => x.IdCartorio).HasColumnName("Id_Cartorio").IsRequired();
        builder.Property(x => x.IdOutorgante).HasColumnName("Id_Outorgante").IsRequired();
        builder.Property(x => x.IdOutorgado).HasColumnName("Id_Outorgado").IsRequired();
        builder.Property(x => x.IdFuncionario).HasColumnName("Id_Funcionario").IsRequired();
        builder.Property(x => x.IdProtocolo).HasColumnName("Id_Protocolo").IsRequired();
        builder.Property(x => x.IdAto).HasColumnName("Id_Ato").IsRequired();
        builder.Property(x => x.IdImovel).HasColumnName("Id_Imovel");
        builder.Property(x => x.IdCertidaoOutroOrgao).HasColumnName("Id_Certidao_Outro_Orgao");
        builder.Property(x => x.NumeroEscritura).HasColumnName("Numero_Escritura").HasMaxLength(30).IsRequired();
        builder.Property(x => x.NumeroLivro).HasColumnName("Numero_Livro");
        builder.Property(x => x.FolhaInicial).HasColumnName("Folha_Inicial");
        builder.Property(x => x.FolhaFinal).HasColumnName("Folha_Final");
        builder.Property(x => x.ValorOperacao).HasColumnName("Valor_Operacao").HasPrecision(17, 2);
        builder.Property(x => x.TextoMinuta).HasColumnName("Texto_Minuta");
        builder.Property(x => x.StatusEscritura).HasColumnName("Id_Status_Escritura").HasConversion<int>();
        builder.Property(x => x.MotivoInativacao).HasColumnName("Motivo_Inativacao").HasMaxLength(500);
        builder.Property(x => x.DataInicioProcesso).HasColumnName("Data_Inicio_Processo");
        builder.Property(x => x.DataLavratura).HasColumnName("Data_Lavratura");
        builder.Property(x => x.DataFimProcesso).HasColumnName("Data_Fim_Processo");
        builder.Property(x => x.DataEnvioDoi).HasColumnName("Data_Envio_DOI");
        builder.Property(x => x.DocumentosRetiradosPeloCliente).HasColumnName("Documentos_Retirados");
        builder.Property(x => x.DataRetiradaDocumentos).HasColumnName("Data_Retirada_Documentos");
        builder.Property(x => x.ObservacoesRetirada).HasColumnName("Observacoes_Retirada").HasMaxLength(500);

        builder.HasOne(x => x.Cartorio).WithMany().HasForeignKey(x => x.IdCartorio).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Outorgante).WithMany().HasForeignKey(x => x.IdOutorgante).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Outorgado).WithMany().HasForeignKey(x => x.IdOutorgado).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Funcionario).WithMany().HasForeignKey(x => x.IdFuncionario).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Protocolo).WithMany().HasForeignKey(x => x.IdProtocolo).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Ato).WithMany().HasForeignKey(x => x.IdAto).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Imovel).WithMany().HasForeignKey(x => x.IdImovel).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CertidaoOutroOrgao).WithMany().HasForeignKey(x => x.IdCertidaoOutroOrgao).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class DocumentoDigitalizadoMapping : AuditableEntityMapping<DocumentoDigitalizado>
{
    public override void Configure(EntityTypeBuilder<DocumentoDigitalizado> builder)
    {
        base.Configure(builder);
        builder.ToTable("Documentos_Digitalizados");
        builder.Property(x => x.Id).HasColumnName("Id_Documento");
        builder.Property(x => x.IdEscritura).HasColumnName("Id_Escritura");
        builder.Property(x => x.IdCliente).HasColumnName("Id_Cliente");
        builder.Property(x => x.TipoDocumento).HasColumnName("Tipo_Documento").HasConversion<int>();
        builder.Property(x => x.NomeArquivo).HasColumnName("Nome_Arquivo").HasMaxLength(180).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("Content_Type").HasMaxLength(80).IsRequired();
        builder.Property(x => x.TamanhoBytes).HasColumnName("Tamanho_Bytes");
        builder.Property(x => x.HashSha256).HasColumnName("Hash_SHA256").HasMaxLength(64);
        builder.Property(x => x.ConteudoPdf).HasColumnName("Conteudo_PDF").IsRequired();
        builder.Property(x => x.Observacao).HasColumnName("Observacao").HasMaxLength(300);

        builder.HasOne(x => x.Escritura)
            .WithMany(e => e.Documentos)
            .HasForeignKey(x => x.IdEscritura)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class BackupRegistroMapping : AuditableEntityMapping<BackupRegistro>
{
    public override void Configure(EntityTypeBuilder<BackupRegistro> builder)
    {
        base.Configure(builder);
        builder.ToTable("Backups");
        builder.Property(x => x.Id).HasColumnName("Id_Backup");
        builder.Property(x => x.DataBackup).HasColumnName("Data_Backup").IsRequired();
        builder.Property(x => x.DataAgendada).HasColumnName("Data_Agendada");
        builder.Property(x => x.DescricaoBackup).HasColumnName("Descricao_Backup").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.IdFuncionario).HasColumnName("Id_Funcionario");
        builder.Property(x => x.Manual).HasColumnName("Manual");
        builder.Property(x => x.Criptografado).HasColumnName("Criptografado");
        builder.Property(x => x.Executado).HasColumnName("Executado");
        builder.Property(x => x.NomeArquivoGerado).HasColumnName("Nome_Arquivo_Gerado").HasMaxLength(150);
        builder.Property(x => x.TamanhoBytes).HasColumnName("Tamanho_Bytes");
        builder.Property(x => x.HashIntegridade).HasColumnName("Hash_Integridade").HasMaxLength(64);
        builder.Property(x => x.ConteudoCriptografado).HasColumnName("Conteudo_Criptografado");

        builder.HasOne(x => x.Funcionario)
            .WithMany()
            .HasForeignKey(x => x.IdFuncionario)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class TipoRelatorioMapping : AuditableEntityMapping<TipoRelatorio>
{
    public override void Configure(EntityTypeBuilder<TipoRelatorio> builder)
    {
        base.Configure(builder);
        builder.ToTable("Tipos_Relatorios");
        builder.Property(x => x.Id).HasColumnName("Id_Relatorio");
        builder.Property(x => x.Codigo).HasColumnName("Codigo_Relatorio").HasConversion<int>();
        builder.Property(x => x.NomeTipoRelatorio).HasColumnName("Tipo_Relatorios").HasMaxLength(80).IsRequired();
        builder.Property(x => x.DescricaoRelatorio).HasColumnName("Descricao_Relatorio").HasMaxLength(1000).IsRequired();
    }
}

public sealed class HistoricoAuditoriaMapping : IEntityTypeConfiguration<HistoricoAuditoria>
{
    public void Configure(EntityTypeBuilder<HistoricoAuditoria> builder)
    {
        builder.ToTable("Historico_Auditoria");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id_Auditoria").ValueGeneratedOnAdd();
        builder.Property(x => x.Tabela).HasColumnName("Tabela").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ChavePrimaria).HasColumnName("Chave_Primaria").HasMaxLength(60).IsRequired();
        builder.Property(x => x.Acao).HasColumnName("Acao_Auditoria").HasConversion<int>();
        builder.Property(x => x.LoginUsuario).HasColumnName("Login_Usuario").HasMaxLength(80).IsRequired();
        builder.Property(x => x.DataOcorrencia).HasColumnName("Data_Ocorrencia").IsRequired();
        builder.Property(x => x.ValoresAntigosJson).HasColumnName("Valores_Antigos_Json");
        builder.Property(x => x.ValoresNovosJson).HasColumnName("Valores_Novos_Json");
    }
}
