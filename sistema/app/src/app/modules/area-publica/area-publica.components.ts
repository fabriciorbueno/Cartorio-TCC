import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { SaecApiService } from '../../core/services/saec-api.service';
import { AuthService, UsuarioService } from '../../core/services/auth-core.services';
import { AndamentoProcessoPublicoDto } from '../../shared/models/saec.models';

/**
 * UC-12: Consultar Andamento de Processo (Web / Mobile Responsivo)
 * Permite ao cliente acompanhar as 4 fases da escritura informando apenas o Número de Protocolo.
 */
@Component({
  selector: 'app-consulta-andamento',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressBarModule,
    MatDividerModule
  ],
  template: `
    <div class="public-shell">
      <header class="public-header">
        <div class="header-inner">
          <div class="flex items-center gap-3">
            <mat-icon style="color:#e6c168;font-size:32px;width:32px;height:32px">account_balance</mat-icon>
            <div>
              <h1 style="margin:0;font-size:1.25rem">2º Tabelionato de Notas de Campinas — SAEC</h1>
              <span style="font-size:0.82rem;color:#cbd8ea">UC-12: Portal Público & Mobile de Consulta de Andamento de Escrituras</span>
            </div>
          </div>
          <a mat-flat-button color="primary" routerLink="/app/dashboard">
            <mat-icon>arrow_back</mat-icon>
            Acessar Sistema Interno (SAEC)
          </a>
        </div>
      </header>

      <main class="public-container">
        <mat-card class="p-6 mb-4">
          <div class="page-header">
            <div>
              <h1><mat-icon>travel_explore</mat-icon> Consultar Andamento de Processo (UC-12)</h1>
              <p>Informe o número do protocolo recebido no atendimento para acompanhar em tempo real a situação da sua escritura.</p>
            </div>
          </div>

          <div class="search-row">
            <mat-form-field appearance="outline" class="flex-1">
              <mat-label>Número do Protocolo (10 caracteres)</mat-label>
              <input matInput [(ngModel)]="numeroProtocolo" maxlength="10" placeholder="Ex.: 4119914266" (keyup.enter)="consultar()" />
              <mat-icon matSuffix>tag</mat-icon>
            </mat-form-field>
            <button mat-flat-button color="primary" class="search-btn" (click)="consultar()" [disabled]="carregando()">
              <mat-icon>search</mat-icon>
              Consultar Protocolo
            </button>
          </div>

          <div class="flex flex-wrap items-center gap-2 mt-2">
            <span class="text-sm text-muted">Protocolos de demonstração (Casos de Teste do TCC):</span>
            <button mat-stroked-button (click)="selecionarExemplo('4119914266')">
              4119914266 (Finalizada - CT-12 TCC)
            </button>
            <button mat-stroked-button (click)="selecionarExemplo('2026000002')">
              2026000002 (Em Escrituração)
            </button>
            <button mat-stroked-button (click)="selecionarExemplo('2026000003')">
              2026000003 (Aguardando Documentação)
            </button>
          </div>

          @if (carregando()) {
            <mat-progress-bar mode="indeterminate" class="mt-4"></mat-progress-bar>
          }

          @if (mensagemErro()) {
            <div class="status-badge erro mt-4 p-4 w-full">
              <mat-icon>error_outline</mat-icon>
              <span>{{ mensagemErro() }}</span>
            </div>
          }
        </mat-card>

        @if (resultado(); as proc) {
          <mat-card class="p-6">
            <div class="flex flex-wrap justify-between items-center gap-4 mb-4">
              <div>
                <span class="text-sm text-muted">Protocolo Localizado</span>
                <h2 style="margin:0.15rem 0;color:var(--saec-primary)">Protocolo Nº {{ proc.numeroProtocolo }}</h2>
                <p style="margin:0" class="text-sm text-muted">{{ proc.cartorioNome }} | Ato: <strong>{{ proc.naturezaAto }}</strong></p>
              </div>
              <span class="status-badge" [ngClass]="proc.statusAtual === 'Finalizada' ? 'finalizada' : 'andamento'">
                <mat-icon>{{ proc.statusAtual === 'Finalizada' ? 'check_circle' : 'schedule' }}</mat-icon>
                {{ proc.statusAtualDescricao }}
              </span>
            </div>

            <mat-divider class="mb-4"></mat-divider>

            <div class="form-grid mb-4">
              <div>
                <div class="text-sm text-muted">Solicitante / Apresentante</div>
                <div class="font-bold">{{ proc.nomeApresentante }}</div>
              </div>
              <div>
                <div class="text-sm text-muted">Escrevente Responsável</div>
                <div class="font-bold">{{ proc.escreventeResponsavel || 'Equipe Notarial SAEC' }}</div>
              </div>
              <div>
                <div class="text-sm text-muted">Data de Abertura</div>
                <div class="font-bold">{{ proc.dataSolicitacao | date:'dd/MM/yyyy HH:mm' }}</div>
              </div>
              @if (proc.numeroLivro) {
                <div>
                  <div class="text-sm text-muted">Livro / Folhas Lavradas</div>
                  <div class="font-bold">Livro {{ proc.numeroLivro }} — Folhas {{ proc.folhas }}</div>
                </div>
              }
            </div>

            <h3 class="text-primary mb-2">Linha do Tempo do Processo Notarial</h3>
            <div class="timeline-grid">
              @for (etapa of proc.etapas; track etapa.ordem) {
                <div class="timeline-step" [class.done]="etapa.concluida" [class.current]="etapa.atual">
                  <div class="step-header">
                    <span class="step-num">{{ etapa.ordem }}</span>
                    <mat-icon>{{ etapa.concluida ? 'task_alt' : (etapa.atual ? 'pending' : 'radio_button_unchecked') }}</mat-icon>
                  </div>
                  <div class="step-title">{{ etapa.titulo }}</div>
                  <div class="step-desc">{{ etapa.descricao }}</div>
                </div>
              }
            </div>

            @if (proc.statusAtual === 'Finalizada') {
              <div class="mt-4 p-4 flex flex-wrap justify-between items-center gap-3" style="background:var(--saec-success-bg);border-radius:8px">
                <div class="flex items-center gap-2" style="color:var(--saec-success)">
                  <mat-icon>verified</mat-icon>
                  <span><strong>Escritura Concluída!</strong> Os documentos e o traslado já estão disponíveis para retirada no cartório.</span>
                </div>
                <a mat-flat-button color="primary" [href]="api.urlCertidaoPorProtocoloPdf(proc.numeroProtocolo)" target="_blank">
                  <mat-icon>picture_as_pdf</mat-icon>
                  Baixar Certidão em PDF (UC-11)
                </a>
              </div>
            }
          </mat-card>
        }
      </main>
    </div>
  `,
  styles: [`
    .public-shell {
      min-height: 100vh;
      background: var(--saec-background);
    }
    .public-header {
      background: linear-gradient(90deg, #09274d, #0f3d75);
      color: #fff;
      padding: 1rem 1.5rem;
    }
    .header-inner {
      max-width: 1080px;
      margin: 0 auto;
      display: flex;
      flex-wrap: wrap;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;
    }
    .public-container {
      max-width: 1080px;
      margin: 2rem auto;
      padding: 0 1rem;
    }
    .search-row {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      align-items: flex-start;
    }
    .search-btn {
      height: 56px;
      padding: 0 1.75rem;
    }
    .timeline-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
      gap: 1rem;
    }
    .timeline-step {
      padding: 1rem;
      border-radius: 10px;
      border: 1px solid var(--saec-border);
      background: #fafbfd;
      transition: all 0.2s;
    }
    .timeline-step.done {
      border-color: #7ccca0;
      background: #f2fbf6;
    }
    .timeline-step.current {
      border-width: 2px;
      border-color: var(--saec-primary);
      background: var(--saec-primary-light);
    }
    .step-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.5rem;
    }
    .step-num {
      font-weight: 700;
      font-size: 0.85rem;
      color: var(--saec-primary);
    }
    .step-title {
      font-weight: 700;
      margin-bottom: 0.25rem;
    }
    .step-desc {
      font-size: 0.82rem;
      color: var(--saec-text-muted);
    }
  `]
})
export class ConsultaAndamentoComponent implements OnInit {
  readonly api = inject(SaecApiService);
  numeroProtocolo = '4119914266';
  readonly carregando = signal(false);
  readonly mensagemErro = signal('');
  readonly resultado = signal<AndamentoProcessoPublicoDto | null>(null);

