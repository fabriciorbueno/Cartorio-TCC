import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SaecApiService } from '../../core/services/saec-api.service';
import {
  ClienteResumoDto,
  DocumentoDigitalizadoDto
} from '../../shared/models/saec.models';
import { CpfCnpjPipe, TelefoneBrPipe } from '../../shared/pipes/documento.pipe';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';

/**
 * UC-08: Gerenciar Dados de Clientes (Dados Pessoais, Endereço, Cônjuge, Certidão de Casamento e Digitalizações PDF)
 */
@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatTabsModule,
    MatSnackBarModule,
    MatDialogModule,
    MatTooltipModule,
    CpfCnpjPipe,
    TelefoneBrPipe
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>groups</mat-icon> Gerenciar Dados de Clientes (UC-08)</h1>
        <p>Qualificação completa de Outorgantes, Outorgados, Cônjuges, Certidões de Casamento e Acervo Digitalizado (PDF).</p>
      </div>
      <button mat-flat-button color="primary" (click)="novoCliente()">
        <mat-icon>person_add</mat-icon>
        Novo Cliente
      </button>
    </div>

    @if (exibirFormulario()) {
      <mat-card class="p-6 mb-4">
        <div class="flex justify-between items-center mb-2">
          <h3 class="text-primary" style="margin:0">
            {{ editandoId() ? 'Editar Cliente #' + editandoId() : 'Cadastrar Novo Cliente (UC-08)' }}
          </h3>
          <button mat-stroked-button (click)="preencherCasoTesteMariaJose()">
            Preencher Caso de Teste CT-08 (Maria José - Casada)
          </button>
        </div>

        <mat-tab-group>
          <!-- ABA 1: DADOS PESSOAIS -->
          <mat-tab label="1. Dados Pessoais do Cliente">
            <div class="form-grid mt-4">
              <mat-form-field appearance="outline">
                <mat-label>Nome Completo (Apenas letras)</mat-label>
                <input matInput [(ngModel)]="form.nomeCliente" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>CPF (11 dígitos)</mat-label>
                <input matInput [(ngModel)]="form.cpfCliente" maxlength="14" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>RG</mat-label>
                <input matInput [(ngModel)]="form.rgCliente" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Órgão Expedidor / UF</mat-label>
                <input matInput [(ngModel)]="form.orgaoExpedidorCliente" placeholder="SSP" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Data de Nascimento (Maior de 18 anos)</mat-label>
                <input matInput type="date" [(ngModel)]="form.dataNascCliente" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Nacionalidade</mat-label>
                <input matInput [(ngModel)]="form.nacionalidadeCliente" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Profissão</mat-label>
                <input matInput [(ngModel)]="form.profissaoCliente" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Estado Civil</mat-label>
                <mat-select [(ngModel)]="form.estadoCivilCliente">
                  <mat-option value="Solteiro(a)">Solteiro(a)</mat-option>
                  <mat-option value="Casado(a)">Casado(a)</mat-option>
                  <mat-option value="Divorciado(a)">Divorciado(a)</mat-option>
                  <mat-option value="Viúvo(a)">Viúvo(a)</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Regime de Casamento / Bens</mat-label>
                <mat-select [(ngModel)]="form.regimeBens">
                  <mat-option value="">Não aplicável</mat-option>
                  <mat-option value="Comunhão Parcial de Bens">Comunhão Parcial de Bens</mat-option>
                  <mat-option value="Comunhão Universal de Bens">Comunhão Universal de Bens</mat-option>
                  <mat-option value="Separação Total de Bens">Separação Total de Bens</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>E-mail</mat-label>
                <input matInput type="email" [(ngModel)]="form.emailCliente" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Telefone / Celular</mat-label>
                <input matInput [(ngModel)]="form.telefoneCliente" required />
              </mat-form-field>
            </div>
          </mat-tab>

          <!-- ABA 2: ENDEREÇO -->
          <mat-tab label="2. Endereço do Cliente">
            <div class="form-grid mt-4">
              <mat-form-field appearance="outline">
                <mat-label>CEP (8 dígitos — Busca automática ViaCEP)</mat-label>
                <input matInput [(ngModel)]="form.endereco.cep" (blur)="buscarCepCliente()" />
                <button mat-icon-button matSuffix type="button" (click)="buscarCepCliente()">
                  <mat-icon>search</mat-icon>
                </button>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Logradouro</mat-label>
                <input matInput [(ngModel)]="form.endereco.logradouro" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Número</mat-label>
                <input matInput [(ngModel)]="form.endereco.numero" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Complemento</mat-label>
                <input matInput [(ngModel)]="form.endereco.complemento" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Bairro</mat-label>
                <input matInput [(ngModel)]="form.endereco.bairro" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Cidade</mat-label>
                <input matInput [(ngModel)]="form.endereco.cidade" required />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>UF</mat-label>
                <input matInput [(ngModel)]="form.endereco.estado" maxlength="2" required />
              </mat-form-field>
            </div>
          </mat-tab>

          <!-- ABA 3: CÔNJUGE E CERTIDÃO DE CASAMENTO -->
          <mat-tab label="3. Cônjuge & Certidão de Casamento">
            <div class="mt-4">
              @if (form.estadoCivilCliente === 'Casado(a)') {
                <div class="status-badge alerta mb-4">
                  <mat-icon>warning</mat-icon>
                  Regra de Negócio UC-08: Para clientes com estado civil "Casado(a)", os dados do Cônjuge e da Certidão de Casamento são obrigatórios.
                </div>
              }

              <h4 class="text-primary" style="margin-top:0">Qualificação do Cônjuge</h4>
              <div class="form-grid">
                <mat-form-field appearance="outline">
                  <mat-label>Nome do Cônjuge</mat-label>
                  <input matInput [(ngModel)]="form.conjuge.nomeConjuge" />
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>CPF do Cônjuge</mat-label>
                  <input matInput [(ngModel)]="form.conjuge.cpfConjuge" maxlength="14" />
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>RG do Cônjuge</mat-label>
                  <input matInput [(ngModel)]="form.conjuge.rgConjuge" />
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Data de Nascimento do Cônjuge</mat-label>
                  <input matInput type="date" [(ngModel)]="form.conjuge.dataNascConjuge" />
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Profissão do Cônjuge</mat-label>
                  <input matInput [(ngModel)]="form.conjuge.profissaoConjuge" />
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>E-mail do Cônjuge</mat-label>
                  <input matInput [(ngModel)]="form.conjuge.emailConjuge" />
                </mat-form-field>
              </div>

              <h4 class="text-primary mb-2">Dados da Certidão de Casamento</h4>
              <div class="form-grid">
                <mat-form-field appearance="outline">
                  <mat-label>Cartório de Registro Civil</mat-label>
                  <input matInput [(ngModel)]="form.certidaoCasamento.cartorioCertidaoCasamento" placeholder="Ex.: 2º Cartório de Notas de Campinas" />
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Livro</mat-label>
                  <input matInput [(ngModel)]="form.certidaoCasamento.livroCertidaoCasamento" />
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Folhas</mat-label>
                  <input matInput [(ngModel)]="form.certidaoCasamento.folhasCertidaoCasamento" />
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Termo</mat-label>
                  <input matInput [(ngModel)]="form.certidaoCasamento.termoCertidaoCasamento" />
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Data do Casamento</mat-label>
                  <input matInput type="date" [(ngModel)]="form.certidaoCasamento.dataCertidaoCasamento" />
                </mat-form-field>
              </div>
            </div>
          </mat-tab>
        </mat-tab-group>

        <div class="flex justify-end gap-2 mt-4">
          <button mat-stroked-button (click)="exibirFormulario.set(false)">Cancelar</button>
          <button mat-flat-button color="primary" (click)="salvarCliente()">
            <mat-icon>save</mat-icon>
            Salvar Cliente
          </button>
        </div>
      </mat-card>
    }

    <mat-tab-group>
      <mat-tab label="Clientes Cadastrados no SAEC">
        <mat-card class="p-4 mt-4">
          <div class="flex flex-wrap gap-3 items-center mb-4">
            <mat-form-field appearance="outline" class="flex-1">
              <mat-label>Pesquisar Cliente por Nome ou CPF</mat-label>
              <input matInput [(ngModel)]="termoBusca" (keyup.enter)="pesquisar()" placeholder="Ex.: Fabricio Lima dos Santos, João Azevedo ou Maria José" />
              <mat-icon matSuffix>search</mat-icon>
            </mat-form-field>
            <button mat-flat-button color="primary" (click)="pesquisar()">Pesquisar</button>
          </div>

          <div class="table-container">
            <table mat-table [dataSource]="clientes()">
              <ng-container matColumnDef="nome">
                <th mat-header-cell *matHeaderCellDef>Nome / Profissão</th>
                <td mat-cell *matCellDef="let c">
                  <strong>{{ c.nomeCliente }}</strong>
                  <div class="text-sm text-muted">{{ c.profissaoCliente }} | {{ c.cidadeEstado }}</div>
                </td>
              </ng-container>

              <ng-container matColumnDef="cpf">
                <th mat-header-cell *matHeaderCellDef>CPF / RG</th>
                <td mat-cell *matCellDef="let c">
                  <div>{{ c.cpfCliente | cpfCnpj }}</div>
                  <div class="text-sm text-muted">RG: {{ c.rgCliente }}</div>
                </td>
              </ng-container>

              <ng-container matColumnDef="estadoCivil">
                <th mat-header-cell *matHeaderCellDef>Estado Civil / Cônjuge</th>
                <td mat-cell *matCellDef="let c">
                  <div>{{ c.estadoCivilCliente }}</div>
                  @if (c.nomeConjuge) {
                    <div class="text-sm text-muted">Cônjuge: {{ c.nomeConjuge }}</div>
                  }
                </td>
              </ng-container>

              <ng-container matColumnDef="contato">
                <th mat-header-cell *matHeaderCellDef>Contato</th>
                <td mat-cell *matCellDef="let c">
                  <div>{{ c.emailCliente }}</div>
                  <div class="text-sm text-muted">{{ c.telefoneCliente | telefoneBr }}</div>
                </td>
              </ng-container>

              <ng-container matColumnDef="acoes">
                <th mat-header-cell *matHeaderCellDef class="text-right">Ações</th>
                <td mat-cell *matCellDef="let c" class="text-right">
                  <button mat-icon-button color="primary" (click)="editarCliente(c.id)" matTooltip="Editar cliente">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button color="warn" (click)="inativarCliente(c)" matTooltip="Inativar cliente">
                    <mat-icon>person_remove</mat-icon>
                  </button>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="colunas"></tr>
              <tr mat-row *matRowDef="let row; columns: colunas;"></tr>
            </table>
          </div>
        </mat-card>
      </mat-tab>

      <mat-tab label="Consulta de Documentos Digitalizados por Cliente (PDF)">
        <mat-card class="p-4 mt-4">
          <div class="flex flex-wrap gap-3 items-center mb-4">
            <mat-form-field appearance="outline" class="flex-1">
              <mat-label>Buscar Documentos Digitalizados por Nome ou CPF do Cliente (Requisito 1.1 do TCC)</mat-label>
              <input matInput [(ngModel)]="termoDocs" (keyup.enter)="pesquisarDocumentos()" placeholder="Ex.: Fabricio Lima" />
            </mat-form-field>
            <button mat-flat-button color="primary" (click)="pesquisarDocumentos()">Buscar PDFs</button>
          </div>

          <div class="table-container">
            <table mat-table [dataSource]="documentos()">
              <ng-container matColumnDef="arquivo">
                <th mat-header-cell *matHeaderCellDef>Arquivo PDF</th>
                <td mat-cell *matCellDef="let d">
                  <strong>{{ d.nomeArquivo }}</strong>
                  <div class="text-sm text-muted">{{ d.observacao }}</div>
                </td>
              </ng-container>
              <ng-container matColumnDef="cliente">
                <th mat-header-cell *matHeaderCellDef>Cliente Vinculado</th>
                <td mat-cell *matCellDef="let d">{{ d.nomeCliente }} ({{ d.cpfCliente | cpfCnpj }})</td>
              </ng-container>
              <ng-container matColumnDef="hash">
                <th mat-header-cell *matHeaderCellDef>Hash SHA-256</th>
                <td mat-cell *matCellDef="let d"><code>{{ d.hashSha256 | slice:0:16 }}...</code></td>
              </ng-container>
              <ng-container matColumnDef="download">
                <th mat-header-cell *matHeaderCellDef class="text-right">Download</th>
                <td mat-cell *matCellDef="let d" class="text-right">
                  <a mat-stroked-button color="primary" [href]="api.urlDownloadDocumentoPdf(d.id)" target="_blank">
                    <mat-icon>download</mat-icon> Baixar PDF
                  </a>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="['arquivo', 'cliente', 'hash', 'download']"></tr>
              <tr mat-row *matRowDef="let row; columns: ['arquivo', 'cliente', 'hash', 'download'];"></tr>
            </table>
          </div>
        </mat-card>
      </mat-tab>
    </mat-tab-group>
  `
})
export class ClientesComponent implements OnInit {
  readonly api = inject(SaecApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly clientes = signal<ClienteResumoDto[]>([]);
  readonly documentos = signal<DocumentoDigitalizadoDto[]>([]);
  readonly exibirFormulario = signal(false);
  readonly editandoId = signal<number | null>(null);
  readonly colunas = ['nome', 'cpf', 'estadoCivil', 'contato', 'acoes'];

  termoBusca = '';
  termoDocs = '';
  form = this.criarFormVazio();

  ngOnInit(): void {
    this.pesquisar();
    this.pesquisarDocumentos();
  }

  private criarFormVazio() {
    return {
      nomeCliente: '',
      rgCliente: '',
      orgaoExpedidorCliente: 'SSP',
      nacionalidadeCliente: 'Brasileiro(a)',
      estadoOrgaoExpedidorCliente: 'SP',
      cpfCliente: '',
      dataNascCliente: '1985-06-20',
      emailCliente: '',
      generoCliente: 'M',
      estadoCivilCliente: 'Solteiro(a)',
      regimeBens: '',
      profissaoCliente: '',
      telefoneCliente: '1932314455',
      endereco: {
        cep: '13040000',
        pais: 'Brasil',
        estado: 'SP',
        cidade: 'Campinas',
        bairro: 'Jardim do Trevo',
        logradouro: 'Avenida Prestes Maia',
        numero: '241',
        complemento: ''
      },
      conjuge: {
        nomeConjuge: '',
        rgConjuge: '',
        orgaoExpedidorConjuge: 'SSP',
        nacionalidadeConjuge: 'Brasileiro(a)',
        estadoOrgaoExpedidorConjuge: 'SP',
        cpfConjuge: '',
        dataNascConjuge: '1987-03-15',
        emailConjuge: '',
        generoConjuge: 'F',
        estadoCivilConjuge: 'Casado(a)',
        profissaoConjuge: '',
        telefoneConjuge: '1932314455'
      },
      certidaoCasamento: {
        cartorioCertidaoCasamento: '2º Cartório de Notas de Campinas',
        livroCertidaoCasamento: '1',
        folhasCertidaoCasamento: '23',
        termoCertidaoCasamento: '4512',
        dataCertidaoCasamento: '2010-09-10'
      }
    };
  }

  preencherCasoTesteMariaJose(): void {
    this.form = {
      nomeCliente: 'Maria José Conceição',
      rgCliente: '19887766',
      orgaoExpedidorCliente: 'SSP',
      nacionalidadeCliente: 'Brasileiro(a)',
      estadoOrgaoExpedidorCliente: 'SP',
      cpfCliente: '52998224725',
      dataNascCliente: '1975-01-30',
      emailCliente: 'maria.conceicao@email.com',
      generoCliente: 'F',
      estadoCivilCliente: 'Casado(a)',
      regimeBens: 'Comunhão Parcial de Bens',
      profissaoCliente: 'Professora',
      telefoneCliente: '1938874411',
      endereco: {
        cep: '13183091',
        pais: 'Brasil',
        estado: 'SP',
        cidade: 'Hortolândia',
        bairro: 'Jardim Nova Hortolândia',
        logradouro: 'Rua Antonio Carlos',
        numero: '85',
        complemento: ''
      },
      conjuge: {
        nomeConjuge: 'Raimundo José',
        rgConjuge: '18776655',
        orgaoExpedidorConjuge: 'SSP',
        nacionalidadeConjuge: 'Brasileiro(a)',
        estadoOrgaoExpedidorConjuge: 'SP',
        cpfConjuge: '11144477735',
        dataNascConjuge: '1973-04-22',
        emailConjuge: 'raimundo.jose@email.com',
        generoConjuge: 'M',
        estadoCivilConjuge: 'Casado(a)',
        profissaoConjuge: 'Comerciante',
        telefoneConjuge: '1938874411'
      },
      certidaoCasamento: {
        cartorioCertidaoCasamento: '2º Cartório de Notas de Campinas',
        livroCertidaoCasamento: '1',
        folhasCertidaoCasamento: '23',
        termoCertidaoCasamento: '4512',
        dataCertidaoCasamento: '2001-09-10'
      }
    };
  }

  pesquisar(): void {
    this.api.listarClientes(this.termoBusca).subscribe(r => this.clientes.set(r.itens));
  }

  pesquisarDocumentos(): void {
    this.api.consultarDocumentosPorCliente(this.termoDocs).subscribe(d => this.documentos.set(d));
  }

  novoCliente(): void {
    this.editandoId.set(null);
    this.form = this.criarFormVazio();
    this.exibirFormulario.set(true);
  }

  editarCliente(id: number): void {
    this.api.obterCliente(id).subscribe(c => {
      this.editandoId.set(c.id);
      this.form = {
        nomeCliente: c.nomeCliente,
        rgCliente: c.rgCliente,
        orgaoExpedidorCliente: c.orgaoExpedidorCliente,
        nacionalidadeCliente: c.nacionalidadeCliente,
        estadoOrgaoExpedidorCliente: c.estadoOrgaoExpedidorCliente,
        cpfCliente: c.cpfCliente,
        dataNascCliente: c.dataNascCliente,
        emailCliente: c.emailCliente,
        generoCliente: c.generoCliente,
        estadoCivilCliente: c.estadoCivilCliente,
        regimeBens: c.regimeBens || '',
        profissaoCliente: c.profissaoCliente,
        telefoneCliente: c.telefoneCliente,
        endereco: { ...c.endereco, complemento: c.endereco.complemento || '' },
        conjuge: c.conjuge ? {
          nomeConjuge: c.conjuge.nomeConjuge,
          rgConjuge: c.conjuge.rgConjuge,
          orgaoExpedidorConjuge: c.conjuge.orgaoExpedidorConjuge,
          nacionalidadeConjuge: c.conjuge.nacionalidadeConjuge,
          estadoOrgaoExpedidorConjuge: c.conjuge.estadoOrgaoExpedidorConjuge,
          cpfConjuge: c.conjuge.cpfConjuge,
          dataNascConjuge: c.conjuge.dataNascConjuge,
          emailConjuge: c.conjuge.emailConjuge,
          generoConjuge: c.conjuge.generoConjuge,
          estadoCivilConjuge: c.conjuge.estadoCivilConjuge,
          profissaoConjuge: c.conjuge.profissaoConjuge,
          telefoneConjuge: c.conjuge.telefoneConjuge
        } : this.criarFormVazio().conjuge,
        certidaoCasamento: c.certidaoCasamento ? {
          cartorioCertidaoCasamento: c.certidaoCasamento.cartorioCertidaoCasamento,
          livroCertidaoCasamento: c.certidaoCasamento.livroCertidaoCasamento,
          folhasCertidaoCasamento: c.certidaoCasamento.folhasCertidaoCasamento,
          termoCertidaoCasamento: c.certidaoCasamento.termoCertidaoCasamento,
          dataCertidaoCasamento: c.certidaoCasamento.dataCertidaoCasamento
        } : this.criarFormVazio().certidaoCasamento
      };
      this.exibirFormulario.set(true);
    });
  }

  buscarCepCliente(): void {
    if (!this.form.endereco.cep) return;
    this.api.consultarCep(this.form.endereco.cep).subscribe(c => {
      this.form.endereco.logradouro = c.logradouro;
      this.form.endereco.bairro = c.bairro;
      this.form.endereco.cidade = c.cidade;
      this.form.endereco.estado = c.estado;
    });
  }

  salvarCliente(): void {
    const isCasado = this.form.estadoCivilCliente === 'Casado(a)';
    const payload = {
      nomeCliente: this.form.nomeCliente,
      rgCliente: this.form.rgCliente,
      orgaoExpedidorCliente: this.form.orgaoExpedidorCliente,
      nacionalidadeCliente: this.form.nacionalidadeCliente,
      estadoOrgaoExpedidorCliente: this.form.estadoOrgaoExpedidorCliente,
      cpfCliente: this.form.cpfCliente,
      dataNascCliente: this.form.dataNascCliente,
      emailCliente: this.form.emailCliente,
      generoCliente: this.form.generoCliente,
      estadoCivilCliente: this.form.estadoCivilCliente,
      regimeBens: this.form.regimeBens || null,
      profissaoCliente: this.form.profissaoCliente,
      telefoneCliente: this.form.telefoneCliente,
      endereco: this.form.endereco,
      statusCliente: 'Ativo',
      conjuge: isCasado && this.form.conjuge.nomeConjuge ? {
        ...this.form.conjuge,
        endereco: this.form.endereco
      } : null,
      certidaoCasamento: isCasado && this.form.certidaoCasamento.cartorioCertidaoCasamento
        ? this.form.certidaoCasamento
        : null
    };

    const id = this.editandoId();
    const obs = id ? this.api.alterarCliente(id, payload) : this.api.incluirCliente(payload);
    obs.subscribe(() => {
      this.snackBar.open('Dados do cliente salvos com sucesso!', 'OK', { duration: 3500 });
      this.exibirFormulario.set(false);
      this.pesquisar();
    });
  }

  inativarCliente(c: ClienteResumoDto): void {
    this.dialog.open(ConfirmDialogComponent, {
      data: {
        titulo: 'Inativar Cliente',
        mensagem: `Deseja realmente inativar o cliente "${c.nomeCliente}"?`,
        corConfirmar: 'warn',
        textoConfirmar: 'Inativar'
      }
    }).afterClosed().subscribe(res => {
      if (res?.confirmado) {
        this.api.inativarCliente(c.id).subscribe(() => {
          this.snackBar.open('Cliente inativado.', 'OK', { duration: 3000 });
          this.pesquisar();
        });
      }
    });
  }
}
