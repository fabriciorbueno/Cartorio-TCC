import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import {
  AndamentoProcessoPublicoDto,
  ApiResponse,
  AtoDto,
  BackupDto,
  CalcularOrcamentoRequest,
  CartorioDto,
  CepConsultaDto,
  ClienteDetalheDto,
  ClienteResumoDto,
  DashboardResumoDto,
  DocumentoDigitalizadoDto,
  EscrituraDetalheDto,
  EscrituraResumoDto,
  FaixaCustasDto,
  FuncionarioDetalheDto,
  FuncionarioResumoDto,
  IndiceEscrituraDto,
  OpcaoDto,
  OrcamentoDto,
  PaginaDto,
  PerfilDto,
  PermissaoDto,
  ProtocoloDto,
  RelatorioResultadoDto,
  ResultadoPaginado,
  TipoDocumentoDigitalizado,
  TipoRelatorioEnum
} from '../../shared/models/saec.models';

@Injectable({ providedIn: 'root' })
export class SaecApiService {
  private readonly http = inject(HttpClient);
  readonly baseUrl = 'http://localhost:5000/api';

  // --- Catálogos e CEP ---
  listarPerfis(): Observable<PerfilDto[]> {
    return this.http.get<ApiResponse<PerfilDto[]>>(`${this.baseUrl}/catalogos/perfis`).pipe(
      map(r => r.dados),
      catchError(() => of<PerfilDto[]>([
        { id: 1, nomePerfil: 'Administrador', descricao: 'Acesso irrestrito a todas as funcionalidades do SAEC', status: 'Ativo' },
        { id: 2, nomePerfil: 'Tabelião', descricao: 'Gestão notarial completa, lavratura, relatórios e DOI', status: 'Ativo' },
        { id: 3, nomePerfil: 'Escrevente', descricao: 'Conferência, lavratura de escrituras, clientes, orçamentos e certidões', status: 'Ativo' },
        { id: 4, nomePerfil: 'Auxiliar de Cartório', descricao: 'Recepção, protocolos, atendimento inicial e digitalizações', status: 'Ativo' }
      ]))
    );
  }

  listarPaginas(): Observable<PaginaDto[]> {
    return this.http.get<ApiResponse<PaginaDto[]>>(`${this.baseUrl}/catalogos/paginas`).pipe(
      map(r => r.dados),
      catchError(() => of([
        { id: 1, nomePagina: 'Dashboard Notarial', url: '/app/dashboard', modulo: 'Geral' },
        { id: 2, nomePagina: 'Gerenciar Permissões (UC-01)', url: '/app/permissoes', modulo: 'Administração' },
        { id: 3, nomePagina: 'Gerenciar Funcionários (UC-02)', url: '/app/funcionarios', modulo: 'Administração' },
        { id: 4, nomePagina: 'Backups Criptografados (UC-03 / UC-05)', url: '/app/backups', modulo: 'Administração' },
        { id: 5, nomePagina: 'Relatórios Gerenciais (UC-04)', url: '/app/relatorios', modulo: 'Gerencial' },
        { id: 6, nomePagina: 'Gerar Orçamento CNB/SP (UC-06)', url: '/app/orcamentos', modulo: 'Atendimento' },
        { id: 7, nomePagina: 'Gerenciar Protocolos (UC-07)', url: '/app/protocolos', modulo: 'Atendimento' },
        { id: 8, nomePagina: 'Gerenciar Clientes (UC-08)', url: '/app/clientes', modulo: 'Escrituração' },
        { id: 9, nomePagina: 'Gerenciar Escrituras (UC-09)', url: '/app/escrituras', modulo: 'Escrituração' },
        { id: 10, nomePagina: 'Índice de Livros Notariais', url: '/app/indice-livros', modulo: 'Escrituração' },
        { id: 11, nomePagina: 'Exportar Arquivo DOI - RFB (UC-10)', url: '/app/envio-doi', modulo: 'Obrigações Fiscais' },
        { id: 12, nomePagina: 'Emitir Certidões (UC-11)', url: '/app/certidoes', modulo: 'Atendimento' }
      ]))
    );
  }

