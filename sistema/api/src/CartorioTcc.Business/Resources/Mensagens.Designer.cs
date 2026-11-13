#nullable enable
using System.Globalization;
using System.Resources;

namespace CartorioTcc.Business.Resources;

/// <summary>
/// Centralizador tipado de mensagens de negócio a partir de <c>Mensagens.resx</c>.
/// </summary>
public static class Mensagens
{
    private static ResourceManager? _resourceMan;

    public static ResourceManager ResourceManager =>
        _resourceMan ??= new ResourceManager("CartorioTcc.Business.Resources.Mensagens", typeof(Mensagens).Assembly);

    public static CultureInfo? Culture { get; set; }

    private static string Get(string name) =>
        ResourceManager.GetString(name, Culture) ?? name;

    public static string CamposObrigatorios => Get(nameof(CamposObrigatorios));
    public static string PermissaoAlteradaSucesso => Get(nameof(PermissaoAlteradaSucesso));
    public static string PermissaoIncluidaSucesso => Get(nameof(PermissaoIncluidaSucesso));
    public static string PermissaoExcluidaSucesso => Get(nameof(PermissaoExcluidaSucesso));
    public static string PermissaoNaoEncontrada => Get(nameof(PermissaoNaoEncontrada));
    public static string PermissaoDuplicada => Get(nameof(PermissaoDuplicada));
    public static string FuncionarioInseridoSucesso => Get(nameof(FuncionarioInseridoSucesso));
    public static string FuncionarioAlteradoSucesso => Get(nameof(FuncionarioAlteradoSucesso));
    public static string FuncionarioExcluidoSucesso => Get(nameof(FuncionarioExcluidoSucesso));
    public static string FuncionarioNaoEncontrado => Get(nameof(FuncionarioNaoEncontrado));
    public static string FuncionarioJaCadastrado => Get(nameof(FuncionarioJaCadastrado));
    public static string ClienteInseridoSucesso => Get(nameof(ClienteInseridoSucesso));
    public static string ClienteAlteradoSucesso => Get(nameof(ClienteAlteradoSucesso));
    public static string ClienteExcluidoSucesso => Get(nameof(ClienteExcluidoSucesso));
    public static string ClienteNaoEncontrado => Get(nameof(ClienteNaoEncontrado));
    public static string ClienteJaCadastrado => Get(nameof(ClienteJaCadastrado));
    public static string ClienteMenorDeIdade => Get(nameof(ClienteMenorDeIdade));
    public static string CpfInvalido => Get(nameof(CpfInvalido));
    public static string CnpjInvalido => Get(nameof(CnpjInvalido));
    public static string NomeInvalido => Get(nameof(NomeInvalido));
    public static string CepInvalido => Get(nameof(CepInvalido));
    public static string TelefoneInvalido => Get(nameof(TelefoneInvalido));
    public static string EmailInvalido => Get(nameof(EmailInvalido));
    public static string SenhaInvalida => Get(nameof(SenhaInvalida));
    public static string SenhasNaoConferem => Get(nameof(SenhasNaoConferem));
    public static string ValorOrcamentoInvalido => Get(nameof(ValorOrcamentoInvalido));
    public static string OrcamentoGeradoSucesso => Get(nameof(OrcamentoGeradoSucesso));
    public static string ProtocoloGeradoSucesso => Get(nameof(ProtocoloGeradoSucesso));
    public static string ProtocoloAlteradoSucesso => Get(nameof(ProtocoloAlteradoSucesso));
    public static string ProtocoloExcluidoSucesso => Get(nameof(ProtocoloExcluidoSucesso));
    public static string ProtocoloNaoEncontrado => Get(nameof(ProtocoloNaoEncontrado));
    public static string ProtocoloInvalido => Get(nameof(ProtocoloInvalido));
    public static string EscrituraInseridaSucesso => Get(nameof(EscrituraInseridaSucesso));
    public static string EscrituraAlteradaSucesso => Get(nameof(EscrituraAlteradaSucesso));
    public static string EscrituraFinalizadaSucesso => Get(nameof(EscrituraFinalizadaSucesso));
    public static string EscrituraExcluidaSucesso => Get(nameof(EscrituraExcluidaSucesso));
    public static string EscrituraNaoEncontrada => Get(nameof(EscrituraNaoEncontrada));
    public static string EscrituraJaCadastradaParaProtocolo => Get(nameof(EscrituraJaCadastradaParaProtocolo));
    public static string EscrituraFinalizadaImutavel => Get(nameof(EscrituraFinalizadaImutavel));
    public static string MotivoInativacaoObrigatorio => Get(nameof(MotivoInativacaoObrigatorio));
    public static string OutorganteOutorgadoIguais => Get(nameof(OutorganteOutorgadoIguais));
    public static string NenhumaEscrituraDisponivelDoi => Get(nameof(NenhumaEscrituraDisponivelDoi));
    public static string SelecioneEscrituraAntesDeGerar => Get(nameof(SelecioneEscrituraAntesDeGerar));
    public static string ArquivoApenasPdf => Get(nameof(ArquivoApenasPdf));
    public static string BackupRealizadoSucesso => Get(nameof(BackupRealizadoSucesso));
    public static string BackupAgendadoSucesso => Get(nameof(BackupAgendadoSucesso));
    public static string BackupDataDeveSerMaiorQueHoje => Get(nameof(BackupDataDeveSerMaiorQueHoje));
}
