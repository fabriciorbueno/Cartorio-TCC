import { Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import Chart from 'chart.js/auto';
import { SaecApiService } from '../../core/services/saec-api.service';
import { DashboardResumoDto } from '../../shared/models/saec.models';
import { CpfCnpjPipe } from '../../shared/pipes/documento.pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatTooltipModule,
    CpfCnpjPipe
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>dashboard</mat-icon> Painel de Controle Notarial — SAEC</h1>
        <p>2º Tabelionato de Notas de Campinas | Monitoramento de Escrituras, Protocolos, Custas CNB/SP, DOI e Backups Criptografados</p>
      </div>
      <div class="flex flex-wrap gap-2">
        <a mat-stroked-button color="primary" routerLink="/app/orcamentos">
          <mat-icon>calculate</mat-icon> Simular Orçamento (UC-06)
        </a>
        <a mat-stroked-button color="primary" routerLink="/app/protocolos">
          <mat-icon>assignment</mat-icon> Novo Protocolo (UC-07)
        </a>
        <a mat-flat-button color="primary" routerLink="/app/escrituras">
          <mat-icon>note_add</mat-icon> Gerenciar Escrituras (UC-09)
        </a>
      </div>
    </div>

    @if (resumo(); as d) {
      <!-- KPI CARDS -->
      <div class="kpi-grid mb-4">
        <mat-card class="kpi-card">
          <div class="kpi-icon blue"><mat-icon>groups</mat-icon></div>
          <div>
            <div class="kpi-label">Clientes Ativos (UC-08)</div>
            <div class="kpi-value">{{ d.totalClientesAtivos }}</div>
          </div>
        </mat-card>

        <mat-card class="kpi-card">
          <div class="kpi-icon amber"><mat-icon>assignment</mat-icon></div>
          <div>
            <div class="kpi-label">Protocolos Ativos (UC-07)</div>
            <div class="kpi-value">{{ d.totalProtocolosAtivos }}</div>
          </div>
        </mat-card>

        <mat-card class="kpi-card">
          <div class="kpi-icon green"><mat-icon>verified</mat-icon></div>
          <div>
            <div class="kpi-label">Escrituras Finalizadas / Em Andamento</div>
            <div class="kpi-value">{{ d.totalEscriturasFinalizadas }} / {{ d.totalEscriturasEmAndamento }}</div>
          </div>
        </mat-card>

        <mat-card class="kpi-card">
          <div class="kpi-icon gold"><mat-icon>payments</mat-icon></div>
          <div>
            <div class="kpi-label">Volume Imobiliário Escriturado</div>
            <div class="kpi-value">{{ d.valorTotalEscriturado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</div>
          </div>
        </mat-card>
      </div>

      <!-- CHARTS (Chart.js) -->
      <div class="form-grid-2 mb-4">
        <mat-card class="p-4">
          <h3 class="text-primary" style="margin-top:0">Escrituras por Status de Andamento (UC-09 / UC-12)</h3>
          <div class="chart-box">
            <canvas #statusChartCanvas></canvas>
          </div>
        </mat-card>

        <mat-card class="p-4">
          <h3 class="text-primary" style="margin-top:0">Escrituras por Natureza de Ato Notarial</h3>
          <div class="chart-box">
            <canvas #atoChartCanvas></canvas>
          </div>
        </mat-card>
      </div>

      <!-- ÚLTIMAS ESCRITURAS -->
      <mat-card class="p-4">
        <div class="flex justify-between items-center mb-4">
          <h3 class="text-primary" style="margin:0">Últimas Escrituras Registradas no Acervo</h3>
          <a mat-button color="primary" routerLink="/app/escrituras">Ver Todas as Escrituras</a>
        </div>

        <div class="table-container">
          <table mat-table [dataSource]="d.ultimasEscrituras">
            <ng-container matColumnDef="protocolo">
              <th mat-header-cell *matHeaderCellDef>Protocolo</th>
              <td mat-cell *matCellDef="let e"><strong>{{ e.numeroProtocolo }}</strong></td>
            </ng-container>

            <ng-container matColumnDef="livroFolha">
              <th mat-header-cell *matHeaderCellDef>Livro / Folhas</th>
              <td mat-cell *matCellDef="let e">Livro {{ e.numeroLivro }} | Fls. {{ e.folhaInicial }}-{{ e.folhaFinal }}</td>
            </ng-container>

            <ng-container matColumnDef="ato">
              <th mat-header-cell *matHeaderCellDef>Natureza do Ato</th>
              <td mat-cell *matCellDef="let e">{{ e.tipoAto }}</td>
            </ng-container>

            <ng-container matColumnDef="partes">
              <th mat-header-cell *matHeaderCellDef>Outorgante → Outorgado</th>
              <td mat-cell *matCellDef="let e">
                <div><strong>{{ e.nomeOutorgante }}</strong> ({{ e.cpfOutorgante | cpfCnpj }})</div>
                <div class="text-sm text-muted">→ {{ e.nomeOutorgado }} ({{ e.cpfOutorgado | cpfCnpj }})</div>
              </td>
            </ng-container>

            <ng-container matColumnDef="valor">
              <th mat-header-cell *matHeaderCellDef>Valor Operação</th>
              <td mat-cell *matCellDef="let e">{{ e.valorOperacao | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
            </ng-container>

            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let e">
                <span class="status-badge" [ngClass]="e.statusEscritura === 'Finalizada' ? 'finalizada' : 'andamento'">
                  {{ e.statusEscrituraDescricao }}
                </span>
              </td>
            </ng-container>

            <ng-container matColumnDef="acoes">
              <th mat-header-cell *matHeaderCellDef class="text-right">Ações Rápidas</th>
              <td mat-cell *matCellDef="let e" class="text-right">
                <a mat-icon-button color="primary" [href]="api.urlCertidaoEscrituraPdf(e.id)" target="_blank" matTooltip="Emitir Certidão PDF (UC-11)">
                  <mat-icon>picture_as_pdf</mat-icon>
                </a>
                <a mat-icon-button color="primary" [href]="api.urlExportarDoiXml(e.id)" target="_blank" matTooltip="Exportar XML DOI - RFB (UC-10)">
                  <mat-icon>code</mat-icon>
                </a>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="colunas"></tr>
            <tr mat-row *matRowDef="let row; columns: colunas;"></tr>
          </table>
        </div>
      </mat-card>
    }
  `,
  styles: [`
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 1rem;
    }
    .kpi-card {
      display: flex;
      flex-direction: row !important;
      align-items: center;
      gap: 1rem;
      padding: 1.25rem;
    }
    .kpi-icon {
      width: 52px;
      height: 52px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      &.blue { background: #e8f0fe; color: #0f3d75; }
      &.amber { background: #fef7e0; color: #b06000; }
      &.green { background: #e6f4ea; color: #1b7f4b; }
      &.gold { background: #fbf3e4; color: #946c1e; }
    }
    .kpi-label {
      font-size: 0.82rem;
      color: var(--saec-text-muted);
    }
    .kpi-value {
      font-size: 1.4rem;
      font-weight: 700;
      color: var(--saec-text);
    }
    .chart-box {
      position: relative;
      height: 240px;
      width: 100%;
    }
  `]
})
export class DashboardComponent implements OnInit {
  readonly api = inject(SaecApiService);
  readonly resumo = signal<DashboardResumoDto | null>(null);
  readonly colunas = ['protocolo', 'livroFolha', 'ato', 'partes', 'valor', 'status', 'acoes'];

  @ViewChild('statusChartCanvas') statusCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('atoChartCanvas') atoCanvas?: ElementRef<HTMLCanvasElement>;
  private statusChart?: Chart;
  private atoChart?: Chart;

  ngOnInit(): void {
    this.api.obterDashboard().subscribe(d => {
      this.resumo.set(d);
      setTimeout(() => this.renderizarGraficos(d), 50);
    });
  }

  private renderizarGraficos(d: DashboardResumoDto): void {
    if (this.statusCanvas?.nativeElement) {
      this.statusChart?.destroy();
      const labels = Object.keys(d.escriturasPorStatus);
      const values = Object.values(d.escriturasPorStatus);
      this.statusChart = new Chart(this.statusCanvas.nativeElement, {
        type: 'doughnut',
        data: {
          labels: labels.length ? labels : ['Finalizadas', 'Em Escrituração'],
          datasets: [{
            data: values.length ? values : [1, 1],
            backgroundColor: ['#1b7f4b', '#0f3d75', '#b06000', '#c5221f']
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }

    if (this.atoCanvas?.nativeElement) {
      this.atoChart?.destroy();
      const labels = Object.keys(d.escriturasPorNaturezaAto);
      const values = Object.values(d.escriturasPorNaturezaAto);
      this.atoChart = new Chart(this.atoCanvas.nativeElement, {
        type: 'bar',
        data: {
          labels: labels.length ? labels : ['Compra e Venda'],
          datasets: [{
            label: 'Quantidade de Escrituras',
            data: values.length ? values : [2],
            backgroundColor: '#0f3d75'
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
    }
  }
}
