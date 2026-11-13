import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SaecApiService } from '../../core/services/saec-api.service';
import {
  AtoDto,
  CartorioDto,
  EscrituraDetalheDto,
  EscrituraResumoDto,
  OpcaoDto,
  TipoDocumentoDigitalizado
} from '../../shared/models/saec.models';
import { CpfCnpjPipe } from '../../shared/pipes/documento.pipe';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';

/**
 * UC-09: Gerenciar Escrituras (Fluxo completo em abas, Prévia de Minuta, Lavratura, Digitalizações PDF,
 * Retirada de Documentos, Emissão de Certidão PDF [UC-11] e Exportação XML DOI [UC-10])
 */
@Component({
  selector: 'app-escrituras',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatTabsModule,
    MatSnackBarModule,
    MatDialogModule,
    MatTooltipModule,
    CpfCnpjPipe
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>description</mat-icon> Gerenciar Escrituras Públicas (UC-09)</h1>
        <p>Lavratura automatizada de escrituras, conferência prévia de minuta em tela, anexação de PDFs, índice de livros e DOI.</p>
      </div>
      <button mat-flat-button color="primary" (click)="novaEscritura()">
        <mat-icon>note_add</mat-icon>
        Nova Escritura Pública
      </button>
    </div>

    @if (exibirWizard()) {
      <mat-card class="p-6 mb-4">
        <div class="flex flex-wrap justify-between items-center gap-2 mb-4">
          <h3 class="text-primary" style="margin:0">
            {{ detalheAtual() ? 'Escritura Nº ' + detalheAtual()!.numeroEscritura + ' (Protocolo ' + detalheAtual()!.numeroProtocolo + ')' : 'Nova Escritura Pública (UC-09)' }}
          </h3>

          <div class="flex flex-wrap gap-2">
            @if (detalheAtual(); as esc) {
              @if (esc.statusEscritura !== 'Finalizada' && esc.statusEscritura !== 'Inativa') {
                <button mat-flat-button color="primary" (click)="finalizarLavratura(esc.id)">
                  <mat-icon>gavel</mat-icon>
                  Finalizar Lavratura (Gerar Índice & Habilitar DOI)
                </button>
              }
              <a mat-stroked-button color="primary" [href]="api.urlCertidaoEscrituraPdf(esc.id)" target="_blank">
                <mat-icon>picture_as_pdf</mat-icon>
                Emitir Certidão PDF (UC-11)
              </a>
              <a mat-stroked-button color="primary" [href]="api.urlExportarDoiXml(esc.id)" target="_blank">
                <mat-icon>code</mat-icon>
                Exportar XML DOI (UC-10)
              </a>
            }
          </div>
        </div>

        <mat-tab-group>
          <!-- ABA 1: ESCRITURA, PROTOCOLO, LIVRO E FOLHAS -->
          <mat-tab label="1. Escritura & Livro/Folhas">
            <div class="form-grid mt-4">
              <mat-form-field appearance="outline">
                <mat-label>Cartório / Serventia</mat-label>
                <mat-select [(ngModel)]="form.idCartorio">
                  @for (ct of cartorios(); track ct.id) {
                    <mat-option [value]="ct.id">{{ ct.nomeCartorio }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Protocolo Vinculado (UC-07)</mat-label>
                <mat-select [(ngModel)]="form.idProtocolo">
                  @for (pr of protocolosDisponiveis(); track pr.id) {
                    <mat-option [value]="pr.id">{{ pr.descricao }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Natureza do Ato Notarial</mat-label>
                <mat-select [(ngModel)]="form.idAto">
                  @for (a of atos(); track a.id) {
                    <mat-option [value]="a.id">{{ a.tipoAto }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Número do Livro (Apenas números)</mat-label>
                <input matInput type="number" [(ngModel)]="form.numeroLivro" min="1" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Folha Inicial</mat-label>
                <input matInput type="number" [(ngModel)]="form.folhaInicial" min="1" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Folha Final</mat-label>
                <input matInput type="number" [(ngModel)]="form.folhaFinal" min="1" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Valor da Operação / Transação (R$)</mat-label>
                <input matInput type="number" [(ngModel)]="form.valorOperacao" min="0" required />
              </mat-form-field>
            </div>
          </mat-tab>

          <!-- ABA 2: OUTORGANTE E OUTORGADO -->
          <mat-tab label="2. Outorgante & Outorgado (Clientes)">
            <div class="form-grid-2 mt-4">
              <mat-card class="p-4">
                <h4 class="text-primary" style="margin-top:0">Outorgante (Transmitente / Vendedor)</h4>
                <mat-form-field appearance="outline">
                  <mat-label>Selecionar Cliente Outorgante (UC-08)</mat-label>
                  <mat-select [(ngModel)]="form.idOutorgante">
                    @for (c of clientesOpcoes(); track c.id) {
                      <mat-option [value]="c.id">{{ c.descricao }}</mat-option>
                    }
                  </mat-select>
                </mat-form-field>
                @if (detalheAtual()?.outorgante; as outg) {
                  <div class="text-sm">
                    <div><strong>CPF:</strong> {{ outg.cpfCliente | cpfCnpj }} | <strong>RG:</strong> {{ outg.rgCliente }}</div>
                    <div><strong>Estado Civil:</strong> {{ outg.estadoCivilCliente }} ({{ outg.regimeBens || '-' }})</div>
                    <div><strong>Endereço:</strong> {{ outg.endereco.logradouro }}, {{ outg.endereco.numero }} - {{ outg.endereco.bairro }}, {{ outg.endereco.cidade }}/{{ outg.endereco.estado }}</div>
                    @if (outg.conjuge) {
                      <div class="mt-2"><strong>Cônjuge:</strong> {{ outg.conjuge.nomeConjuge }} (CPF: {{ outg.conjuge.cpfConjuge | cpfCnpj }})</div>
                    }
                  </div>
                }
              </mat-card>

              <mat-card class="p-4">
                <h4 class="text-primary" style="margin-top:0">Outorgado (Adquirente / Comprador)</h4>
                <mat-form-field appearance="outline">
                  <mat-label>Selecionar Cliente Outorgado (UC-08)</mat-label>
                  <mat-select [(ngModel)]="form.idOutorgado">
                    @for (c of clientesOpcoes(); track c.id) {
                      <mat-option [value]="c.id">{{ c.descricao }}</mat-option>
                    }
                  </mat-select>
                </mat-form-field>
                @if (detalheAtual()?.outorgado; as outd) {
                  <div class="text-sm">
                    <div><strong>CPF:</strong> {{ outd.cpfCliente | cpfCnpj }} | <strong>RG:</strong> {{ outd.rgCliente }}</div>
                    <div><strong>Estado Civil:</strong> {{ outd.estadoCivilCliente }} ({{ outd.regimeBens || '-' }})</div>
                    <div><strong>Endereço:</strong> {{ outd.endereco.logradouro }}, {{ outd.endereco.numero }} - {{ outd.endereco.bairro }}, {{ outd.endereco.cidade }}/{{ outd.endereco.estado }}</div>
                    @if (outd.conjuge) {
                      <div class="mt-2"><strong>Cônjuge:</strong> {{ outd.conjuge.nomeConjuge }} (CPF: {{ outd.conjuge.cpfConjuge | cpfCnpj }})</div>
                    }
                  </div>
                }
              </mat-card>
            </div>
          </mat-tab>

          <!-- ABA 3: IMÓVEL E ENDEREÇO DO IMÓVEL -->
          <mat-tab label="3. Imóvel & Endereço do Imóvel">
            <div class="form-grid mt-4">
              <mat-form-field appearance="outline">
                <mat-label>Matrícula / Nº de Registro do Imóvel</mat-label>
                <input matInput [(ngModel)]="form.imovel.numeroRegistro" placeholder="Ex.: 2235" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Cartório de Registro de Imóveis</mat-label>
                <input matInput [(ngModel)]="form.imovel.cartorioRegistroImoveis" placeholder="2º Cartório de Registro de Imóveis de Campinas" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Prefeitura / Município</mat-label>
                <input matInput [(ngModel)]="form.imovel.nomePrefeitura" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Inscrição Municipal / IPTU</mat-label>
                <input matInput [(ngModel)]="form.imovel.inscricaoMunicipal" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Valor Venal de Referência (R$)</mat-label>
                <input matInput type="number" [(ngModel)]="form.imovel.valorVenal" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Valor do Imóvel na Transação (R$)</mat-label>
                <input matInput type="number" [(ngModel)]="form.imovel.valorImovel" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>CEP do Imóvel (Busca ViaCEP)</mat-label>
                <input matInput [(ngModel)]="form.imovel.endereco.cep" (blur)="buscarCepImovel()" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Logradouro do Imóvel</mat-label>
                <input matInput [(ngModel)]="form.imovel.endereco.logradouro" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Número</mat-label>
                <input matInput [(ngModel)]="form.imovel.endereco.numero" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Bairro</mat-label>
                <input matInput [(ngModel)]="form.imovel.endereco.bairro" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Cidade / UF</mat-label>
                <input matInput [(ngModel)]="form.imovel.endereco.cidade" />
              </mat-form-field>

              <mat-form-field appearance="outline" class="col-span-full">
                <mat-label>Descrição Completa do Imóvel</mat-label>
                <textarea matInput rows="2" [(ngModel)]="form.imovel.descricaoImovel"></textarea>
              </mat-form-field>
            </div>
          </mat-tab>

          <!-- ABA 4: PRÉVIA EM TELA DA MINUTA NOTARIAL -->
          <mat-tab label="4. Prévia da Minuta & Redação Notarial">
            <div class="mt-4">
              <p class="text-sm text-muted mb-2">
                Requisito Funcional 1.1 do TCC: Exibição da prévia da escritura na tela antes da impressão em papel oficial para conferência pelas partes.
              </p>
              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Texto Integral da Minuta da Escritura Pública (Editável antes da Lavratura)</mat-label>
                <textarea matInput rows="10" [(ngModel)]="form.textoMinuta" placeholder="Deixe em branco ao criar para gerar automaticamente a minuta padronizada a partir das partes e do imóvel..."></textarea>
              </mat-form-field>

              @if (form.textoMinuta) {
                <h4 class="text-primary mb-2">Folha de Conferência Notarial (Prévia de Impressão)</h4>
                <div class="notary-paper">{{ form.textoMinuta }}</div>
              }
            </div>
          </mat-tab>

          <!-- ABA 5: DIGITALIZAÇÕES PDF & RETIRADA -->
          <mat-tab label="5. Digitalizações (PDF) & Retirada">
            <div class="mt-4">
              @if (detalheAtual(); as esc) {
                <div class="form-grid-2 mb-4">
                  <mat-card class="p-4">
                    <h4 class="text-primary" style="margin-top:0">Anexar Documento Digitalizado (Somente PDF)</h4>
                    <mat-form-field appearance="outline">
                      <mat-label>Tipo de Documento</mat-label>
                      <mat-select [(ngModel)]="novoDocTipo">
                        <mat-option value="DocumentosApresentados">Documentos Pessoais / Matrícula</mat-option>
                        <mat-option value="EscrituraDigitalizada">Traslado da Escritura Assinada</mat-option>
                        <mat-option value="CertidaoCasamento">Certidão de Casamento</mat-option>
                        <mat-option value="CertidaoOutroOrgao">Certidão de Outro Órgão</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Descrição / Observação do PDF</mat-label>
                      <input matInput [(ngModel)]="novoDocDescricao" placeholder="Ex.: RG, CPF e Matrícula 2235 conferidos" />
                    </mat-form-field>

                    <div class="flex items-center gap-2">
                      <input type="file" accept="application/pdf,.pdf" (change)="selecionarArquivoPdf($event)" />
                    </div>
                  </mat-card>

                  <mat-card class="p-4">
                    <h4 class="text-primary" style="margin-top:0">Protocolo de Retirada de Documentos</h4>
                    <mat-form-field appearance="outline">
                      <mat-label>Identificação do Portador / Observações de Retirada</mat-label>
                      <input matInput [(ngModel)]="obsRetirada" placeholder="Retirado por Fabricio Lima dos Santos (Outorgado)" />
                    </mat-form-field>
                    <div class="flex flex-wrap gap-2">
                      <button mat-flat-button color="primary" (click)="registrarRetirada(esc.id)">
                        <mat-icon>how_to_reg</mat-icon>
                        Registrar Retirada
                      </button>
                      <a mat-stroked-button color="primary" [href]="api.urlProtocoloRetiradaPdf(esc.id)" target="_blank">
                        <mat-icon>picture_as_pdf</mat-icon>
                        Imprimir Protocolo de Retirada (PDF)
                      </a>
                    </div>
                  </mat-card>
                </div>

                <h4 class="text-primary mb-2">Documentos PDF Vinculados a esta Escritura ({{ esc.documentos.length }})</h4>
                <div class="table-container">
                  <table mat-table [dataSource]="esc.documentos">
                    <ng-container matColumnDef="nome">
                      <th mat-header-cell *matHeaderCellDef>Arquivo PDF</th>
                      <td mat-cell *matCellDef="let d"><strong>{{ d.nomeArquivo }}</strong></td>
                    </ng-container>
                    <ng-container matColumnDef="tipo">
                      <th mat-header-cell *matHeaderCellDef>Classificação</th>
                      <td mat-cell *matCellDef="let d">{{ d.tipoDocumentoDescricao }}</td>
                    </ng-container>
                    <ng-container matColumnDef="hash">
                      <th mat-header-cell *matHeaderCellDef>Hash SHA-256</th>
                      <td mat-cell *matCellDef="let d"><code>{{ d.hashSha256 | slice:0:18 }}...</code></td>
                    </ng-container>
                    <ng-container matColumnDef="acoes">
                      <th mat-header-cell *matHeaderCellDef class="text-right">Ações</th>
                      <td mat-cell *matCellDef="let d" class="text-right">
                        <a mat-icon-button color="primary" [href]="api.urlDownloadDocumentoPdf(d.id)" target="_blank">
                          <mat-icon>download</mat-icon>
                        </a>
                        <button mat-icon-button color="warn" (click)="removerPdf(d.id, esc.id)">
                          <mat-icon>delete</mat-icon>
                        </button>
                      </td>
                    </ng-container>
                    <tr mat-header-row *matHeaderRowDef="['nome', 'tipo', 'hash', 'acoes']"></tr>
                    <tr mat-row *matRowDef="let row; columns: ['nome', 'tipo', 'hash', 'acoes'];"></tr>
                  </table>
                </div>
              } @else {
                <div class="status-badge info p-4">
                  Salve a escritura primeiro para habilitar o upload de documentos PDF e o termo de retirada.
                </div>
              }
            </div>
          </mat-tab>
        </mat-tab-group>

        <div class="flex justify-end gap-2 mt-4">
          <button mat-stroked-button (click)="exibirWizard.set(false)">Fechar</button>
          <button mat-flat-button color="primary" (click)="salvarEscritura()">
            <mat-icon>save</mat-icon>
            Salvar Escritura e Atualizar Minuta
          </button>
        </div>
      </mat-card>
    }

    <!-- LISTAGEM PAGINADA (20 REGISTROS POR PÁGINA CONFORME RN DO TCC) -->
    <mat-card class="p-4">
      <div class="flex flex-wrap gap-3 items-center mb-4">
        <mat-form-field appearance="outline" class="flex-1">
          <mat-label>Pesquisar Escritura por Protocolo, Livro, Outorgante, Outorgado ou Matrícula</mat-label>
          <input matInput [(ngModel)]="termoBusca" (keyup.enter)="pesquisar()" placeholder="Ex.: 4119914266, Fabricio Lima, João Azevedo ou 2235" />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
        <button mat-flat-button color="primary" (click)="pesquisar()">Pesquisar</button>
      </div>

      <div class="table-container">
        <table mat-table [dataSource]="escrituras()">
          <ng-container matColumnDef="protocolo">
            <th mat-header-cell *matHeaderCellDef>Protocolo / Nº</th>
            <td mat-cell *matCellDef="let e">
              <strong class="text-primary">{{ e.numeroProtocolo }}</strong>
              <div class="text-sm text-muted">{{ e.numeroEscritura }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="livroFolhas">
            <th mat-header-cell *matHeaderCellDef>Livro / Folhas</th>
            <td mat-cell *matCellDef="let e">
              <strong>Livro {{ e.numeroLivro }}</strong>
              <div class="text-sm text-muted">Folhas {{ e.folhaInicial }} a {{ e.folhaFinal }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="partes">
            <th mat-header-cell *matHeaderCellDef>Outorgante → Outorgado</th>
            <td mat-cell *matCellDef="let e">
              <div><strong>{{ e.nomeOutorgante }}</strong></div>
              <div class="text-sm text-muted">→ {{ e.nomeOutorgado }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="imovel">
            <th mat-header-cell *matHeaderCellDef>Matrícula / Valor</th>
            <td mat-cell *matCellDef="let e">
              <div>Matrícula: {{ e.matriculaImovel || '-' }}</div>
              <div class="font-bold">{{ e.valorOperacao | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status / DOI</th>
            <td mat-cell *matCellDef="let e">
              <span class="status-badge" [ngClass]="e.statusEscritura === 'Finalizada' ? 'finalizada' : (e.statusEscritura === 'Inativa' ? 'inativo' : 'andamento')">
                {{ e.statusEscrituraDescricao }}
              </span>
              @if (e.dataEnvioDoi) {
                <div class="text-sm text-muted mt-2">DOI Exportado</div>
              }
            </td>
          </ng-container>

          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef class="text-right">Ações</th>
            <td mat-cell *matCellDef="let e" class="text-right">
              <button mat-icon-button color="primary" (click)="abrirDetalhe(e.id)" matTooltip="Abrir Minuta / Editar Escritura">
                <mat-icon>edit_document</mat-icon>
              </button>
              <a mat-icon-button color="primary" [href]="api.urlCertidaoEscrituraPdf(e.id)" target="_blank" matTooltip="Emitir Certidão PDF (UC-11)">
                <mat-icon>picture_as_pdf</mat-icon>
              </a>
              <a mat-icon-button color="primary" [href]="api.urlExportarDoiXml(e.id)" target="_blank" matTooltip="Exportar XML DOI (UC-10)">
                <mat-icon>code</mat-icon>
              </a>
              <button mat-icon-button color="warn" (click)="inativarEscritura(e)" matTooltip="Inativar com Justificativa Obrigatória">
                <mat-icon>block</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="colunas"></tr>
          <tr mat-row *matRowDef="let row; columns: colunas;"></tr>
        </table>
      </div>
    </mat-card>
  `
})
export class EscriturasComponent implements OnInit {
  readonly api = inject(SaecApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly escrituras = signal<EscrituraResumoDto[]>([]);
  readonly detalheAtual = signal<EscrituraDetalheDto | null>(null);
  readonly cartorios = signal<CartorioDto[]>([]);
  readonly atos = signal<AtoDto[]>([]);
  readonly protocolosDisponiveis = signal<OpcaoDto[]>([]);
  readonly clientesOpcoes = signal<OpcaoDto[]>([]);
  readonly exibirWizard = signal(false);

  readonly colunas = ['protocolo', 'livroFolhas', 'partes', 'imovel', 'status', 'acoes'];

  termoBusca = '';
  novoDocTipo: TipoDocumentoDigitalizado = 'DocumentosApresentados';
  novoDocDescricao = 'Matrícula e documentos pessoais conferidos';
  obsRetirada = 'Entregue ao comprador mediante apresentação de RG/CPF.';

  form = this.criarFormPadrao();

  ngOnInit(): void {
    this.api.listarCartorios().subscribe(c => this.cartorios.set(c));
    this.api.listarAtos().subscribe(a => this.atos.set(a));
    this.api.listarOpcoesClientes().subscribe(c => this.clientesOpcoes.set(c));
    this.api.listarProtocolosDisponiveis().subscribe(p => this.protocolosDisponiveis.set(p));
    this.pesquisar();
  }

  private criarFormPadrao() {
    return {
      idCartorio: 1,
      idProtocolo: 3,
      idAto: 1,
      idFuncionario: 2,
      idOutorgante: 1,
      idOutorgado: 2,
      numeroLivro: 1,
      folhaInicial: 7,
      folhaFinal: 9,
      valorOperacao: 500000,
      textoMinuta: '',
      imovel: {
        numeroRegistro: '2235',
        valorVenal: 400000,
        nomePrefeitura: 'Prefeitura Municipal de Campinas',
        descricaoImovel: 'Imóvel residencial na Avenida Prestes Maia, 241 - Jardim do Trevo, Campinas/SP.',
        valorImovel: 500000,
        areaMetrosQuadrados: 300,
        inscricaoMunicipal: '3411220099',
        cartorioRegistroImoveis: '2º Cartório de Registro de Imóveis de Campinas',
        endereco: {
          cep: '13040000',
          pais: 'Brasil',
          estado: 'SP',
          cidade: 'Campinas',
          bairro: 'Jardim do Trevo',
          logradouro: 'Avenida Prestes Maia',
          numero: '241',
          complemento: ''
        }
      }
    };
  }

  pesquisar(): void {
    this.api.listarEscrituras(this.termoBusca, 1, 20).subscribe(r => this.escrituras.set(r.itens));
  }

  novaEscritura(): void {
    this.detalheAtual.set(null);
    this.form = this.criarFormPadrao();
    this.api.listarProtocolosDisponiveis().subscribe(p => this.protocolosDisponiveis.set(p));
    this.exibirWizard.set(true);
  }

  abrirDetalhe(id: number): void {
    this.api.obterEscritura(id).subscribe(esc => {
      this.detalheAtual.set(esc);
      this.api.listarProtocolosDisponiveis(esc.idProtocolo).subscribe(p => this.protocolosDisponiveis.set(p));
      this.form = {
        idCartorio: esc.idCartorio,
        idProtocolo: esc.idProtocolo,
        idAto: esc.idAto,
        idFuncionario: esc.idFuncionario,
        idOutorgante: esc.outorgante.id,
        idOutorgado: esc.outorgado.id,
        numeroLivro: esc.numeroLivro,
        folhaInicial: esc.folhaInicial,
        folhaFinal: esc.folhaFinal,
        valorOperacao: esc.valorOperacao,
        textoMinuta: esc.textoMinuta,
        imovel: esc.imovel ? {
          numeroRegistro: esc.imovel.numeroRegistro,
          valorVenal: esc.imovel.valorVenal,
          nomePrefeitura: esc.imovel.nomePrefeitura,
          descricaoImovel: esc.imovel.descricaoImovel || '',
          valorImovel: esc.imovel.valorImovel,
          areaMetrosQuadrados: esc.imovel.areaMetrosQuadrados || 300,
          inscricaoMunicipal: esc.imovel.inscricaoMunicipal || '',
          cartorioRegistroImoveis: esc.imovel.cartorioRegistroImoveis || '',
          endereco: { ...esc.imovel.endereco, complemento: esc.imovel.endereco.complemento || '' }
        } : this.criarFormPadrao().imovel
      };
      this.exibirWizard.set(true);
    });
  }

  buscarCepImovel(): void {
    if (!this.form.imovel.endereco.cep) return;
    this.api.consultarCep(this.form.imovel.endereco.cep).subscribe(c => {
      this.form.imovel.endereco.logradouro = c.logradouro;
      this.form.imovel.endereco.bairro = c.bairro;
      this.form.imovel.endereco.cidade = c.cidade;
      this.form.imovel.endereco.estado = c.estado;
    });
  }

  salvarEscritura(): void {
    const payload = {
      idCartorio: this.form.idCartorio,
      idProtocolo: this.form.idProtocolo,
      idAto: this.form.idAto,
      idFuncionario: this.form.idFuncionario,
      idOutorgante: this.form.idOutorgante,
      novoOutorgante: null,
      idOutorgado: this.form.idOutorgado,
      novoOutorgado: null,
      imovel: this.form.imovel,
      certidaoOutroOrgao: null,
      numeroLivro: this.form.numeroLivro,
      folhaInicial: this.form.folhaInicial,
      folhaFinal: this.form.folhaFinal,
      valorOperacao: this.form.valorOperacao,
      textoMinuta: this.form.textoMinuta || null,
      statusEscritura: this.detalheAtual()?.statusEscritura ?? 'EmProcessoDeEscrituracao'
    };

    const atual = this.detalheAtual();
    const obs = atual
      ? this.api.atualizarEscritura(atual.id, payload)
      : this.api.criarEscritura(payload);

    obs.subscribe(salva => {
      this.detalheAtual.set(salva);
      this.form.textoMinuta = salva.textoMinuta;
      this.snackBar.open(`Escritura ${salva.numeroEscritura} salva e minuta atualizada!`, 'OK', { duration: 4000 });
      this.pesquisar();
    });
  }

  finalizarLavratura(id: number): void {
    this.api.finalizarLavratura(id, this.form.textoMinuta).subscribe(esc => {
      this.detalheAtual.set(esc);
      this.snackBar.open('Escritura lavrada com sucesso! Índice de livros atualizado e exportação DOI habilitada.', 'OK', { duration: 4500 });
      this.pesquisar();
    });
  }

  registrarRetirada(id: number): void {
    this.api.registrarRetiradaDocumentos(id, this.obsRetirada).subscribe(esc => {
      this.detalheAtual.set(esc);
      this.snackBar.open('Retirada de documentos registrada com sucesso!', 'OK', { duration: 3500 });
      this.pesquisar();
    });
  }

  selecionarArquivoPdf(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    const esc = this.detalheAtual();
    if (!file || !esc) return;

    if (!file.name.toLowerCase().endsWith('.pdf')) {
      this.snackBar.open('Regra de Negócio: Apenas arquivos no formato PDF são permitidos.', 'Fechar', { duration: 4000 });
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const base64 = reader.result as string;
      this.api.anexarPdfBase64({
        escrituraId: esc.id,
        clienteId: esc.outorgado.id,
        nomeArquivo: file.name,
        conteudoBase64: base64,
        tipoDocumento: this.novoDocTipo,
        descricao: this.novoDocDescricao
      }).subscribe(() => {
        this.snackBar.open(`Arquivo PDF "${file.name}" anexado com sucesso!`, 'OK', { duration: 3500 });
        this.abrirDetalhe(esc.id);
      });
    };
    reader.readAsDataURL(file);
  }

  removerPdf(docId: number, escId: number): void {
    this.api.removerDocumentoPdf(docId).subscribe(() => {
      this.snackBar.open('Documento PDF removido.', 'OK', { duration: 3000 });
      this.abrirDetalhe(escId);
    });
  }

  inativarEscritura(e: EscrituraResumoDto): void {
    this.dialog.open(ConfirmDialogComponent, {
      data: {
        titulo: 'Inativar Escritura (RN: Justificativa Obrigatória)',
        mensagem: `Informe o motivo da inativação da Escritura ${e.numeroEscritura} (Protocolo ${e.numeroProtocolo}):`,
        exigirMotivo: true,
        labelMotivo: 'Justificativa de inativação para registro na Auditoria',
        corConfirmar: 'warn',
        textoConfirmar: 'Inativar Escritura'
      }
    }).afterClosed().subscribe(res => {
      if (res?.confirmado && res.motivo) {
        this.api.inativarEscritura(e.id, res.motivo).subscribe(() => {
          this.snackBar.open('Escritura inativada e justificativa gravada no histórico de auditoria.', 'OK', { duration: 4000 });
          this.pesquisar();
        });
      }
    });
  }
}
