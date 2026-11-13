import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService, UsuarioService } from '../core/services/auth-core.services';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  uc?: string;
}

interface MenuGroup {
  titulo: string;
  itens: MenuItem[];
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatChipsModule,
    MatTooltipModule,
    MatDividerModule
  ],
  template: `
    <div class="shell-wrapper">
      <!-- HEADER -->
      <mat-toolbar class="shell-header">
        <button mat-icon-button (click)="sidenavOpened.set(!sidenavOpened())" aria-label="Alternar menu lateral">
          <mat-icon>menu</mat-icon>
        </button>

        <div class="brand" routerLink="/app/dashboard">
          <mat-icon class="brand-icon">account_balance</mat-icon>
          <div class="brand-texts">
            <span class="brand-title">SAEC — CartorioTcc</span>
            <span class="brand-subtitle">2º Tabelionato de Notas de Campinas</span>
          </div>
        </div>

        <span class="spacer"></span>

        <a mat-stroked-button class="public-btn" routerLink="/consulta-andamento" matTooltip="UC-12: Consulta Pública de Andamento por Protocolo">
          <mat-icon>travel_explore</mat-icon>
          <span>Consulta Pública (UC-12)</span>
        </a>

        <div class="auth-pill" matTooltip="Autenticação em Modo Bypass Temporário (Estrutura JWT pronta)">
          <mat-icon>verified_user</mat-icon>
          <span>Auth Bypass Ativo</span>
        </div>

        <!-- Seletor de Ator/Usuário Simulado -->
        <button mat-button [matMenuTriggerFor]="userMenu" class="user-trigger">
          <mat-icon>badge</mat-icon>
          <div class="user-meta">
            <span class="user-name">{{ usuarioService.usuarioAtual().nome }}</span>
            <span class="user-role">{{ usuarioService.usuarioAtual().perfil }}</span>
          </div>
          <mat-icon>expand_more</mat-icon>
        </button>

        <mat-menu #userMenu="matMenu">
          <div class="px-4 py-2 text-sm text-muted font-bold">Alternar Ator SAEC (Simulação de Escopo)</div>
          <mat-divider></mat-divider>
          @for (u of usuarioService.usuariosDisponiveis; track u.id) {
            <button mat-menu-item (click)="usuarioService.alternarUsuarioSimulado(u.id)">
              <mat-icon>{{ u.id === usuarioService.usuarioAtual().id ? 'radio_button_checked' : 'radio_button_unchecked' }}</mat-icon>
              <span>{{ u.nome }} ({{ u.perfil }})</span>
            </button>
          }
          <mat-divider></mat-divider>
          <a mat-menu-item routerLink="/login">
            <mat-icon>login</mat-icon>
            <span>Tela de Login (Preparada p/ JWT)</span>
          </a>
        </mat-menu>
      </mat-toolbar>

      <!-- SIDENAV + CONTENT -->
      <mat-sidenav-container class="shell-container">
        <mat-sidenav [opened]="sidenavOpened()" mode="side" class="shell-sidenav">
          <nav aria-label="Menu Principal SAEC">
            @for (grupo of gruposMenu; track grupo.titulo) {
              <div class="nav-group-title">{{ grupo.titulo }}</div>
              <mat-nav-list>
                @for (item of grupo.itens; track item.route) {
                  <a mat-list-item [routerLink]="item.route" routerLinkActive="active-link">
                    <mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
                    <span matListItemTitle>{{ item.label }}</span>
                    @if (item.uc) {
                      <span matListItemMeta class="uc-tag">{{ item.uc }}</span>
                    }
                  </a>
                }
              </mat-nav-list>
            }
          </nav>
        </mat-sidenav>

        <mat-sidenav-content class="shell-content">
          <!-- BREADCRUMB -->
          <div class="breadcrumb-bar">
            <a routerLink="/app/dashboard" class="breadcrumb-home">
              <mat-icon>home</mat-icon>
              <span>SAEC</span>
            </a>
            <mat-icon class="breadcrumb-sep">chevron_right</mat-icon>
            <span class="breadcrumb-current">{{ paginaAtualTitulo() }}</span>
          </div>

          <!-- ROUTER OUTLET -->
          <main class="main-viewport">
            <router-outlet></router-outlet>
          </main>

          <!-- FOOTER -->
          <footer class="shell-footer">
            <span><strong>SAEC</strong> — Sistema Automatizador de Escrituração para Cartórios | 2º Tabelionato de Notas de Campinas/SP</span>
            <span>Arquitetura .NET 10 (Clean Architecture / CQRS) + Angular 21.2.23 (Material Design 3)</span>
          </footer>
        </mat-sidenav-content>
      </mat-sidenav-container>
    </div>
  `,
  styles: [`
    .shell-wrapper {
      display: flex;
      flex-direction: column;
      height: 100vh;
      overflow: hidden;
    }
    .shell-header {
      background: linear-gradient(90deg, #09274d 0%, #0f3d75 60%, #164e94 100%);
      color: #fff;
      height: 64px;
      padding: 0 1rem;
      gap: 0.75rem;
      z-index: 10;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.18);
    }
    .brand {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      cursor: pointer;
    }
    .brand-icon {
      font-size: 28px;
      width: 28px;
      height: 28px;
      color: #e6c168;
    }
    .brand-texts {
      display: flex;
      flex-direction: column;
      line-height: 1.15;
    }
    .brand-title {
      font-weight: 700;
      font-size: 1.05rem;
      letter-spacing: 0.3px;
    }
    .brand-subtitle {
      font-size: 0.74rem;
      color: #cbd8ea;
    }
    .spacer {
      flex: 1 1 auto;
    }
    .public-btn {
      color: #fff !important;
      border-color: rgba(255, 255, 255, 0.35) !important;
      font-size: 0.82rem;
    }
    .auth-pill {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      padding: 0.25rem 0.65rem;
      border-radius: 999px;
      background: rgba(230, 193, 104, 0.18);
      border: 1px solid rgba(230, 193, 104, 0.45);
      color: #ffe299;
      font-size: 0.75rem;
      font-weight: 600;
    }
    .user-trigger {
      color: #fff !important;
    }
    .user-meta {
      display: inline-flex;
      flex-direction: column;
      align-items: flex-start;
      line-height: 1.1;
      margin: 0 0.35rem;
      text-align: left;
    }
    .user-name {
      font-size: 0.84rem;
      font-weight: 600;
    }
    .user-role {
      font-size: 0.7rem;
      color: #ffd984;
    }
    .shell-container {
      flex: 1;
      background: var(--saec-background);
    }
    .shell-sidenav {
      width: 278px;
      background: #ffffff;
      border-right: 1px solid var(--saec-border);
      padding-bottom: 1.5rem;
    }
    .nav-group-title {
      padding: 1rem 1rem 0.25rem;
      font-size: 0.72rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.8px;
      color: var(--saec-text-muted);
    }
    .uc-tag {
      font-size: 0.68rem;
      font-weight: 700;
      padding: 0.1rem 0.4rem;
      border-radius: 4px;
      background: var(--saec-primary-light);
      color: var(--saec-primary);
    }
    .active-link {
      background-color: var(--saec-primary-light) !important;
      color: var(--saec-primary) !important;
      font-weight: 600;
    }
    .shell-content {
      display: flex;
      flex-direction: column;
      min-height: 100%;
    }
    .breadcrumb-bar {
      display: flex;
      align-items: center;
      gap: 0.35rem;
      padding: 0.65rem 1.5rem;
      background: #fff;
      border-bottom: 1px solid var(--saec-border);
      font-size: 0.84rem;
    }
    .breadcrumb-home {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      color: var(--saec-primary);
      text-decoration: none;
      font-weight: 600;
    }
    .breadcrumb-sep {
      font-size: 18px;
      width: 18px;
      height: 18px;
      color: var(--saec-text-muted);
    }
    .breadcrumb-current {
      color: var(--saec-text-muted);
      font-weight: 500;
    }
    .main-viewport {
      flex: 1;
      padding: 1.5rem;
      max-width: 1480px;
      width: 100%;
      margin: 0 auto;
    }
    .shell-footer {
      display: flex;
      flex-wrap: wrap;
      justify-content: space-between;
      gap: 0.5rem;
      padding: 0.85rem 1.5rem;
      background: #fff;
      border-top: 1px solid var(--saec-border);
      font-size: 0.78rem;
      color: var(--saec-text-muted);
    }
  `]
})
export class MainLayoutComponent {
  readonly usuarioService = inject(UsuarioService);
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly sidenavOpened = signal(true);
  readonly paginaAtualTitulo = signal('Dashboard Notarial');

