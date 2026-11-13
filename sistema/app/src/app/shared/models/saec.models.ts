export interface ApiResponse<T> {
  sucesso: boolean;
  dados: T;
  mensagens?: string[];
  erros?: string[];
  alertas?: string[];
}

export interface ResultadoPaginado<T> {
  itens: T[];
  totalRegistros: number;
  pagina: number;
  tamanhoPagina: number;
  totalPaginas: number;
}

export interface OpcaoDto {
  id: number;
  descricao: string;
  codigoAuxiliar?: string;
}

export type StatusRegistro = 'Ativo' | 'Inativo' | 1 | 0;

export type StatusEscritura =
  | 'Inativa'
  | 'AguardandoDocumentacao'
  | 'EmProcessoDeEscrituracao'
  | 'EmProcessoDeImpressao'
  | 'Finalizada';

export type TipoDocumentoDigitalizado =
  | 'DocumentosApresentados'
  | 'EscrituraDigitalizada'
  | 'CertidaoCasamento'
  | 'CertidaoOutroOrgao'
  | 'ComprovanteResidencia';

export type TipoRelatorioEnum =
  | 'EscriturasCriadas'
  | 'ProtocolosAtendimento'
  | 'IndiceLivroNotas'
  | 'ClientesCadastrados'
  | 'OrcamentosRealizados'
  | 'AuditoriaSistema';

export interface EnderecoDto {
  cep: string;
  pais: string;
  estado: string;
  cidade: string;
  bairro: string;
  logradouro: string;
  numero: string;
  complemento?: string | null;
}

export interface CepConsultaDto {
  cep: string;
  logradouro: string;
  bairro: string;
  cidade: string;
  estado: string;
  pais: string;
  complemento?: string | null;
}

export interface PerfilDto {
  id: number;
  nomePerfil: string;
  descricao?: string;
  status: StatusRegistro;
}

export interface PaginaDto {
  id: number;
  nomePagina: string;
  url: string;
  modulo?: string;
}

export interface PermissaoDto {
  id: number;
  idPerfil: number;
  nomePerfil: string;
  idPagina: number;
  nomePagina: string;
  urlPagina: string;
  modulo?: string;
  podeVisualizar: boolean;
  podeIncluir: boolean;
  podeEditar: boolean;
  podeExcluir: boolean;
  status: StatusRegistro;
}

export interface CartorioDto {
  id: number;
  nomeCartorio: string;
  cnpjCartorio: string;
  tabeliaoResponsavel?: string;
  enderecoCompleto?: string;
  telefone?: string;
  email?: string;
  codigoCns?: string;
}

export interface AtoDto {
  id: number;
  tipoAto: string;
  descricaoAto: string;
  modeloMinutaPadrao?: string;
  exigeImovel: boolean;
  geraDoi: boolean;
  ativo: boolean;
}

export interface FuncionarioResumoDto {
  id: number;
  nomeFuncionario: string;
  emailFuncionario: string;
  cpfFuncionario: string;
  rgFuncionario: string;
  idPerfil: number;
  nomePerfil: string;
  cargo?: string;
  telefoneFuncionario?: string;
  dataAdmissao?: string;
  statusFuncionario: StatusRegistro;
  cidadeEstado: string;
}

export interface FuncionarioDetalheDto {
  id: number;
  idPerfil: number;
  nomePerfil: string;
  nomeFuncionario: string;
  dataNascFuncionario: string;
  dataAdmissao?: string;
  emailFuncionario: string;
  cpfFuncionario: string;
  rgFuncionario: string;
  telefoneFuncionario?: string;
  cargo?: string;
  statusFuncionario: StatusRegistro;
  endereco: EnderecoDto;
  dtCriacao: string;
  dtAlteracao?: string;
}

export interface ConjugeDto {
  id?: number | null;
  nomeConjuge: string;
  rgConjuge: string;
  orgaoExpedidorConjuge: string;
  nacionalidadeConjuge: string;
  estadoOrgaoExpedidorConjuge: string;
  cpfConjuge: string;
  dataNascConjuge: string;
  emailConjuge: string;
  generoConjuge: string;
  estadoCivilConjuge: string;
  profissaoConjuge: string;
  telefoneConjuge: string;
  endereco: EnderecoDto;
}

export interface CertidaoCasamentoDto {
  id?: number | null;
  cartorioCertidaoCasamento: string;
  livroCertidaoCasamento: string;
  folhasCertidaoCasamento: string;
  termoCertidaoCasamento: string;
  dataCertidaoCasamento: string;
  averbacoes?: string | null;
}

