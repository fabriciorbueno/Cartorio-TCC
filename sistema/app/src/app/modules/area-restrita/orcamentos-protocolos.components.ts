import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
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
import { MatTooltipModule } from '@angular/material/tooltip';
import { SaecApiService } from '../../core/services/saec-api.service';
import {
  AtoDto,
  CalcularOrcamentoRequest,
  FaixaCustasDto,
  OpcaoDto,
  OrcamentoDto,
  ProtocoloDto,
  StatusEscritura
} from '../../shared/models/saec.models';
import { CpfCnpjPipe, TelefoneBrPipe } from '../../shared/pipes/documento.pipe';

/**
 * UC-06: Gerar Orçamento (Simulador e Tabela Oficial de Custas Notariais CNB/SP)
 */
@Component({
  selector: 'app-orcamentos',
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
    MatTooltipModule
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>calculate</mat-icon> Gerar Orçamento de Escrituras — Tabela CNB/SP (UC-06)</h1>
        <p>Cálculo automatizado de emolumentos notariais, custas estaduais, IPESP, TJ/SP, Registro Civil, MP e previsão de Registro de Imóveis.</p>
      </div>
    </div>

    <mat-tab-group>
      <mat-tab label="Simulador e Emissão de Orçamento (UC-06)">
        <div class="pt-4 form-grid-2 mt-4">
          <mat-card class="p-6">
            <h3 class="text-primary" style="margin-top:0">Parâmetros da Transação Imobiliária</h3>

            <div class="form-grid-2">
              <mat-form-field appearance="outline">
                <mat-label>Valor Base da Transação / Imóvel (R$)</mat-label>
                <input matInput type="number" [(ngModel)]="req.valorBaseAto" (ngModelChange)="simular()" min="1" step="1000" />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Cliente Solicitante (Opcional)</mat-label>
                <mat-select [(ngModel)]="req.idCliente">
                  <mat-option [value]="null">Atendimento Balcão</mat-option>
                  @for (c of clientesOpcoes(); track c.id) {
                    <mat-option [value]="c.id">{{ c.descricao }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
            </div>

            <div class="flex flex-col gap-2 mb-4">
              <mat-checkbox [(ngModel)]="req.oneroso" (change)="simular()">
                Ato Oneroso (Compra e Venda / Dação em Pagamento)
              </mat-checkbox>
              <mat-checkbox [(ngModel)]="req.hipoteca" (change)="simular()">
                Com Garantia Hipotecária / Alienação Fiduciária Simultânea (+25%)
              </mat-checkbox>
              <mat-checkbox [(ngModel)]="req.ufesp2500" (change)="simular()">
                Imóvel até 2.500 UFESPs — Habitação de Interesse Social (Redução de 40%)
              </mat-checkbox>
              <mat-checkbox [(ngModel)]="req.salarios30" (change)="simular()">
                Imóvel até 30 Salários Mínimos — 1ª Aquisição SFH (Redução de 50%)
              </mat-checkbox>
              <mat-checkbox [(ngModel)]="req.certidaoInclusa" (change)="simular()">
                Incluir Emissão de Certidão / Traslado Notarial
              </mat-checkbox>
            </div>

            <mat-form-field appearance="outline">
              <mat-label>Outras Despesas / Diligências (R$)</mat-label>
              <input matInput type="number" [(ngModel)]="req.outrosValores" (ngModelChange)="simular()" min="0" />
            </mat-form-field>

            <div class="flex justify-end gap-2 mt-2">
              <button mat-stroked-button (click)="carregarCasoTesteTcc()">
                Carregar Caso de Teste CT-06 (R$ 500.000,00)
              </button>
              <button mat-flat-button color="primary" (click)="salvarOrcamento()">
                <mat-icon>save</mat-icon>
                Registrar e Gerar Orçamento
              </button>
            </div>
          </mat-card>

          @if (simulacao(); as s) {
            <mat-card class="p-6">
              <h3 class="text-primary" style="margin-top:0">Demonstrativo Detalhado de Custas (CNB/SP)</h3>
              <div class="status-badge info mb-4">{{ s.faixaDescricao }}</div>

              <div class="custas-list">
                <div class="custas-row">
                  <span>Valor Base Declarado:</span>
                  <strong>{{ s.valorBaseAto | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
                </div>
                <div class="custas-row">
                  <span>Emolumentos do Tabelionato (Tabelião):</span>
                  <span>{{ s.emolumentosTabelionato | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-row">
                  <span>Receita do Estado de São Paulo:</span>
                  <span>{{ s.emolumentosEstado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-row">
                  <span>Contribuição IPESP / Carteira das Serventias:</span>
                  <span>{{ s.contribuicaoIpesp | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-row">
                  <span>Fundo de Compensação do Registro Civil (SINOREG):</span>
                  <span>{{ s.fundoRegistroCivil | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-row">
                  <span>Taxa de Fiscalização Judicial (TJ/SP):</span>
                  <span>{{ s.taxaTribunalJustica | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-row">
                  <span>Fundo Especial do Ministério Público (FEMP):</span>
                  <span>{{ s.fundoMinisterioPublico | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-row">
                  <span>ISSQN Municipal Estimado (5%):</span>
                  <span>{{ s.issqnEstimado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-row">
                  <span>Certidão / Traslado:</span>
                  <span>{{ s.valorCertidao | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-row">
                  <span>Previsão Tabela II — Registro de Imóveis:</span>
                  <span>{{ s.previsaoRegistroImoveis | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-row">
                  <span>Outros Valores:</span>
                  <span>{{ s.outrosValores | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
                </div>
                <div class="custas-total">
                  <span>VALOR TOTAL ESTIMADO:</span>
                  <strong>{{ s.valorTotalOrcamento | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
                </div>
              </div>
            </mat-card>
          }
        </div>

        <mat-card class="p-4 mt-4">
          <h3 class="text-primary" style="margin-top:0">Histórico de Orçamentos Registrados</h3>
          <div class="table-container">
            <table mat-table [dataSource]="orcamentos()">
              <ng-container matColumnDef="id">
                <th mat-header-cell *matHeaderCellDef>Nº</th>
                <td mat-cell *matCellDef="let o">#{{ o.id }}</td>
              </ng-container>
              <ng-container matColumnDef="cliente">
                <th mat-header-cell *matHeaderCellDef>Cliente</th>
                <td mat-cell *matCellDef="let o"><strong>{{ o.nomeCliente || 'Atendimento Balcão' }}</strong></td>
              </ng-container>
              <ng-container matColumnDef="faixa">
                <th mat-header-cell *matHeaderCellDef>Faixa CNB/SP</th>
                <td mat-cell *matCellDef="let o">{{ o.faixaDescricao }}</td>
              </ng-container>
              <ng-container matColumnDef="valorBase">
                <th mat-header-cell *matHeaderCellDef>Valor Base</th>
                <td mat-cell *matCellDef="let o">{{ o.valorBaseAto | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
              </ng-container>
              <ng-container matColumnDef="valorTotal">
                <th mat-header-cell *matHeaderCellDef>Total Orçado</th>
                <td mat-cell *matCellDef="let o"><strong>{{ o.valorTotalOrcamento | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong></td>
              </ng-container>
              <ng-container matColumnDef="acoes">
                <th mat-header-cell *matHeaderCellDef class="text-right">PDF</th>
                <td mat-cell *matCellDef="let o" class="text-right">
                  <a mat-icon-button color="primary" [href]="api.urlOrcamentoPdf(o.id)" target="_blank" matTooltip="Baixar Orçamento em PDF">
                    <mat-icon>picture_as_pdf</mat-icon>
                  </a>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="colunasOrc"></tr>
              <tr mat-row *matRowDef="let row; columns: colunasOrc;"></tr>
            </table>
          </div>
        </mat-card>
      </mat-tab>

      <mat-tab label="Tabela Oficial de Faixas de Custas CNB/SP (Lei 11.331/2002)">
        <mat-card class="p-4 mt-4">
          <div class="table-container">
            <table mat-table [dataSource]="faixas()">
              <ng-container matColumnDef="faixa">
                <th mat-header-cell *matHeaderCellDef>Faixa de Valor do Ato</th>
                <td mat-cell *matCellDef="let f"><strong>{{ f.descricaoFaixa }}</strong></td>
              </ng-container>
              <ng-container matColumnDef="tabeliao">
                <th mat-header-cell *matHeaderCellDef>Tabelião</th>
                <td mat-cell *matCellDef="let f">{{ f.emolumentosTabeliao | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
              </ng-container>
              <ng-container matColumnDef="estado">
                <th mat-header-cell *matHeaderCellDef>Estado</th>
                <td mat-cell *matCellDef="let f">{{ f.receitaEstado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
              </ng-container>
              <ng-container matColumnDef="ipesp">
                <th mat-header-cell *matHeaderCellDef>IPESP</th>
                <td mat-cell *matCellDef="let f">{{ f.contribuicaoIpesp | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
              </ng-container>
              <ng-container matColumnDef="totalNotarial">
                <th mat-header-cell *matHeaderCellDef>Total Notarial</th>
                <td mat-cell *matCellDef="let f"><strong>{{ f.totalNotarial | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong></td>
              </ng-container>
              <ng-container matColumnDef="registroImoveis">
                <th mat-header-cell *matHeaderCellDef>Previsão Reg. Imóveis</th>
                <td mat-cell *matCellDef="let f">{{ f.estimativaRegistroImoveis | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="colunasFaixas"></tr>
              <tr mat-row *matRowDef="let row; columns: colunasFaixas;"></tr>
            </table>
          </div>
        </mat-card>
      </mat-tab>
    </mat-tab-group>
  `,
  styles: [`
    .custas-list {
      display: flex;
      flex-direction: column;
      gap: 0.55rem;
    }
    .custas-row {
      display: flex;
      justify-content: space-between;
      border-bottom: 1px dashed var(--saec-border);
      padding-bottom: 0.35rem;
      font-size: 0.92rem;
    }
    .custas-total {
      display: flex;
      justify-content: space-between;
      margin-top: 0.5rem;
      padding: 0.85rem 1rem;
      background: var(--saec-primary-light);
      color: var(--saec-primary);
      border-radius: 8px;
      font-size: 1.1rem;
    }
  `]
})
export class OrcamentosComponent implements OnInit {
  readonly api = inject(SaecApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly clientesOpcoes = signal<OpcaoDto[]>([]);
  readonly orcamentos = signal<OrcamentoDto[]>([]);
  readonly faixas = signal<FaixaCustasDto[]>([]);
  readonly simulacao = signal<OrcamentoDto | null>(null);

  readonly colunasOrc = ['id', 'cliente', 'faixa', 'valorBase', 'valorTotal', 'acoes'];
  readonly colunasFaixas = ['faixa', 'tabeliao', 'estado', 'ipesp', 'totalNotarial', 'registroImoveis'];

  req: CalcularOrcamentoRequest = {
    valorBaseAto: 500000,
    idCliente: 2,
    oneroso: true,
    hipoteca: false,
    ufesp2500: true,
    salarios30: false,
    certidaoInclusa: true,
    outrosValores: 0
  };

  ngOnInit(): void {
    this.api.listarOpcoesClientes().subscribe(c => this.clientesOpcoes.set(c));
    this.api.obterTabelaCustas().subscribe(f => this.faixas.set(f));
    this.carregarHistorico();
    this.simular();
  }

  carregarCasoTesteTcc(): void {
    this.req = {
      valorBaseAto: 500000,
      idCliente: 2,
      oneroso: true,
      hipoteca: false,
      ufesp2500: true,
      salarios30: false,
      certidaoInclusa: true,
      outrosValores: 0
    };
    this.simular();
  }

  simular(): void {
    if (!this.req.valorBaseAto || this.req.valorBaseAto <= 0) return;
    this.api.simularOrcamento(this.req).subscribe(s => this.simulacao.set(s));
  }

  salvarOrcamento(): void {
    this.api.criarOrcamento(this.req).subscribe(novo => {
      this.simulacao.set(novo);
      this.snackBar.open(`Orçamento #${novo.id} registrado com sucesso!`, 'OK', { duration: 3500 });
      this.carregarHistorico();
    });
  }

  private carregarHistorico(): void {
    this.api.listarOrcamentos().subscribe(lista => this.orcamentos.set(lista));
  }
}

/**
 * UC-07: Gerenciar Protocolo de Atendimento (Geração automática do Número de Protocolo de 10 caracteres)
 */
@Component({
  selector: 'app-protocolos',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSnackBarModule,
    MatTooltipModule,
    CpfCnpjPipe,
    TelefoneBrPipe
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>assignment</mat-icon> Gerenciar Protocolos de Atendimento (UC-07)</h1>
        <p>Recepção de documentos, geração automática de protocolo alfanumérico (10 caracteres) e integração com a Consulta Pública (UC-12).</p>
      </div>
      <button mat-flat-button color="primary" (click)="exibirForm.set(!exibirForm())">
        <mat-icon>add_task</mat-icon>
        Gerar Novo Protocolo
      </button>
    </div>

    @if (exibirForm()) {
      <mat-card class="p-6 mb-4">
        <h3 class="text-primary" style="margin-top:0">Emitir Novo Protocolo de Recebimento de Documentos</h3>

        <div class="form-grid">
          <mat-form-field appearance="outline">
            <mat-label>Número do Protocolo (RN: Preenchimento Automático)</mat-label>
            <input matInput value="GERADO AUTOMATICAMENTE (10 DÍGITOS)" disabled />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Cliente Apresentante / Solicitante</mat-label>
            <mat-select [(ngModel)]="form.idCliente" required>
              @for (c of clientes(); track c.id) {
                <mat-option [value]="c.id">{{ c.descricao }}</mat-option>
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
            <mat-label>Documentos Entregues na Recepção</mat-label>
            <input matInput [(ngModel)]="form.documentosApresentados" placeholder="RG, CPF, Certidão de Casamento, Matrícula..." />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Documentos Pendentes / Faltantes</mat-label>
            <input matInput [(ngModel)]="form.documentosFaltantes" placeholder="Ex.: Guia de ITBI quitada" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Valor Estimado do Orçamento (R$)</mat-label>
            <input matInput type="number" [(ngModel)]="form.valorOrcamentoEstimado" />
          </mat-form-field>

          <mat-form-field appearance="outline" class="col-span-full">
            <mat-label>Observações de Atendimento</mat-label>
            <textarea matInput rows="2" [(ngModel)]="form.observacoes"></textarea>
          </mat-form-field>
        </div>

        <div class="flex justify-end gap-2">
          <button mat-stroked-button (click)="exibirForm.set(false)">Cancelar</button>
          <button mat-flat-button color="primary" (click)="criar()">
            <mat-icon>check</mat-icon>
            Gerar Protocolo de Atendimento
          </button>
        </div>
      </mat-card>
    }

    <mat-card class="p-4">
      <div class="flex flex-wrap gap-3 items-center mb-4">
        <mat-form-field appearance="outline" class="flex-1">
          <mat-label>Pesquisar por Número de Protocolo (ex.: 4119914266) ou Nome do Cliente</mat-label>
          <input matInput [(ngModel)]="termo" (keyup.enter)="carregar()" />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
        <button mat-flat-button color="primary" (click)="carregar()">Pesquisar</button>
      </div>

      <div class="table-container">
        <table mat-table [dataSource]="protocolos()">
          <ng-container matColumnDef="numero">
            <th mat-header-cell *matHeaderCellDef>Nº Protocolo (10 chars)</th>
            <td mat-cell *matCellDef="let p">
              <strong class="text-primary">{{ p.numeroProtocolo }}</strong>
              <div class="text-sm text-muted">{{ p.dtCriacao | date:'dd/MM/yyyy HH:mm' }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="solicitante">
            <th mat-header-cell *matHeaderCellDef>Solicitante / Contato</th>
            <td mat-cell *matCellDef="let p">
              <strong>{{ p.nomeCliente }}</strong> ({{ p.cpfCliente | cpfCnpj }})
              <div class="text-sm text-muted">{{ p.telefoneCliente | telefoneBr }} | {{ p.emailCliente }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="ato">
            <th mat-header-cell *matHeaderCellDef>Ato Solicitado / Escrevente</th>
            <td mat-cell *matCellDef="let p">
              <div>{{ p.tipoAto || p.naturezaPedido }}</div>
              <div class="text-sm text-muted">Resp.: {{ p.nomeEscrevente || 'A definir' }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="andamento">
            <th mat-header-cell *matHeaderCellDef>Fase do Processo (UC-12)</th>
            <td mat-cell *matCellDef="let p">
              <mat-select [value]="p.statusAndamento" (selectionChange)="atualizarStatus(p, $event.value)" class="status-select">
                <mat-option value="AguardandoDocumentacao">1. Aguardando documentação</mat-option>
                <mat-option value="EmProcessoDeEscrituracao">2. Em processo de escrituração</mat-option>
                <mat-option value="EmProcessoDeImpressao">3. Em processo de impressão</mat-option>
                <mat-option value="Finalizada">4. Finalizado – aguardando retirada</mat-option>
              </mat-select>
            </td>
          </ng-container>

          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef class="text-right">Comprovante PDF</th>
            <td mat-cell *matCellDef="let p" class="text-right">
              <a mat-icon-button color="primary" [href]="api.urlProtocoloPdf(p.id)" target="_blank" matTooltip="Imprimir Comprovante de Protocolo em PDF">
                <mat-icon>picture_as_pdf</mat-icon>
              </a>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="colunas"></tr>
          <tr mat-row *matRowDef="let row; columns: colunas;"></tr>
        </table>
      </div>
    </mat-card>
  `,
  styles: [`
    .status-select {
      max-width: 245px;
      font-size: 0.85rem;
      font-weight: 600;
    }
  `]
})
export class ProtocolosComponent implements OnInit {
  readonly api = inject(SaecApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly protocolos = signal<ProtocoloDto[]>([]);
  readonly clientes = signal<OpcaoDto[]>([]);
  readonly atos = signal<AtoDto[]>([]);
  readonly exibirForm = signal(false);
  readonly colunas = ['numero', 'solicitante', 'ato', 'andamento', 'acoes'];

  termo = '';
  form = {
    idCliente: 2,
    idAto: 1,
    idFuncionario: 2,
    documentosApresentados: 'RG, CPF, Certidão de Casamento e Matrícula Imobiliária',
    documentosFaltantes: '',
    valorOrcamentoEstimado: 5850,
    observacoes: 'Conferência documental iniciada na recepção.'
  };

  ngOnInit(): void {
    this.api.listarOpcoesClientes().subscribe(c => this.clientes.set(c));
    this.api.listarAtos().subscribe(a => this.atos.set(a));
    this.carregar();
  }

  carregar(): void {
    this.api.listarProtocolos(this.termo).subscribe(r => this.protocolos.set(r.itens));
  }

  criar(): void {
    this.api.criarProtocolo({
      ...this.form,
      naturezaPedido: 'Escritura Pública',
      statusAndamento: 'AguardandoDocumentacao'
    }).subscribe(novo => {
      this.snackBar.open(`Protocolo nº ${novo.numeroProtocolo} gerado com sucesso!`, 'OK', { duration: 4000 });
      this.exibirForm.set(false);
      this.carregar();
    });
  }

  atualizarStatus(p: ProtocoloDto, novoStatus: StatusEscritura): void {
    this.api.alterarProtocolo(p.id, {
      idCliente: p.idCliente,
      idAto: p.idAto,
      idFuncionario: p.idFuncionario,
      idOrcamento: p.idOrcamento,
      naturezaPedido: p.naturezaPedido,
      documentosApresentados: p.documentosApresentados,
      documentosFaltantes: p.documentosFaltantes,
      valorOrcamentoEstimado: p.valorOrcamentoEstimado,
      observacoes: p.observacoes,
      statusAndamento: novoStatus
    }).subscribe(() => {
      this.snackBar.open(`Andamento do protocolo ${p.numeroProtocolo} atualizado para consulta pública (UC-12).`, 'OK', { duration: 3000 });
      this.carregar();
    });
  }
}