  readonly gruposMenu: MenuGroup[] = [
    {
      titulo: 'Visão Geral',
      itens: [
        { label: 'Painel & Indicadores', icon: 'dashboard', route: '/app/dashboard' }
      ]
    },
    {
      titulo: 'Atendimento e Recepção',
      itens: [
        { label: 'Orçamentos CNB/SP', icon: 'calculate', route: '/app/orcamentos', uc: 'UC-06' },
        { label: 'Protocolos', icon: 'Assignment', route: '/app/protocolos', uc: 'UC-07' },
        { label: 'Emitir Certidões', icon: 'workspace_premium', route: '/app/certidoes', uc: 'UC-11' }
      ]
    },
    {
      titulo: 'Escrituração Notarial',
      itens: [
        { label: 'Clientes & Cônjuges', icon: 'groups', route: '/app/clientes', uc: 'UC-08' },
        { label: 'Escrituras Públicas', icon: 'description', route: '/app/escrituras', uc: 'UC-09' },
        { label: 'Índice de Livros', icon: 'menu_book', route: '/app/indice-livros' }
      ]
    },
    {
      titulo: 'Obrigações & Gerencial',
      itens: [
        { label: 'Exportação DOI (RFB)', icon: 'upload_file', route: '/app/envio-doi', uc: 'UC-10' },
        { label: 'Relatórios Gerenciais', icon: 'analytics', route: '/app/relatorios', uc: 'UC-04' }
      ]
    },
    {
      titulo: 'Administração & Segurança',
      itens: [
        { label: 'Permissões de Perfil', icon: 'admin_panel_settings', route: '/app/permissoes', uc: 'UC-01' },
        { label: 'Funcionários', icon: 'badge', route: '/app/funcionarios', uc: 'UC-02' },
        { label: 'Backups Criptografados', icon: 'encrypted', route: '/app/backups', uc: 'UC-03/05' }
      ]
    },
    {
      titulo: 'Suporte & Documentação',
      itens: [
        { label: 'Central de Ajuda & TCC', icon: 'help_center', route: '/app/central-ajuda' }
      ]
    }
  ];

  constructor() {
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => {
      const url = this.router.url;
      for (const g of this.gruposMenu) {
        const found = g.itens.find(i => url.startsWith(i.route));
        if (found) {
          this.paginaAtualTitulo.set(found.uc ? `${found.label} (${found.uc})` : found.label);
          break;
        }
      }
    });
  }
}
