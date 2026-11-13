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
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SaecApiService } from '../../core/services/saec-api.service';
import {
  BackupDto,
  RelatorioLinhaDto,
  RelatorioResultadoDto,
  TipoRelatorioEnum
} from '../../shared/models/saec.models';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';

/**
 * UC-04: Gerar Relatórios Gerenciais (em tela com visualização de detalhes e em PDF)
 */
@Component({
  selector: 'app-relatorios',
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
    MatSnackBarModule,
    MatDialogModule,
    MatTooltipModule
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>analytics</mat-icon> Gerar Relatórios Gerenciais (UC-04)</h1>
        <p>Emissão de relatórios gerenciais em tela e em PDF com registro automático de auditoria do usuário solicitante.</p>
      </div>
    </div>

    <mat-card class="p-6 mb-4">
      <div class="form-grid">
        <mat-form-field appearance="outline">
          <mat-label>Tipo de Relatório (UC-04)</mat-label>
          <mat-select [(ngModel)]="tipoRelatorio" (selectionChange)="gerarEmTela()">
            <mat-option value="EscriturasCriadas">1. Relatório de Escrituras Criadas</mat-option>
            <mat-option value="ProtocolosAtendimento">2. Relatório de Protocolos de Atendimento</mat-option>
            <mat-option value="IndiceLivroNotas">3. Relatório de Índice de Livros e Notas</mat-option>
            <mat-option value="ClientesCadastrados">4. Relatório de Clientes Cadastrados</mat-option>
            <mat-option value="OrcamentosRealizados">5. Relatório Financeiro de Orçamentos CNB/SP</mat-option>
            <mat-option value="AuditoriaSistema">6. Relatório de Auditoria e Backups do Sistema</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Data Inicial (Opcional)</mat-label>
          <input matInput type="date" [(ngModel)]="dataInicio" />
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Data Final (Opcional)</mat-label>
          <input matInput type="date" [(ngModel)]="dataFim" />
        </mat-form-field>
      </div>

      <div class="flex justify-end gap-2">
        <button mat-stroked-button color="primary" (click)="gerarEmTela()">
          <mat-icon>table_view</mat-icon>
          Gerar Relatório na Tela
        </button>
        <button mat-flat-button color="primary" (click)="exportarPdf()">
          <mat-icon>picture_as_pdf</mat-icon>
          Gerar Relatório em PDF
        </button>
      </div>
    </mat-card>

    @if (relatorio(); as rel) {
      <mat-card class="p-4">
        <div class="flex flex-wrap justify-between items-center gap-2 mb-4">
          <div>
            <h3 class="text-primary" style="margin:0">{{ rel.tituloRelatorio }}</h3>
            <span class="text-sm text-muted">
              Emitido em {{ rel.dataGeracao | date:'dd/MM/yyyy HH:mm' }} por <strong>{{ rel.geradoPor }}</strong> |
              Registros: <strong>{{ rel.totalRegistros }}</strong> |
              Soma Financeira: <strong>{{ rel.somaValores | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
            </span>
          </div>
        </div>

        <div class="table-container">
          <table mat-table [dataSource]="rel.itens">
            <ng-container matColumnDef="codigo">
              <th mat-header-cell *matHeaderCellDef>Código</th>
              <td mat-cell *matCellDef="let i"><strong>{{ i.codigo }}</strong></td>
            </ng-container>

            <ng-container matColumnDef="titulo">
              <th mat-header-cell *matHeaderCellDef>Descrição / Parte Principal</th>
              <td mat-cell *matCellDef="let i">
                <div><strong>{{ i.titulo }}</strong></div>
                <div class="text-sm text-muted">{{ i.partePrincipal }}</div>
              </td>
            </ng-container>

            <ng-container matColumnDef="secundaria">
              <th mat-header-cell *matHeaderCellDef>Parte Secundária / Responsável</th>
              <td mat-cell *matCellDef="let i">{{ i.parteSecundaria }}</td>
            </ng-container>

            <ng-container matColumnDef="statusValor">
              <th mat-header-cell *matHeaderCellDef>Status / Valor</th>
              <td mat-cell *matCellDef="let i">
                <div><span class="status-badge info">{{ i.status }}</span></div>
                @if (i.valor) {
                  <div class="font-bold mt-2">{{ i.valor | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</div>
                }
              </td>
            </ng-container>

            <ng-container matColumnDef="detalhes">
              <th mat-header-cell *matHeaderCellDef class="text-right">Visualizar</th>
              <td mat-cell *matCellDef="let i" class="text-right">
                <button mat-stroked-button color="primary" (click)="verDetalhes(i)">
                  <mat-icon>visibility</mat-icon>
                  Visualizar
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="['codigo', 'titulo', 'secundaria', 'statusValor', 'detalhes']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['codigo', 'titulo', 'secundaria', 'statusValor', 'detalhes'];"></tr>
          </table>
        </div>
      </mat-card>
    }
  `
})
export class RelatoriosComponent implements OnInit {
  private readonly api = inject(SaecApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly relatorio = signal<RelatorioResultadoDto | null>(null);
  tipoRelatorio: TipoRelatorioEnum = 'EscriturasCriadas';
  dataInicio = '';
  dataFim = '';

  ngOnInit(): void {
    this.gerarEmTela();
  }

  gerarEmTela(): void {
    this.api.gerarRelatorioEmTela({
      tipoRelatorio: this.tipoRelatorio,
      dataInicio: this.dataInicio || null,
      dataFim: this.dataFim || null
    }).subscribe(r => this.relatorio.set(r));
  }

  exportarPdf(): void {
    this.api.baixarRelatorioPdf({
      tipoRelatorio: this.tipoRelatorio,
      dataInicio: this.dataInicio || null,
      dataFim: this.dataFim || null
    }).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Relatorio_${this.tipoRelatorio}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
      this.snackBar.open('Relatório PDF gerado com sucesso!', 'OK', { duration: 3500 });
    });
  }

  verDetalhes(item: RelatorioLinhaDto): void {
    this.dialog.open(ConfirmDialogComponent, {
      data: {
        titulo: `Detalhes do Registro — ${item.codigo}`,
        mensagem: `${item.titulo} | ${item.partePrincipal} | ${item.parteSecundaria} | Detalhes: ${item.detalhes}`,
        textoConfirmar: 'Fechar'
      }
    });
  }
}

/**
 * UC-03: Gerar Backup Criptografado Agendado e UC-05: Gerar Manualmente Backup Criptografado
 */
@Component({
  selector: 'app-backups',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>encrypted</mat-icon> Backups Criptografados Agendados & Manuais (UC-03 / UC-05)</h1>
        <p>Proteção do acervo notarial com criptografia AES-256-CBC e verificação de integridade SHA-256.</p>
      </div>
    </div>

    <div class="form-grid-2 mb-4">
      <mat-card class="p-6">
        <h3 class="text-primary" style="margin-top:0">UC-05: Gerar Manualmente Backup Criptografado</h3>
        <p class="text-sm text-muted">Gera imediatamente um pacote criptografado (.saec.enc) com todo o acervo notarial atual.</p>
        <mat-form-field appearance="outline">
          <mat-label>Descrição / Destino do Backup Manual</mat-label>
          <input matInput [(ngModel)]="descricaoManual" placeholder="Backup manual de segurança antes de fechamento de livro" />
        </mat-form-field>
        <button mat-flat-button color="primary" (click)="executarManual()">
          <mat-icon>security</mat-icon>
          Gerar Backup Criptografado Agora (AES-256)
        </button>
      </mat-card>

      <mat-card class="p-6">
        <h3 class="text-primary" style="margin-top:0">UC-03: Configurar Backup Criptografado Agendado</h3>
        <p class="text-sm text-muted">Agenda a execução automática em segundo plano pelo CartorioTcc.WorkerService.</p>
        <div class="form-grid-2">
          <mat-form-field appearance="outline">
            <mat-label>Data/Hora Agendada</mat-label>
            <input matInput type="datetime-local" [(ngModel)]="dataAgendada" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Identificação do Agendamento</mat-label>
            <input matInput [(ngModel)]="descricaoAgendada" />
          </mat-form-field>
        </div>
        <button mat-stroked-button color="primary" (click)="agendar()">
          <mat-icon>schedule_send</mat-icon>
          Programar Backup Automático
        </button>
      </mat-card>
    </div>

    <mat-card class="p-4">
      <h3 class="text-primary" style="margin-top:0">Histórico de Backups Criptografados (AES-256 + SHA-256)</h3>
      <div class="table-container">
        <table mat-table [dataSource]="backups()">
          <ng-container matColumnDef="arquivo">
            <th mat-header-cell *matHeaderCellDef>Arquivo Criptografado</th>
            <td mat-cell *matCellDef="let b">
              <strong>{{ b.nomeArquivoGerado }}</strong>
              <div class="text-sm text-muted">{{ b.descricaoBackup }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="tipo">
            <th mat-header-cell *matHeaderCellDef>Origem / Data</th>
            <td mat-cell *matCellDef="let b">
              <span class="status-badge info">{{ b.manual ? 'Manual (UC-05)' : 'Agendado (UC-03)' }}</span>
              <div class="text-sm text-muted mt-2">{{ b.dataBackup | date:'dd/MM/yyyy HH:mm' }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="hash">
            <th mat-header-cell *matHeaderCellDef>Hash SHA-256</th>
            <td mat-cell *matCellDef="let b"><code>{{ b.hashIntegridade | slice:0:22 }}...</code></td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let b">
              <span class="status-badge" [ngClass]="b.executado ? 'sucesso' : 'alerta'">
                {{ b.executado ? 'Criptografado (AES-256)' : 'Agendado na Fila' }}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="download">
            <th mat-header-cell *matHeaderCellDef class="text-right">Download</th>
            <td mat-cell *matCellDef="let b" class="text-right">
              @if (b.executado) {
                <a mat-stroked-button color="primary" [href]="api.urlDownloadBackup(b.id)" target="_blank">
                  <mat-icon>download</mat-icon>
                  Baixar .saec.enc
                </a>
              }
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="['arquivo', 'tipo', 'hash', 'status', 'download']"></tr>
          <tr mat-row *matRowDef="let row; columns: ['arquivo', 'tipo', 'hash', 'status', 'download'];"></tr>
        </table>
      </div>
    </mat-card>
  `
})
export class BackupsComponent implements OnInit {
  readonly api = inject(SaecApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly backups = signal<BackupDto[]>([]);
  descricaoManual = 'Backup manual criptografado AES-256 — C:\\Backups\\SAEC\\Manual';
  dataAgendada = '2026-11-14T23:00';
  descricaoAgendada = 'Backup diário automático noturno AES-256';

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.api.listarBackups().subscribe(b => this.backups.set(b));
  }

  executarManual(): void {
    this.api.executarBackupManual(this.descricaoManual).subscribe(novo => {
      this.snackBar.open(`Backup criptografado "${novo.nomeArquivoGerado}" gerado com sucesso!`, 'OK', { duration: 4000 });
      this.carregar();
    });
  }

  agendar(): void {
    this.api.agendarBackup(this.dataAgendada, this.descricaoAgendada).subscribe(() => {
      this.snackBar.open('Backup criptografado agendado com sucesso!', 'OK', { duration: 3500 });
      this.carregar();
    });
  }
}
