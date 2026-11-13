using CartorioTcc.Business.Builders;
using CartorioTcc.Business.Core.Interfaces;
using CartorioTcc.Business.Core.Models;
using CartorioTcc.Business.Core.Notifications;
using CartorioTcc.Business.Dtos;
using CartorioTcc.Business.Entities;
using CartorioTcc.Business.Enums;
using CartorioTcc.Business.Interfaces;
using CartorioTcc.Business.Resources;

namespace CartorioTcc.Business.Services;

/// <summary>
/// UC-06: Gerar Orçamento de Atos Notariais (simulação e persistência conforme Tabela de Custas CNB/SP).
/// </summary>
public sealed class OrcamentoService(
    INotificadorService notificador,
    IOrcamentoRepository repository,
    IOrcamentoQueryService queryService,
    IDateTimeProvider dateTimeProvider) : BaseService(notificador), IOrcamentoService
{
    public OrcamentoDto Simular(CalcularOrcamentoRequest request)
    {
        if (request.ValorBaseAto <= 0)
        {
            NotificarErro(Mensagens.ValorOrcamentoInvalido, nameof(request.ValorBaseAto));
            return new OrcamentoDto(0, request.IdCliente, null, 0, request.Oneroso, request.Hipoteca,
                request.Ufesp2500, request.Salarios30, request.CertidaoInclusa, request.OutrosValores,
                string.Empty, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, dateTimeProvider.Agora);
        }

        var entidade = TabelaCustasNotariaisCalculator.Calcular(request);
        return MapearDto(entidade);
    }

    public async Task<OrcamentoDto?> GerarESalvarAsync(CalcularOrcamentoRequest request, CancellationToken ct = default)
    {
        if (request.ValorBaseAto <= 0)
        {
            NotificarErro(Mensagens.ValorOrcamentoInvalido, nameof(request.ValorBaseAto));
            return null;
        }

        var entidade = TabelaCustasNotariaisCalculator.Calcular(request);
        repository.Adicionar(entidade);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.OrcamentoGeradoSucesso);
        return await queryService.ObterPorIdAsync(entidade.Id, ct) ?? MapearDto(entidade);
    }

    private OrcamentoDto MapearDto(Orcamento o) => new(
        o.Id,
        o.IdCliente,
        o.Cliente?.NomeCliente,
        o.ValorBaseAto,
        o.Oneroso,
        o.Hipoteca,
        o.Ufesp2500,
        o.Salarios30,
        o.CertidaoInclusa,
        o.OutrosValores,
        o.FaixaDescricao,
        o.EmolumentosTabelionato,
        o.EmolumentosEstado,
        o.ContribuicaoIpesp,
        o.FundoRegistroCivil,
        o.TaxaTribunalJustica,
        o.FundoMinisterioPublico,
        o.IssqnEstimado,
        o.ValorCertidao,
        o.PrevisaoRegistroImoveis,
        o.ValorTotalOrcamento,
        o.DtCriacao == default ? dateTimeProvider.Agora : o.DtCriacao);
}