  listarCartorios(): Observable<CartorioDto[]> {
    return this.http.get<ApiResponse<CartorioDto[]>>(`${this.baseUrl}/catalogos/cartorios`).pipe(
      map(r => r.dados),
      catchError(() => of([
        {
          id: 1,
          nomeCartorio: '2º Tabelionato de Notas de Campinas',
          cnpjCartorio: '46017488000185',
          tabeliaoResponsavel: 'Daniel Luiz Granado (Escrevente) / Wemeson Carvalho (Tabelião)',
          enderecoCompleto: 'Rua General Osório, 971 - Centro, Campinas/SP',
          telefone: '1932314455',
          email: 'contato@2notas-campinas.not.br',
          codigoCns: '11.542-8'
        }
      ]))
    );
  }

  listarAtos(): Observable<AtoDto[]> {
    return this.http.get<ApiResponse<AtoDto[]>>(`${this.baseUrl}/catalogos/atos`).pipe(
      map(r => r.dados),
      catchError(() => of([
        { id: 1, tipoAto: 'Compra e Venda', descricaoAto: 'Escritura Pública de Compra e Venda de Imóvel Urbano', exigeImovel: true, geraDoi: true, ativo: true },
        { id: 2, tipoAto: 'Doação', descricaoAto: 'Escritura Pública de Doação de Bem Imóvel', exigeImovel: true, geraDoi: true, ativo: true },
        { id: 3, tipoAto: 'Permuta', descricaoAto: 'Escritura Pública de Permuta entre Bens Imóveis', exigeImovel: true, geraDoi: true, ativo: true },
        { id: 4, tipoAto: 'Inventário e Partilha', descricaoAto: 'Escritura Pública de Inventário e Partilha Extrajudicial', exigeImovel: true, geraDoi: true, ativo: true },
        { id: 5, tipoAto: 'Confissão de Dívida com Hipoteca', descricaoAto: 'Escritura Pública de Confissão de Dívida com Garantia Hipotecária', exigeImovel: true, geraDoi: true, ativo: true }
      ]))
    );
  }

  consultarCep(cep: string): Observable<CepConsultaDto> {
    const digits = cep.replace(/\D/g, '');
    return this.http.get<ApiResponse<CepConsultaDto>>(`${this.baseUrl}/catalogos/cep/${digits}`).pipe(
      map(r => r.dados),
      catchError(() => {
        const mapLocal: Record<string, CepConsultaDto> = {
          '13183091': { cep: '13183091', logradouro: 'Rua Antonio Carlos', bairro: 'Jardim Nova Hortolândia', cidade: 'Hortolândia', estado: 'SP', pais: 'Brasil' },
          '13060245': { cep: '13060245', logradouro: 'Rua 2', bairro: 'Vila União', cidade: 'Campinas', estado: 'SP', pais: 'Brasil' },
          '13060725': { cep: '13060725', logradouro: 'Rua Moacyr Lacerda', bairro: 'Jardim Yeda', cidade: 'Campinas', estado: 'SP', pais: 'Brasil' },
          '13040000': { cep: '13040000', logradouro: 'Avenida Prestes Maia', bairro: 'Jardim do Trevo', cidade: 'Campinas', estado: 'SP', pais: 'Brasil' },
          '13010111': { cep: '13010111', logradouro: 'Rua General Osório', bairro: 'Centro', cidade: 'Campinas', estado: 'SP', pais: 'Brasil' }
        };
        return of(mapLocal[digits] ?? { cep: digits, logradouro: 'Rua Principal', bairro: 'Centro', cidade: 'Campinas', estado: 'SP', pais: 'Brasil' });
      })
    );
  }

  // --- UC-01: Permissões ---
  listarPermissoes(idPerfil?: number): Observable<PermissaoDto[]> {
    const params = idPerfil ? new HttpParams().set('idPerfil', idPerfil) : undefined;
    return this.http.get<ApiResponse<PermissaoDto[]>>(`${this.baseUrl}/permissoes`, { params }).pipe(
      map(r => r.dados)
    );
  }

  incluirPermissao(cmd: unknown): Observable<PermissaoDto> {
    return this.http.post<ApiResponse<PermissaoDto>>(`${this.baseUrl}/permissoes`, cmd).pipe(map(r => r.dados));
  }

  alterarPermissao(id: number, cmd: unknown): Observable<PermissaoDto> {
    return this.http.put<ApiResponse<PermissaoDto>>(`${this.baseUrl}/permissoes/${id}`, cmd).pipe(map(r => r.dados));
  }