export interface ClienteResumoDto {
  id: number;
  nomeCliente: string;
  cpfCliente: string;
  rgCliente: string;
  emailCliente: string;
  telefoneCliente: string;
  estadoCivilCliente: string;
  profissaoCliente: string;
  cidadeEstado: string;
  nomeConjuge?: string | null;
  statusCliente: StatusRegistro;
}

export interface ClienteDetalheDto {
  id: number;
  nomeCliente: string;
  rgCliente: string;
  orgaoExpedidorCliente: string;
  nacionalidadeCliente: string;
  estadoOrgaoExpedidorCliente: string;
  cpfCliente: string;
  dataNascCliente: string;
  emailCliente: string;
  generoCliente: string;
  estadoCivilCliente: string;
  regimeBens?: string | null;
  profissaoCliente: string;
  telefoneCliente: string;
  statusCliente: StatusRegistro;
  endereco: EnderecoDto;
  conjuge?: ConjugeDto | null;
  certidaoCasamento?: CertidaoCasamentoDto | null;
  dtCriacao: string;
}

export interface CalcularOrcamentoRequest {
  valorBaseAto: number;
  idCliente?: number | null;
  oneroso: boolean;
  hipoteca: boolean;
  ufesp2500: boolean;
  salarios30: boolean;
  certidaoInclusa: boolean;
  outrosValores: number;
}

export interface OrcamentoDto {
  id: number;
  idCliente?: number | null;
  nomeCliente?: string | null;
  valorBaseAto: number;
  oneroso: boolean;
  hipoteca: boolean;
  ufesp2500: boolean;
  salarios30: boolean;
  certidaoInclusa: boolean;
  outrosValores: number;
  faixaDescricao: string;
  emolumentosTabelionato: number;
  emolumentosEstado: number;
  contribuicaoIpesp: number;
  fundoRegistroCivil: number;
  taxaTribunalJustica: number;
  fundoMinisterioPublico: number;
  issqnEstimado: number;
  valorCertidao: number;
  previsaoRegistroImoveis: number;
  valorTotalOrcamento: number;
  dtCriacao: string;
}

export interface FaixaCustasDto {
  ordem: number;
  descricaoFaixa: string;
  valorMinimo: number;
  valorMaximo: number;
  emolumentosTabeliao: number;
  receitaEstado: number;
  contribuicaoIpesp: number;
  fundoRegistroCivil: number;
  taxaTribunalJustica: number;
  fundoMinisterioPublico: number;
  totalNotarial: number;
  estimativaRegistroImoveis: number;
}

export interface ProtocoloDto {
  id: number;
  numeroProtocolo: string;
  idCliente: number;
  nomeCliente: string;
  cpfCliente: string;
  telefoneCliente: string;
  emailCliente: string;
  idAto?: number | null;
  tipoAto?: string | null;
  idFuncionario?: number | null;
  nomeEscrevente?: string | null;
  idOrcamento?: number | null;
  naturezaPedido?: string | null;
  documentosApresentados?: string | null;
  documentosFaltantes?: string | null;
  valorOrcamentoEstimado?: number | null;
  observacoes?: string | null;
  statusAndamento: StatusEscritura;
  statusAndamentoDescricao: string;
  statusProtocolo: StatusRegistro;
  idEscrituraVinculada?: number | null;
  numeroEscrituraVinculada?: string | null;
  dtCriacao: string;
}

export interface EtapaAndamentoDto {
  ordem: number;
  etapa: StatusEscritura;
  titulo: string;
  descricao: string;
  concluida: boolean;
  atual: boolean;
}

export interface AndamentoProcessoPublicoDto {
  numeroProtocolo: string;
  nomeApresentante: string;
  naturezaAto: string;
  cartorioNome: string;
  escreventeResponsavel?: string | null;
  statusAtual: StatusEscritura;
  statusAtualDescricao: string;
  dataSolicitacao: string;
  dataUltimaAtualizacao?: string | null;
  dataConclusao?: string | null;
  numeroLivro?: string | null;
  folhas?: string | null;
  documentosProntosParaRetirada: boolean;
  documentosRetirados: boolean;
  etapas: EtapaAndamentoDto[];
}

export interface ImovelDto {
  id?: number | null;
  idCliente?: number | null;
  numeroRegistro: string;
  valorVenal: number;
  nomePrefeitura: string;
  descricaoImovel?: string | null;
  valorImovel: number;
  areaMetrosQuadrados?: number | null;
  valorPorMetroQuadrado?: number | null;
  inscricaoMunicipal?: string | null;
  cartorioRegistroImoveis?: string | null;
  endereco: EnderecoDto;
}

export interface CertidaoOutroOrgaoDto {
  id?: number | null;
  cartorioCertidaoOutroOrgao: string;
  livroCertidaoOutroOrgao: string;
  folhasCertidaoOutroOrgao: string;
  termoCertidaoOutroOrgao: string;
  dataCertidaoOutroOrgao: string;
}