/// <summary>
/// UC-07: Gerenciar Protocolo de Atendimento.
/// Regra do campo Número do Protocolo: preenchimento automático, formato alfanumérico limitado a 10 caracteres (ex.: 2026000001).
/// </summary>
public sealed class ProtocoloService(
    INotificadorService notificador,
    IProtocoloRepository repository,
    IClienteRepository clienteRepository,
    IProtocoloQueryService queryService,
    IDateTimeProvider dateTimeProvider) : BaseService(notificador), IProtocoloService
{
    public async Task<ProtocoloDto?> IncluirAsync(SalvarProtocoloCommand command, CancellationToken ct = default)
    {
        if (command.IdCliente <= 0)
        {
            NotificarErro(Mensagens.CamposObrigatorios, nameof(command.IdCliente));
            return null;
        }

        var cliente = await clienteRepository.ObterPorIdAsync(command.IdCliente, ct);
        if (cliente is null)
        {
            NotificarNaoEncontrado(Mensagens.ClienteNaoEncontrado);
            return null;
        }

        var ano = dateTimeProvider.Hoje.Year;
        var sequencial = await repository.ObterProximoSequencialAsync(ano, ct);
        var numeroProtocolo = $"{ano:D4}{sequencial:D6}";

        var protocolo = new Protocolo(
            numeroProtocolo,
            command.IdCliente,
            command.IdAto,
            command.IdFuncionario,
            command.IdOrcamento,
            command.NaturezaPedido,
            command.DocumentosApresentados,
            command.DocumentosFaltantes,
            command.ValorOrcamentoEstimado,
            command.Observacoes);

        repository.Adicionar(protocolo);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.ProtocoloGeradoSucesso);
        return await queryService.ObterPorIdAsync(protocolo.Id, ct);
    }

    public async Task<ProtocoloDto?> AlterarAsync(int id, SalvarProtocoloCommand command, CancellationToken ct = default)
    {
        if (command.IdCliente <= 0)
        {
            NotificarErro(Mensagens.CamposObrigatorios, nameof(command.IdCliente));
            return null;
        }

        var protocolo = await repository.ObterPorIdAsync(id, ct);
        if (protocolo is null)
        {
            NotificarNaoEncontrado(Mensagens.ProtocoloNaoEncontrado);
            return null;
        }

        protocolo.Atualizar(
            command.IdCliente,
            command.IdAto,
            command.IdFuncionario,
            command.IdOrcamento,
            command.NaturezaPedido,
            command.DocumentosApresentados,
            command.DocumentosFaltantes,
            command.ValorOrcamentoEstimado,
            command.Observacoes,
            command.StatusAndamento);

        repository.Atualizar(protocolo);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.ProtocoloAlteradoSucesso);
        return await queryService.ObterPorIdAsync(protocolo.Id, ct);
    }

    public async Task<bool> InativarAsync(int id, CancellationToken ct = default)
    {
        var protocolo = await repository.ObterPorIdAsync(id, ct);
        if (protocolo is null)
        {
            NotificarNaoEncontrado(Mensagens.ProtocoloNaoEncontrado);
            return false;
        }

        protocolo.Inativar();
        repository.Atualizar(protocolo);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.ProtocoloExcluidoSucesso);
        return true;
    }
}