  ngOnInit(): void {
    this.consultar();
  }

  selecionarExemplo(num: string): void {
    this.numeroProtocolo = num;
    this.consultar();
  }

  consultar(): void {
    if (!this.numeroProtocolo.trim()) {
      this.mensagemErro.set('Informe o número do protocolo para realizar a consulta.');
      return;
    }
    this.carregando.set(true);
    this.mensagemErro.set('');
    this.api.consultarAndamentoPublico(this.numeroProtocolo).subscribe({
      next: dados => {
        this.resultado.set(dados);
        this.carregando.set(false);
      },
      error: () => {
        this.carregando.set(false);
        this.resultado.set(null);
        this.mensagemErro.set('Nenhum processo encontrado para o número de protocolo informado.');
      }
    });
  }
}

/**
 * Tela de Login na Área Pública (Preparada para futura ativação do JWT corporativo)
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="login-backdrop">
      <mat-card class="login-card p-6">
        <div class="text-center mb-4">
          <mat-icon style="font-size:46px;width:46px;height:46px;color:var(--saec-primary)">account_balance</mat-icon>
          <h1 style="margin:0.5rem 0 0.2rem;color:var(--saec-primary)">SAEC — CartorioTcc</h1>
          <p class="text-sm text-muted" style="margin:0">2º Tabelionato de Notas de Campinas</p>
        </div>

        <div class="status-badge info p-4 mb-4 w-full" style="white-space:normal">
          <mat-icon>info</mat-icon>
          <span>
            <strong>Diretriz Arquitetural Ativa:</strong> A estrutura de Guards, Interceptores e Políticas JWT está criada, porém em modo <strong>Bypass Temporário</strong>. Todos os endpoints e telas estão liberados.
          </span>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>E-mail / Login do Funcionário</mat-label>
          <input matInput [(ngModel)]="email" placeholder="admin@saec.com" />
          <mat-icon matSuffix>person</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Senha (6 a 11 caracteres)</mat-label>
          <input matInput type="password" [(ngModel)]="senha" placeholder="Asd@123" />
          <mat-icon matSuffix>lock</mat-icon>
        </mat-form-field>

        <button mat-flat-button color="primary" class="w-full mb-2" style="height:46px" (click)="entrar()">
          <mat-icon>login</mat-icon>
          Entrar no SAEC (Bypass Ativo)
        </button>

        <a mat-stroked-button class="w-full" routerLink="/consulta-andamento">
          <mat-icon>travel_explore</mat-icon>
          Consultar Andamento de Protocolo (UC-12)
        </a>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-backdrop {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 1rem;
      background: linear-gradient(135deg, #09274d 0%, #0f3d75 55%, #1e6091 100%);
    }
    .login-card {
      width: 100%;
      max-width: 440px;
    }
  `]
})
export class LoginComponent {
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);
  readonly usuarioService = inject(UsuarioService);
  email = 'admin@saec.com';
  senha = 'Asd@123';

  entrar(): void {
    this.router.navigate(['/app/dashboard']);
  }
}