  excluirPermissao(id: number): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/permissoes/${id}`).pipe(map(r => r.dados));
  }

  // --- UC-02: Funcionários ---
  listarFuncionarios(termo = '', pagina = 1, tamanhoPagina = 20): Observable<ResultadoPaginado<FuncionarioResumoDto>> {
    const params = new HttpParams().set('termo', termo).set('pagina', pagina).set('tamanhoPagina', tamanhoPagina);
    return this.http.get<ApiResponse<ResultadoPaginado<FuncionarioResumoDto>>>(`${this.baseUrl}/funcionarios`, { params }).pipe(
      map(r => r.dados)
    );
  }

  obterFuncionario(id: number): Observable<FuncionarioDetalheDto> {
    return this.http.get<ApiResponse<FuncionarioDetalheDto>>(`${this.baseUrl}/funcionarios/${id}`).pipe(map(r => r.dados));
  }

  incluirFuncionario(cmd: unknown): Observable<FuncionarioDetalheDto> {
    return this.http.post<ApiResponse<FuncionarioDetalheDto>>(`${this.baseUrl}/funcionarios`, cmd).pipe(map(r => r.dados));
  }

  alterarFuncionario(id: number, cmd: unknown): Observable<FuncionarioDetalheDto> {
    return this.http.put<ApiResponse<FuncionarioDetalheDto>>(`${this.baseUrl}/funcionarios/${id}`, cmd).pipe(map(r => r.dados));
  }

  inativarFuncionario(id: number): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/funcionarios/${id}`).pipe(map(r => r.dados));
  }

  // --- UC-08: Clientes ---
  listarClientes(termo = '', pagina = 1, tamanhoPagina = 20): Observable<ResultadoPaginado<ClienteResumoDto>> {
    const params = new HttpParams().set('termo', termo).set('pagina', pagina).set('tamanhoPagina', tamanhoPagina);
    return this.http.get<ApiResponse<ResultadoPaginado<ClienteResumoDto>>>(`${this.baseUrl}/clientes`, { params }).pipe(
      map(r => r.dados)
    );
  }

  listarOpcoesClientes(termo = ''): Observable<OpcaoDto[]> {
    const params = new HttpParams().set('termo', termo);
    return this.http.get<ApiResponse<OpcaoDto[]>>(`${this.baseUrl}/clientes/opcoes`, { params }).pipe(map(r => r.dados));
  }

  obterCliente(id: number): Observable<ClienteDetalheDto> {
    return this.http.get<ApiResponse<ClienteDetalheDto>>(`${this.baseUrl}/clientes/${id}`).pipe(map(r => r.dados));
  }

  incluirCliente(cmd: unknown): Observable<ClienteDetalheDto> {
    return this.http.post<ApiResponse<ClienteDetalheDto>>(`${this.baseUrl}/clientes`, cmd).pipe(map(r => r.dados));
  }

  alterarCliente(id: number, cmd: unknown): Observable<ClienteDetalheDto> {
    return this.http.put<ApiResponse<ClienteDetalheDto>>(`${this.baseUrl}/clientes/${id}`, cmd).pipe(map(r => r.dados));
  }

  inativarCliente(id: number): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/clientes/${id}`).pipe(map(r => r.dados));
  }

  consultarDocumentosPorCliente(termo = ''): Observable<DocumentoDigitalizadoDto[]> {
    const params = new HttpParams().set('termo', termo);
    return this.http.get<ApiResponse<DocumentoDigitalizadoDto[]>>(`${this.baseUrl}/clientes/documentos`, { params }).pipe(
      map(r => r.dados)
    );
  }

  // --- UC-06: Orçamentos CNB/SP ---
  listarOrcamentos(): Observable<OrcamentoDto[]> {
    return this.http.get<ApiResponse<OrcamentoDto[]>>(`${this.baseUrl}/orcamentos`).pipe(map(r => r.dados));
  }

  obterTabelaCustas(): Observable<FaixaCustasDto[]> {
    return this.http.get<ApiResponse<FaixaCustasDto[]>>(`${this.baseUrl}/orcamentos/tabela-custas`).pipe(map(r => r.dados));
  }

  simularOrcamento(req: CalcularOrcamentoRequest): Observable<OrcamentoDto> {
    return this.http.post<ApiResponse<OrcamentoDto>>(`${this.baseUrl}/orcamentos/simular`, req).pipe(map(r => r.dados));
  }

  criarOrcamento(req: CalcularOrcamentoRequest): Observable<OrcamentoDto> {
    return this.http.post<ApiResponse<OrcamentoDto>>(`${this.baseUrl}/orcamentos`, req).pipe(map(r => r.dados));
  }

  urlOrcamentoPdf(id: number): string {
    return `${this.baseUrl}/orcamentos/${id}/pdf`;
  }

  // --- UC-07 e UC-12: Protocolos e Consulta Pública ---
  listarProtocolos(termo = '', pagina = 1, tamanhoPagina = 20): Observable<ResultadoPaginado<ProtocoloDto>> {
    const params = new HttpParams().set('termo', termo).set('pagina', pagina).set('tamanhoPagina', tamanhoPagina);
    return this.http.get<ApiResponse<ResultadoPaginado<ProtocoloDto>>>(`${this.baseUrl}/protocolos`, { params }).pipe(
      map(r => r.dados)
    );
  }

  listarProtocolosDisponiveis(incluirId?: number): Observable<OpcaoDto[]> {
    const params = incluirId ? new HttpParams().set('incluirProtocoloId', incluirId) : undefined;
    return this.http.get<ApiResponse<OpcaoDto[]>>(`${this.baseUrl}/protocolos/disponiveis-para-escritura`, { params }).pipe(
      map(r => r.dados)
    );
  }

  criarProtocolo(cmd: unknown): Observable<ProtocoloDto> {
    return this.http.post<ApiResponse<ProtocoloDto>>(`${this.baseUrl}/protocolos`, cmd).pipe(map(r => r.dados));
  }

  alterarProtocolo(id: number, cmd: unknown): Observable<ProtocoloDto> {
    return this.http.put<ApiResponse<ProtocoloDto>>(`${this.baseUrl}/protocolos/${id}`, cmd).pipe(map(r => r.dados));
  }

  inativarProtocolo(id: number): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/protocolos/${id}`).pipe(map(r => r.dados));
  }

  urlProtocoloPdf(id: number): string {
    return `${this.baseUrl}/protocolos/${id}/pdf`;
  }

  consultarAndamentoPublico(numeroProtocolo: string): Observable<AndamentoProcessoPublicoDto> {
    return this.http.get<ApiResponse<AndamentoProcessoPublicoDto>>(`${this.baseUrl}/protocolos/publico/${encodeURIComponent(numeroProtocolo.trim())}`).pipe(
      map(r => r.dados)
    );
  }

  // --- UC-09, UC-10, UC-11: Escrituras, Índice de Livros, DOI e Certidões ---
  listarEscrituras(termo = '', pagina = 1, tamanhoPagina = 20): Observable<ResultadoPaginado<EscrituraResumoDto>> {
    const params = new HttpParams().set('termo', termo).set('pagina', pagina).set('tamanhoPagina', tamanhoPagina);
    return this.http.get<ApiResponse<ResultadoPaginado<EscrituraResumoDto>>>(`${this.baseUrl}/escrituras`, { params }).pipe(
      map(r => r.dados)
    );
  }

  consultarIndiceLivros(termo = '', numeroLivro?: number): Observable<ResultadoPaginado<IndiceEscrituraDto>> {
    let params = new HttpParams().set('termo', termo).set('pagina', 1).set('tamanhoPagina', 50);
    if (numeroLivro) params = params.set('numeroLivro', numeroLivro);
    return this.http.get<ApiResponse<ResultadoPaginado<IndiceEscrituraDto>>>(`${this.baseUrl}/escrituras/indice-livros`, { params }).pipe(
      map(r => r.dados)
    );
  }

  obterEscritura(id: number): Observable<EscrituraDetalheDto> {
    return this.http.get<ApiResponse<EscrituraDetalheDto>>(`${this.baseUrl}/escrituras/${id}`).pipe(map(r => r.dados));
  }

  criarEscritura(cmd: unknown): Observable<EscrituraDetalheDto> {
    return this.http.post<ApiResponse<EscrituraDetalheDto>>(`${this.baseUrl}/escrituras`, cmd).pipe(map(r => r.dados));
  }

  atualizarEscritura(id: number, cmd: unknown): Observable<EscrituraDetalheDto> {
    return this.http.put<ApiResponse<EscrituraDetalheDto>>(`${this.baseUrl}/escrituras/${id}`, cmd).pipe(map(r => r.dados));
  }

  finalizarLavratura(id: number, textoMinutaDefinitivo?: string): Observable<EscrituraDetalheDto> {
    return this.http.post<ApiResponse<EscrituraDetalheDto>>(`${this.baseUrl}/escrituras/${id}/finalizar-lavratura`, {
      idEscritura: id,
      textoMinutaDefinitivo
    }).pipe(map(r => r.dados));
  }

  inativarEscritura(id: number, motivo: string): Observable<boolean> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/escrituras/${id}/inativar`, {
      idEscritura: id,
      motivo
    }).pipe(map(r => r.dados));
  }

  registrarRetiradaDocumentos(id: number, observacoes: string): Observable<EscrituraDetalheDto> {
    return this.http.post<ApiResponse<EscrituraDetalheDto>>(`${this.baseUrl}/escrituras/${id}/retirada`, {
      idEscritura: id,
      observacoes
    }).pipe(map(r => r.dados));
  }

  anexarPdfBase64(req: {
    escrituraId?: number | null;
    clienteId?: number | null;
    nomeArquivo: string;
    conteudoBase64: string;
    tipoDocumento: TipoDocumentoDigitalizado;
    descricao?: string;
  }): Observable<DocumentoDigitalizadoDto> {
    return this.http.post<ApiResponse<DocumentoDigitalizadoDto>>(`${this.baseUrl}/escrituras/anexar-pdf-base64`, req).pipe(
      map(r => r.dados)
    );
  }

  removerDocumentoPdf(documentoId: number): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/escrituras/documentos-digitalizados/${documentoId}`).pipe(
      map(r => r.dados)
    );
  }

  urlCertidaoEscrituraPdf(id: number): string {
    return `${this.baseUrl}/escrituras/${id}/certidao-pdf`;
  }

  urlCertidaoPorProtocoloPdf(numeroProtocolo: string): string {
    return `${this.baseUrl}/escrituras/certidao-por-protocolo/${encodeURIComponent(numeroProtocolo.trim())}`;
  }

  urlExportarDoiXml(id: number): string {
    return `${this.baseUrl}/escrituras/${id}/exportar-doi-xml`;
  }

  urlProtocoloRetiradaPdf(id: number): string {
    return `${this.baseUrl}/escrituras/${id}/protocolo-retirada-pdf`;
  }

  urlDownloadDocumentoPdf(documentoId: number): string {
    return `${this.baseUrl}/escrituras/documentos-digitalizados/${documentoId}/download`;
  }

  // --- UC-03 e UC-05: Backups Criptografados ---
  listarBackups(): Observable<BackupDto[]> {
    return this.http.get<ApiResponse<BackupDto[]>>(`${this.baseUrl}/backups`).pipe(map(r => r.dados));
  }

  executarBackupManual(descricaoBackup: string): Observable<BackupDto> {
    return this.http.post<ApiResponse<BackupDto>>(`${this.baseUrl}/backups/manual`, {
      descricaoBackup,
      idFuncionario: 1
    }).pipe(map(r => r.dados));
  }

  agendarBackup(dataAgendada: string, descricaoBackup: string): Observable<BackupDto> {
    return this.http.post<ApiResponse<BackupDto>>(`${this.baseUrl}/backups/agendado`, {
      dataAgendada,
      descricaoBackup,
      idFuncionario: 1
    }).pipe(map(r => r.dados));
  }

  urlDownloadBackup(id: number): string {
    return `${this.baseUrl}/backups/${id}/download`;
  }

  // --- UC-04: Relatórios e Dashboard ---
  obterDashboard(): Observable<DashboardResumoDto> {
    return this.http.get<ApiResponse<DashboardResumoDto>>(`${this.baseUrl}/relatorios/dashboard`).pipe(map(r => r.dados));
  }

  gerarRelatorioEmTela(filtro: {
    tipoRelatorio: TipoRelatorioEnum;
    dataInicio?: string | null;
    dataFim?: string | null;
    termo?: string | null;
  }): Observable<RelatorioResultadoDto> {
    return this.http.post<ApiResponse<RelatorioResultadoDto>>(`${this.baseUrl}/relatorios/gerar`, filtro).pipe(
      map(r => r.dados)
    );
  }

  baixarRelatorioPdf(filtro: {
    tipoRelatorio: TipoRelatorioEnum;
    dataInicio?: string | null;
    dataFim?: string | null;
    termo?: string | null;
  }): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/relatorios/gerar-pdf`, filtro, { responseType: 'blob' });
  }
}
