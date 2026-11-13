import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'app/dashboard'
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./modules/area-publica/area-publica.components').then(m => m.LoginComponent)
  },
  {
    path: 'consulta-andamento',
    loadComponent: () =>
      import('./modules/area-publica/area-publica.components').then(m => m.ConsultaAndamentoComponent)
  },
  {
    path: 'app',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard'
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./modules/area-restrita/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'permissoes',
        loadComponent: () =>
          import('./modules/area-restrita/permissoes-funcionarios.components').then(m => m.PermissoesComponent)
      },
      {
        path: 'funcionarios',
        loadComponent: () =>
          import('./modules/area-restrita/permissoes-funcionarios.components').then(m => m.FuncionariosComponent)
      },
      {
        path: 'orcamentos',
        loadComponent: () =>
          import('./modules/area-restrita/orcamentos-protocolos.components').then(m => m.OrcamentosComponent)
      },
      {
        path: 'protocolos',
        loadComponent: () =>
          import('./modules/area-restrita/orcamentos-protocolos.components').then(m => m.ProtocolosComponent)
      },
      {
        path: 'clientes',
        loadComponent: () =>
          import('./modules/area-restrita/clientes.component').then(m => m.ClientesComponent)
      },
      {
        path: 'escrituras',
        loadComponent: () =>
          import('./modules/area-restrita/escrituras.component').then(m => m.EscriturasComponent)
      },
      {
        path: 'indice-livros',
        loadComponent: () =>
          import('./modules/area-restrita/indice-doi-certidoes.components').then(m => m.IndiceLivrosComponent)
      },
      {
        path: 'envio-doi',
        loadComponent: () =>
          import('./modules/area-restrita/indice-doi-certidoes.components').then(m => m.EnvioDoiComponent)
      },
      {
        path: 'certidoes',
        loadComponent: () =>
          import('./modules/area-restrita/indice-doi-certidoes.components').then(m => m.CertidoesComponent)
      },
      {
        path: 'relatorios',
        loadComponent: () =>
          import('./modules/area-restrita/relatorios-backups.components').then(m => m.RelatoriosComponent)
      },
      {
        path: 'backups',
        loadComponent: () =>
          import('./modules/area-restrita/relatorios-backups.components').then(m => m.BackupsComponent)
      },
      {
        path: 'central-ajuda',
        loadComponent: () =>
          import('./modules/central-ajuda/central-ajuda.component').then(m => m.CentralAjudaComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'app/dashboard'
  }
];