/// <summary>
/// UC-09, UC-10 e UC-11: Gerenciar Escrituras, Digitalizações (PDF), Lavratura/Finalização,
/// Geração de Arquivo XML DOI e Emissão de Certidão em PDF.
/// </summary>
public sealed class EscrituraService(
    INotificadorService notificador,
    IEscrituraRepository escrituraRepository,
    IProtocoloRepository protocoloRepository,
    IClienteService clienteService,
    IEscrituraQueryService queryService,
    IDoiExportGenerator doiExportGenerator,
    IPdfGeneratorService pdfGeneratorService,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext) : BaseService(notificador), IEscrituraService
{
    public async Task<EscrituraDetalheDto?> IncluirAsync(SalvarEscrituraCommand command, CancellationToken ct = default)
    {
        if (command.IdCartorio <= 0 || command.IdProtocolo <= 0 || command.IdAto <= 0 || command.IdFuncionario <= 0)
        {
            NotificarErro(Mensagens.CamposObrigatorios);
            return null;
        }

        if (await escrituraRepository.ExisteParaProtocoloAsync(command.IdProtocolo, null, ct))
        {
            NotificarErro(Mensagens.EscrituraJaCadastradaParaProtocolo, nameof(command.IdProtocolo));
            return null;
        }

        var idOutorgante = await ResolverClienteAsync(command.IdOutorgante, command.NovoOutorgante, ct);
        if (!OperacaoValida || idOutorgante is null) return null;

        var idOutorgado = await ResolverClienteAsync(command.IdOutorgado, command.NovoOutorgado, ct);
        if (!OperacaoValida || idOutorgado is null) return null;

        if (idOutorgante.Value == idOutorgado.Value)
        {
            NotificarErro(Mensagens.OutorganteOutorgadoIguais);
            return null;
        }

        int? idImovel = null;
        if (command.Imovel is not null && !string.IsNullOrWhiteSpace(command.Imovel.NumeroRegistro))
        {
            var imovel = new Imovel(
                idOutorgante.Value,
                command.Imovel.NumeroRegistro,
                command.Imovel.ValorVenal,
                command.Imovel.NomePrefeitura,
                command.Imovel.DescricaoImovel,
                command.Imovel.ValorImovel,
                command.Imovel.Endereco.ParaDomain(),
                command.Imovel.AreaMetrosQuadrados,
                command.Imovel.InscricaoMunicipal,
                command.Imovel.CartorioRegistroImoveis);

            escrituraRepository.AdicionarImovel(imovel);
            await escrituraRepository.UnitOfWork.CommitAsync(ct);
            idImovel = imovel.Id;
        }

        int? idCertidaoOutroOrgao = null;
        if (command.CertidaoOutroOrgao is not null &&
            !string.IsNullOrWhiteSpace(command.CertidaoOutroOrgao.CartorioCertidaoOutroOrgao))
        {
            var certidao = new CertidaoOutroOrgao(
                command.CertidaoOutroOrgao.CartorioCertidaoOutroOrgao,
                command.CertidaoOutroOrgao.LivroCertidaoOutroOrgao,
                command.CertidaoOutroOrgao.FolhasCertidaoOutroOrgao,
                command.CertidaoOutroOrgao.TermoCertidaoOutroOrgao,
                command.CertidaoOutroOrgao.DataCertidaoOutroOrgao);

            escrituraRepository.AdicionarCertidaoOutroOrgao(certidao);
            await escrituraRepository.UnitOfWork.CommitAsync(ct);
            idCertidaoOutroOrgao = certidao.Id;
        }

        var seq = await escrituraRepository.ObterProximoSequencialEscrituraAsync(ct);
        var numeroLivro = command.NumeroLivro > 0 ? command.NumeroLivro : 1 + ((seq - 1) / 200);
        var folhaInicial = command.FolhaInicial > 0 ? command.FolhaInicial : 1 + (((seq - 1) % 200) * 2);
        var folhaFinal = command.FolhaFinal >= folhaInicial ? command.FolhaFinal : folhaInicial + 1;
        var numeroEscritura = $"ESC-{dateTimeProvider.Hoje.Year}-{seq:D5}";

        var escritura = new Escritura(
            command.IdCartorio,
            idOutorgante.Value,
            idOutorgado.Value,
            command.IdFuncionario,
            command.IdProtocolo,
            command.IdAto,
            idImovel,
            idCertidaoOutroOrgao,
            numeroEscritura,
            numeroLivro,
            folhaInicial,
            folhaFinal,
            command.ValorOperacao,
            command.TextoMinuta,
            dateTimeProvider.Agora,
            command.StatusEscritura == StatusEscritura.Inativa
                ? StatusEscritura.EmProcessoDeEscrituracao
                : command.StatusEscritura);

        if (command.StatusEscritura == StatusEscritura.Finalizada)
            escritura.FinalizarLavratura(dateTimeProvider.Agora, command.TextoMinuta);

        escrituraRepository.Adicionar(escritura);

        var protocolo = await protocoloRepository.ObterPorIdAsync(command.IdProtocolo, ct);
        protocolo?.AlterarStatusAndamento(escritura.StatusEscritura);

        await escrituraRepository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.EscrituraInseridaSucesso);
        return await queryService.ObterPorIdAsync(escritura.Id, ct);
    }

    public async Task<EscrituraDetalheDto?> AlterarAsync(int id, SalvarEscrituraCommand command, CancellationToken ct = default)
    {
        if (command.IdCartorio <= 0 || command.IdProtocolo <= 0 || command.IdAto <= 0 || command.IdFuncionario <= 0)
        {
            NotificarErro(Mensagens.CamposObrigatorios);
            return null;
        }

        var escritura = await escrituraRepository.ObterCompletaPorIdAsync(id, ct);
        if (escritura is null)
        {
            NotificarNaoEncontrado(Mensagens.EscrituraNaoEncontrada);
            return null;
        }

        if (escritura.EstaFinalizada)
        {
            NotificarErro(Mensagens.EscrituraFinalizadaImutavel);
            return null;
        }

        if (await escrituraRepository.ExisteParaProtocoloAsync(command.IdProtocolo, id, ct))
        {
            NotificarErro(Mensagens.EscrituraJaCadastradaParaProtocolo, nameof(command.IdProtocolo));
            return null;
        }

        var idOutorgante = await ResolverClienteAsync(command.IdOutorgante, command.NovoOutorgante, ct);
        if (!OperacaoValida || idOutorgante is null) return null;

        var idOutorgado = await ResolverClienteAsync(command.IdOutorgado, command.NovoOutorgado, ct);
        if (!OperacaoValida || idOutorgado is null) return null;

        if (idOutorgante.Value == idOutorgado.Value)
        {
            NotificarErro(Mensagens.OutorganteOutorgadoIguais);
            return null;
        }

        var idImovel = escritura.IdImovel;
        if (command.Imovel is not null && !string.IsNullOrWhiteSpace(command.Imovel.NumeroRegistro))
        {
            if (escritura.Imovel is null)
            {
                var novoImovel = new Imovel(
                    idOutorgante.Value,
                    command.Imovel.NumeroRegistro,
                    command.Imovel.ValorVenal,
                    command.Imovel.NomePrefeitura,
                    command.Imovel.DescricaoImovel,
                    command.Imovel.ValorImovel,
                    command.Imovel.Endereco.ParaDomain(),
                    command.Imovel.AreaMetrosQuadrados,
                    command.Imovel.InscricaoMunicipal,
                    command.Imovel.CartorioRegistroImoveis);
                escrituraRepository.AdicionarImovel(novoImovel);
                await escrituraRepository.UnitOfWork.CommitAsync(ct);
                idImovel = novoImovel.Id;
            }
            else
            {
                escritura.Imovel.Atualizar(
                    idOutorgante.Value,
                    command.Imovel.NumeroRegistro,
                    command.Imovel.ValorVenal,
                    command.Imovel.NomePrefeitura,
                    command.Imovel.DescricaoImovel,
                    command.Imovel.ValorImovel,
                    command.Imovel.Endereco.ParaDomain(),
                    command.Imovel.AreaMetrosQuadrados,
                    command.Imovel.InscricaoMunicipal,
                    command.Imovel.CartorioRegistroImoveis);
            }
        }

        var idCertidao = escritura.IdCertidaoOutroOrgao;
        if (command.CertidaoOutroOrgao is not null &&
            !string.IsNullOrWhiteSpace(command.CertidaoOutroOrgao.CartorioCertidaoOutroOrgao))
        {
            if (escritura.CertidaoOutroOrgao is null)
            {
                var novaCertidao = new CertidaoOutroOrgao(
                    command.CertidaoOutroOrgao.CartorioCertidaoOutroOrgao,
                    command.CertidaoOutroOrgao.LivroCertidaoOutroOrgao,
                    command.CertidaoOutroOrgao.FolhasCertidaoOutroOrgao,
                    command.CertidaoOutroOrgao.TermoCertidaoOutroOrgao,
                    command.CertidaoOutroOrgao.DataCertidaoOutroOrgao);
                escrituraRepository.AdicionarCertidaoOutroOrgao(novaCertidao);
                await escrituraRepository.UnitOfWork.CommitAsync(ct);
                idCertidao = novaCertidao.Id;
            }
            else
            {
                escritura.CertidaoOutroOrgao.Atualizar(
                    command.CertidaoOutroOrgao.CartorioCertidaoOutroOrgao,
                    command.CertidaoOutroOrgao.LivroCertidaoOutroOrgao,
                    command.CertidaoOutroOrgao.FolhasCertidaoOutroOrgao,
                    command.CertidaoOutroOrgao.TermoCertidaoOutroOrgao,
                    command.CertidaoOutroOrgao.DataCertidaoOutroOrgao);
            }
        }

        escritura.Atualizar(
            command.IdCartorio,
            idOutorgante.Value,
            idOutorgado.Value,
            command.IdFuncionario,
            command.IdProtocolo,
            command.IdAto,
            idImovel,
            idCertidao,
            command.NumeroLivro > 0 ? command.NumeroLivro : escritura.NumeroLivro,
            command.FolhaInicial > 0 ? command.FolhaInicial : escritura.FolhaInicial,
            command.FolhaFinal > 0 ? command.FolhaFinal : escritura.FolhaFinal,
            command.ValorOperacao,
            command.TextoMinuta,
            command.StatusEscritura);

        if (command.StatusEscritura == StatusEscritura.Finalizada)
            escritura.FinalizarLavratura(dateTimeProvider.Agora, command.TextoMinuta);

        escrituraRepository.Atualizar(escritura);

        var protocolo = await protocoloRepository.ObterPorIdAsync(command.IdProtocolo, ct);
        protocolo?.AlterarStatusAndamento(escritura.StatusEscritura);

        await escrituraRepository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.EscrituraAlteradaSucesso);
        return await queryService.ObterPorIdAsync(escritura.Id, ct);
    }

    public async Task<EscrituraDetalheDto?> FinalizarLavraturaAsync(FinalizarEscrituraCommand command, CancellationToken ct = default)
    {
        var escritura = await escrituraRepository.ObterCompletaPorIdAsync(command.IdEscritura, ct);
        if (escritura is null)
        {
            NotificarNaoEncontrado(Mensagens.EscrituraNaoEncontrada);
            return null;
        }

        if (escritura.EstaFinalizada)
        {
            NotificarErro(Mensagens.EscrituraFinalizadaImutavel);
            return null;
        }

        escritura.FinalizarLavratura(dateTimeProvider.Agora, command.TextoMinutaDefinitivo);
        escrituraRepository.Atualizar(escritura);

        var protocolo = await protocoloRepository.ObterPorIdAsync(escritura.IdProtocolo, ct);
        protocolo?.AlterarStatusAndamento(StatusEscritura.Finalizada);

        await escrituraRepository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.EscrituraFinalizadaSucesso);
        return await queryService.ObterPorIdAsync(escritura.Id, ct);
    }

    public async Task<bool> InativarAsync(InativarEscrituraCommand command, CancellationToken ct = default)
    {
        var escritura = await escrituraRepository.ObterPorIdAsync(command.IdEscritura, ct);
        if (escritura is null)
        {
            NotificarNaoEncontrado(Mensagens.EscrituraNaoEncontrada);
            return false;
        }

        if (escritura.EstaFinalizada)
        {
            NotificarErro(Mensagens.EscrituraFinalizadaImutavel);
            return false;
        }

        if (string.IsNullOrWhiteSpace(command.Motivo))
        {
            NotificarErro(Mensagens.MotivoInativacaoObrigatorio, nameof(command.Motivo));
            return false;
        }

        escritura.Inativar(command.Motivo);
        escrituraRepository.Atualizar(escritura);

        var protocolo = await protocoloRepository.ObterPorIdAsync(escritura.IdProtocolo, ct);
        protocolo?.AlterarStatusAndamento(StatusEscritura.Inativa);

        await escrituraRepository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.EscrituraExcluidaSucesso);
        return true;
    }

    public async Task<EscrituraDetalheDto?> RegistrarRetiradaDocumentosAsync(RegistrarRetiradaProtocoloCommand command, CancellationToken ct = default)
    {
        var escritura = await escrituraRepository.ObterPorIdAsync(command.IdEscritura, ct);
        if (escritura is null)
        {
            NotificarNaoEncontrado(Mensagens.EscrituraNaoEncontrada);
            return null;
        }

        escritura.RegistrarRetiradaDocumentos(dateTimeProvider.Agora, command.Observacoes);
        escrituraRepository.Atualizar(escritura);
        await escrituraRepository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso("Entrega/retirada de documentos registrada com sucesso.");
        return await queryService.ObterPorIdAsync(escritura.Id, ct);
    }

    public async Task<DocumentoDigitalizadoDto?> AnexarDocumentoPdfAsync(AnexarDocumentoCommand command, CancellationToken ct = default)
    {
        if (command.ConteudoPdf is null || command.ConteudoPdf.Length == 0 || string.IsNullOrWhiteSpace(command.NomeArquivo))
        {
            NotificarErro(Mensagens.CamposObrigatorios);
            return null;
        }

        if (!command.NomeArquivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            NotificarErro(Mensagens.ArquivoApenasPdf, nameof(command.NomeArquivo));
            return null;
        }

        var hash = GerarHashSha256(command.ConteudoPdf);
        var doc = new DocumentoDigitalizado(
            command.IdEscritura,
            command.IdCliente,
            command.TipoDocumento,
            command.NomeArquivo,
            "application/pdf",
            command.ConteudoPdf,
            hash,
            command.Observacao);

        escrituraRepository.AdicionarDocumento(doc);
        await escrituraRepository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso("Documento PDF anexado com sucesso.");
        return new DocumentoDigitalizadoDto(
            doc.Id,
            doc.IdEscritura,
            doc.IdCliente,
            null,
            null,
            doc.TipoDocumento,
            doc.TipoDocumento.ToString(),
            doc.NomeArquivo,
            doc.ContentType,
            doc.TamanhoBytes,
            doc.HashSha256,
            doc.Observacao,
            doc.DtCriacao);
    }

    public async Task<ArquivoGerado?> BaixarDocumentoPdfAsync(int idDocumento, CancellationToken ct = default)
    {
        var doc = await escrituraRepository.ObterDocumentoPorIdAsync(idDocumento, ct);
        if (doc is null)
        {
            NotificarNaoEncontrado("Documento digitalizado não encontrado.");
            return null;
        }

        return new ArquivoGerado(doc.NomeArquivo, doc.ContentType, doc.ConteudoPdf);
    }

    public async Task<bool> RemoverDocumentoPdfAsync(int idDocumento, CancellationToken ct = default)
    {
        var doc = await escrituraRepository.ObterDocumentoPorIdAsync(idDocumento, ct);
        if (doc is null)
        {
            NotificarNaoEncontrado("Documento digitalizado não encontrado.");
            return false;
        }

        escrituraRepository.RemoverDocumento(doc);
        await escrituraRepository.UnitOfWork.CommitAsync(ct);
        NotificarSucesso("Documento removido com sucesso.");
        return true;
    }

    /// <summary>
    /// UC-10: Gerar Arquivo para Envio (Declaração sobre Operações Imobiliárias - DOI em XML para a RFB).
    /// Regra de nome do arquivo: ano-mês-dia-nº de controle.
    /// </summary>
    public async Task<ArquivoGerado?> GerarArquivoDoiXmlAsync(string numeroProtocoloOuEscritura, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(numeroProtocoloOuEscritura))
        {
            NotificarErro(Mensagens.ProtocoloInvalido);
            return null;
        }

        var detalhe = await queryService.ObterPorProtocoloAsync(numeroProtocoloOuEscritura.Trim(), ct);
        if (detalhe is null)
        {
            NotificarNaoEncontrado(Mensagens.NenhumaEscrituraDisponivelDoi);
            return null;
        }

        if (detalhe.StatusEscritura != StatusEscritura.Finalizada)
        {
            NotificarErro("Somente escrituras finalizadas (lavradas) podem gerar o arquivo de envio DOI.");
            return null;
        }

        var entidade = await escrituraRepository.ObterPorIdAsync(detalhe.Id, ct);
        if (entidade is not null)
        {
            entidade.RegistrarEnvioDoi(dateTimeProvider.Agora);
            escrituraRepository.Atualizar(entidade);
            await escrituraRepository.UnitOfWork.CommitAsync(ct);
        }

        return doiExportGenerator.GerarXmlDoi(detalhe, dateTimeProvider.Agora);
    }

    /// <summary>
    /// UC-11: Gerar Certidões em PDF a partir do número do protocolo ou da escritura.
    /// </summary>
    public async Task<ArquivoGerado?> GerarCertidaoPdfAsync(string numeroProtocoloOuEscritura, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(numeroProtocoloOuEscritura))
        {
            NotificarErro(Mensagens.ProtocoloInvalido);
            return null;
        }

        var detalhe = await queryService.ObterPorProtocoloAsync(numeroProtocoloOuEscritura.Trim(), ct);
        if (detalhe is null)
        {
            NotificarNaoEncontrado(Mensagens.EscrituraNaoEncontrada);
            return null;
        }

        return pdfGeneratorService.GerarCertidaoEscrituraPdf(detalhe, dateTimeProvider.Agora, userContext.Nome);
    }

    public async Task<ArquivoGerado?> GerarProtocoloRetiradaPdfAsync(int idEscritura, CancellationToken ct = default)
    {
        var detalhe = await queryService.ObterPorIdAsync(idEscritura, ct);
        if (detalhe is null)
        {
            NotificarNaoEncontrado(Mensagens.EscrituraNaoEncontrada);
            return null;
        }

        return pdfGeneratorService.GerarProtocoloRetiradaPdf(detalhe);
    }

    private async Task<int?> ResolverClienteAsync(int? idClienteExistente, SalvarClienteCommand? novoCliente, CancellationToken ct)
    {
        if (idClienteExistente is > 0) return idClienteExistente.Value;

        if (novoCliente is not null)
        {
            var criado = await clienteService.IncluirAsync(novoCliente, ct);
            return criado?.Id;
        }

        NotificarErro(Mensagens.CamposObrigatorios);
        return null;
    }
}