export interface DocumentoDigitalizadoDto {
  id: number;
  idEscritura?: number | null;
  idCliente?: number | null;
  nomeCliente?: string | null;
  cpfCliente?: string | null;
  tipoDocumento: TipoDocumentoDigitalizado;
  tipoDocumentoDescricao: string;
  nomeArquivo: string;
  contentType: string;
  tamanhoBytes: number;
  hashSha256: string;
  observacao?: string | null;
  dtCriacao: string;
}

export interface EscrituraResumoDto {
  id: number;
  numeroEscritura: string;
  idProtocolo: number;
  numeroProtocolo: string;
  idCartorio: number;
  nomeCartorio: string;
  idAto: number;
  tipoAto: string;
  idOutorgante: number;
  nomeOutorgante: string;
  cpfOutorgante: string;
  idOutorgado: number;
  nomeOutorgado: string;
  cpfOutorgado: string;
  idFuncionario: number;
  nomeEscrevente: string;
  numeroLivro: number;
  folhaInicial: number;
  folhaFinal: number;
  matriculaImovel?: string | null;
  valorOperacao: number;
  statusEscritura: StatusEscritura;
  statusEscrituraDescricao: string;
  dataInicioProcesso: string;
  dataLavratura?: string | null;
  dataFimProcesso?: string | null;
  dataEnvioDoi?: string | null;
  documentosRetiradosPeloCliente: boolean;
  quantidadeDocumentosPdf: number;
}

export interface EscrituraDetalheDto {
  id: number;
  numeroEscritura: string;
  idCartorio: number;
  nomeCartorio: string;
  idProtocolo: number;
  numeroProtocolo: string;
  idAto: number;
  tipoAto: string;
  idFuncionario: number;
  nomeEscrevente: string;
  outorgante: ClienteDetalheDto;
  outorgado: ClienteDetalheDto;
  imovel?: ImovelDto | null;
  certidaoOutroOrgao?: CertidaoOutroOrgaoDto | null;
  numeroLivro: number;
  folhaInicial: number;
  folhaFinal: number;
  valorOperacao: number;
  textoMinuta: string;
  statusEscritura: StatusEscritura;
  statusEscrituraDescricao: string;
  motivoInativacao?: string | null;
  dataInicioProcesso: string;
  dataLavratura?: string | null;
  dataFimProcesso?: string | null;
  dataEnvioDoi?: string | null;
  documentosRetiradosPeloCliente: boolean;
  dataRetiradaDocumentos?: string | null;
  observacoesRetirada?: string | null;
  documentos: DocumentoDigitalizadoDto[];
}

export interface IndiceEscrituraDto {
  idEscritura: number;
  numeroEscritura: string;
  numeroProtocolo: string;
  numeroLivro: number;
  folhaInicial: number;
  folhaFinal: number;
  tipoAto: string;
  nomeOutorgante: string;
  cpfOutorgante: string;
  nomeOutorgado: string;
  cpfOutorgado: string;
  matriculaImovel?: string | null;
  valorOperacao: number;
  dataLavratura: string;
  doiGerada: boolean;
  dataEnvioDoi?: string | null;
  possuiPdfDigitalizado: boolean;
}

export interface BackupDto {
  id: number;
  dataBackup: string;
  dataAgendada?: string | null;
  descricaoBackup: string;
  idFuncionario?: number | null;
  nomeFuncionario: string;
  manual: boolean;
  criptografado: boolean;
  executado: boolean;
  nomeArquivoGerado: string;
  tamanhoBytes: number;
  hashIntegridade: string;
}

export interface RelatorioLinhaDto {
  idReferencia: number;
  codigo: string;
  titulo: string;
  partePrincipal: string;
  parteSecundaria: string;
  categoria: string;
  status: string;
  valor?: number | null;
  dataReferencia: string;
  detalhes: string;
}

export interface RelatorioResultadoDto {
  tipoRelatorio: TipoRelatorioEnum;
  tituloRelatorio: string;
  dataGeracao: string;
  geradoPor: string;
  totalRegistros: number;
  somaValores: number;
  itens: RelatorioLinhaDto[];
}

export interface DashboardResumoDto {
  totalClientesAtivos: number;
  totalProtocolosAtivos: number;
  totalEscriturasEmAndamento: number;
  totalEscriturasFinalizadas: number;
  escriturasPendentesDoi: number;
  totalDocumentosDigitalizados: number;
  valorTotalEscriturado: number;
  ultimoBackupRealizado?: string | null;
  escriturasPorStatus: Record<string, number>;
  escriturasPorNaturezaAto: Record<string, number>;
  ultimasEscrituras: EscrituraResumoDto[];
}