/// <summary>
/// UC-03 (Agendar Backup Criptografado) e UC-05 (Gerar Manualmente Backup Criptografado).
/// </summary>
public sealed class BackupService(
    INotificadorService notificador,
    IBackupRepository repository,
    IBackupCriptografadoProvider criptografiaProvider,
    IRelatorioQueryService queryService,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext) : BaseService(notificador), IBackupService
{
    public async Task<BackupDto?> AgendarBackupAsync(AgendarBackupCommand command, CancellationToken ct = default)
    {
        if (command.DataAgendada.Date <= dateTimeProvider.Hoje.ToDateTime(TimeOnly.MinValue).Date)
        {
            NotificarErro(Mensagens.BackupDataDeveSerMaiorQueHoje, nameof(command.DataAgendada));
            return null;
        }

        var descricao = string.IsNullOrWhiteSpace(command.DescricaoBackup)
            ? $"Backup Criptografado Agendado para {command.DataAgendada:dd/MM/yyyy HH:mm}"
            : command.DescricaoBackup.Trim();

        var pacote = await criptografiaProvider.GerarPacoteBackupCriptografadoAsync(descricao, ct);

        var registro = new BackupRegistro(
            dateTimeProvider.Agora,
            command.DataAgendada,
            descricao,
            command.IdFuncionario ?? userContext.FuncionarioId,
            manual: false,
            executado: true,
            pacote.NomeArquivo,
            pacote.ConteudoCriptografado.LongLength,
            pacote.HashSha256,
            pacote.ConteudoCriptografado);

        repository.Adicionar(registro);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.BackupRealizadoSucesso);
        var todos = await queryService.ListarBackupsAsync(ct);
        return todos.FirstOrDefault(b => b.Id == registro.Id);
    }

    public async Task<BackupDto?> RealizarBackupManualAsync(RealizarBackupManualCommand command, CancellationToken ct = default)
    {
        var descricao = string.IsNullOrWhiteSpace(command.DescricaoBackup)
            ? $"Backup Criptografado Manual ({dateTimeProvider.Agora:dd/MM/yyyy HH:mm:ss})"
            : command.DescricaoBackup.Trim();

        var pacote = await criptografiaProvider.GerarPacoteBackupCriptografadoAsync(descricao, ct);

        var registro = new BackupRegistro(
            dateTimeProvider.Agora,
            null,
            descricao,
            command.IdFuncionario ?? userContext.FuncionarioId,
            manual: true,
            executado: true,
            pacote.NomeArquivo,
            pacote.ConteudoCriptografado.LongLength,
            pacote.HashSha256,
            pacote.ConteudoCriptografado);

        repository.Adicionar(registro);
        await repository.UnitOfWork.CommitAsync(ct);

        NotificarSucesso(Mensagens.BackupRealizadoSucesso);
        var todos = await queryService.ListarBackupsAsync(ct);
        return todos.FirstOrDefault(b => b.Id == registro.Id);
    }

    public async Task<int> ProcessarBackupsAgendadosAsync(CancellationToken ct = default)
    {
        var pendentes = await repository.ObterAgendadosPendentesAsync(dateTimeProvider.Agora, ct);
        if (pendentes.Count == 0) return 0;

        foreach (var item in pendentes)
        {
            var pacote = await criptografiaProvider.GerarPacoteBackupCriptografadoAsync(item.DescricaoBackup, ct);
            item.MarcarExecutado(
                dateTimeProvider.Agora,
                pacote.NomeArquivo,
                pacote.ConteudoCriptografado.LongLength,
                pacote.HashSha256,
                pacote.ConteudoCriptografado);
            repository.Atualizar(item);
        }

        await repository.UnitOfWork.CommitAsync(ct);
        return pendentes.Count;
    }

    public async Task<ArquivoGerado?> BaixarBackupCriptografadoAsync(int idBackup, CancellationToken ct = default)
    {
        var registro = await repository.ObterPorIdAsync(idBackup, ct);
        if (registro is null || registro.ConteudoCriptografado is null)
        {
            NotificarNaoEncontrado("Registro de backup criptografado não encontrado.");
            return null;
        }

        return new ArquivoGerado(registro.NomeArquivoGerado, "application/octet-stream", registro.ConteudoCriptografado);
    }
}
