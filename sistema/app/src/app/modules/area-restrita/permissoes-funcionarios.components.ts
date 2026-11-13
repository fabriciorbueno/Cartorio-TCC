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
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SaecApiService } from '../../core/services/saec-api.service';
import {
  FuncionarioResumoDto,
  PaginaDto,
  PerfilDto,
  PermissaoDto
} from '../../shared/models/saec.models';
import { CpfCnpjPipe, TelefoneBrPipe } from '../../shared/pipes/documento.pipe';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';

/**
 * UC-01: Gerenciar Permissões de Acesso (Associação Perfil x Página)
 */
@Component({
  selector: 'app-permissoes',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>admin_panel_settings</mat-icon> Gerenciar Permissões de Acesso (UC-01)</h1>
        <p>Controle granular de permissões por Perfil (Administrador, Tabelião, Escrevente, Auxiliar de Cartório) e Página do SAEC.</p>
      </div>
    </div>

    <mat-card class="p-4 mb-4">
      <div class="form-grid items-center">
        <mat-form-field appearance="outline">
          <mat-label>Filtrar por Perfil de Acesso</mat-label>
          <mat-select [(ngModel)]="perfilSelecionadoId" (selectionChange)="carregarPermissoes()">
            @for (p of perfis(); track p.id) {
              <mat-option [value]="p.id">{{ p.nomePerfil }}</mat-option>
            }
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Vincular Nova Página ao Perfil</mat-label>
          <mat-select [(ngModel)]="novaPaginaId">
            @for (pg of paginas(); track pg.id) {
              <mat-option [value]="pg.id">{{ pg.nomePagina }} ({{ pg.url }})</mat-option>
            }
          </mat-select>
        </mat-form-field>

        <div>
          <button mat-flat-button color="primary" (click)="adicionarPermissao()" [disabled]="!novaPaginaId">
            <mat-icon>add_moderator</mat-icon>
            Incluir Permissão no Perfil
          </button>
        </div>
      </div>
    </mat-card>

    <mat-card class="p-4">
      <div class="table-container">
        <table mat-table [dataSource]="permissoes()">
          <ng-container matColumnDef="modulo">
            <th mat-header-cell *matHeaderCellDef>Módulo</th>
            <td mat-cell *matCellDef="let p"><span class="status-badge info">{{ p.modulo || 'Geral' }}</span></td>
          </ng-container>

          <ng-container matColumnDef="pagina">
            <th mat-header-cell *matHeaderCellDef>Página / Funcionalidade</th>
            <td mat-cell *matCellDef="let p">
              <strong>{{ p.nomePagina }}</strong>
              <div class="text-sm text-muted">{{ p.urlPagina }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="visualizar">
            <th mat-header-cell *matHeaderCellDef class="text-center">Visualizar</th>
            <td mat-cell *matCellDef="let p" class="text-center">
              <mat-checkbox [(ngModel)]="p.podeVisualizar" (change)="salvarLinha(p)"></mat-checkbox>
            </td>
          </ng-container>

          <ng-container matColumnDef="incluir">
            <th mat-header-cell *matHeaderCellDef class="text-center">Incluir</th>
            <td mat-cell *matCellDef="let p" class="text-center">
              <mat-checkbox [(ngModel)]="p.podeIncluir" (change)="salvarLinha(p)"></mat-checkbox>
            </td>
          </ng-container>

          <ng-container matColumnDef="editar">
            <th mat-header-cell *matHeaderCellDef class="text-center">Editar</th>
            <td mat-cell *matCellDef="let p" class="text-center">
              <mat-checkbox [(ngModel)]="p.podeEditar" (change)="salvarLinha(p)"></mat-checkbox>
            </td>
          </ng-container>

          <ng-container matColumnDef="excluir">
            <th mat-header-cell *matHeaderCellDef class="text-center">Excluir</th>
            <td mat-cell *matCellDef="let p" class="text-center">
              <mat-checkbox [(ngModel)]="p.podeExcluir" (change)="salvarLinha(p)"></mat-checkbox>
            </td>
          </ng-container>

          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef class="text-right">Ações</th>
            <td mat-cell *matCellDef="let p" class="text-right">
              <button mat-icon-button color="warn" (click)="removerPermissao(p)" matTooltip="Remover permissão">
                <mat-icon>delete</mat-icon>
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
export class PermissoesComponent implements OnInit {
  private readonly api = inject(SaecApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly perfis = signal<PerfilDto[]>([]);
  readonly paginas = signal<PaginaDto[]>([]);
  readonly permissoes = signal<PermissaoDto[]>([]);
  readonly colunas = ['modulo', 'pagina', 'visualizar', 'incluir', 'editar', 'excluir', 'acoes'];

  perfilSelecionadoId = 1;
  novaPaginaId?: number;

  ngOnInit(): void {
    this.api.listarPerfis().subscribe(p => this.perfis.set(p));
    this.api.listarPaginas().subscribe(pg => this.paginas.set(pg));
    this.carregarPermissoes();
  }

  carregarPermissoes(): void {
    this.api.listarPermissoes(this.perfilSelecionadoId).subscribe(lista => this.permissoes.set(lista));
  }

  adicionarPermissao(): void {
    if (!this.novaPaginaId) return;
    this.api.incluirPermissao({
      idPerfil: this.perfilSelecionadoId,
      idPagina: this.novaPaginaId,
      podeVisualizar: true,
      podeIncluir: true,
      podeEditar: true,
      podeExcluir: false,
      status: 'Ativo'
    }).subscribe(() => {
      this.snackBar.open('Permissão vinculada ao perfil com sucesso!', 'OK', { duration: 3500 });
      this.carregarPermissoes();
    });
  }

  salvarLinha(p: PermissaoDto): void {
    this.api.alterarPermissao(p.id, {
      idPerfil: p.idPerfil,
      idPagina: p.idPagina,
      podeVisualizar: p.podeVisualizar,
      podeIncluir: p.podeIncluir,
      podeEditar: p.podeEditar,
      podeExcluir: p.podeExcluir,
      status: 'Ativo'
    }).subscribe(() => {
      this.snackBar.open(`Permissão da página "${p.nomePagina}" atualizada.`, 'OK', { duration: 2500 });
    });
  }

  removerPermissao(p: PermissaoDto): void {
    this.api.excluirPermissao(p.id).subscribe(() => {
      this.snackBar.open('Permissão removida.', 'OK', { duration: 3000 });
      this.carregarPermissoes();
    });
  }
}

/**
 * UC-02: Gerenciar Cadastro de Funcionários
 */
@Component({
  selector: 'app-funcionarios',
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
    CpfCnpjPipe,
    TelefoneBrPipe
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>badge</mat-icon> Gerenciar Cadastro de Funcionários (UC-02)</h1>
        <p>Cadastro de Tabeliães, Escreventes, Auxiliares e Administradores do 2º Tabelionato de Notas de Campinas.</p>
      </div>
      <button mat-flat-button color="primary" (click)="novoFormulario()">
        <mat-icon>person_add</mat-icon>
        Novo Funcionário
      </button>
    </div>

    @if (exibirFormulario()) {
      <mat-card class="p-6 mb-4">
        <h3 class="text-primary" style="margin-top:0">
          {{ editandoId() ? 'Editar Funcionário #' + editandoId() : 'Cadastrar Novo Funcionário (UC-02)' }}
        </h3>

        <div class="form-grid">
          <mat-form-field appearance="outline">
            <mat-label>Nome Completo (Apenas letras)</mat-label>
            <input matInput [(ngModel)]="form.nomeFuncionario" required />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>CPF (11 dígitos)</mat-label>
            <input matInput [(ngModel)]="form.cpfFuncionario" maxlength="14" required placeholder="000.000.000-00" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>RG</mat-label>
            <input matInput [(ngModel)]="form.rgFuncionario" required />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Data de Nascimento (Maior de 18 anos)</mat-label>
            <input matInput type="date" [(ngModel)]="form.dataNascFuncionario" required />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Data de Admissão</mat-label>
            <input matInput type="date" [(ngModel)]="form.dataAdmissao" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Cargo no Cartório</mat-label>
            <input matInput [(ngModel)]="form.cargo" placeholder="Ex.: Escrevente Autorizado" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Perfil de Acesso (UC-01)</mat-label>
            <mat-select [(ngModel)]="form.idPerfil" required>
              @for (p of perfis(); track p.id) {
                <mat-option [value]="p.id">{{ p.nomePerfil }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>E-mail Corporativo (Login)</mat-label>
            <input matInput type="email" [(ngModel)]="form.emailFuncionario" required />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Telefone / Celular</mat-label>
            <input matInput [(ngModel)]="form.telefoneFuncionario" placeholder="(19) 99999-9999" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>{{ editandoId() ? 'Nova Senha (Opcional, 6..11 chars)' : 'Senha (6 a 11 caracteres)' }}</mat-label>
            <input matInput type="password" [(ngModel)]="form.senha" minlength="6" maxlength="11" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Confirmar Senha</mat-label>
            <input matInput type="password" [(ngModel)]="form.confirmarSenha" minlength="6" maxlength="11" />
          </mat-form-field>
        </div>

        <h4 class="text-primary mb-2">Endereço Residencial (Auto-preenchimento via CEP / Refit)</h4>
        <div class="form-grid">
          <mat-form-field appearance="outline">
            <mat-label>CEP (8 dígitos)</mat-label>
            <input matInput [(ngModel)]="form.endereco.cep" maxlength="9" (blur)="buscarCep()" placeholder="13183091" />
            <button mat-icon-button matSuffix type="button" (click)="buscarCep()" matTooltip="Consultar CEP">
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
            <mat-label>Estado (UF)</mat-label>
            <input matInput [(ngModel)]="form.endereco.estado" maxlength="2" required />
          </mat-form-field>
        </div>

        <div class="flex justify-end gap-2 mt-4">
          <button mat-stroked-button (click)="exibirFormulario.set(false)">Cancelar</button>
          <button mat-flat-button color="primary" (click)="salvar()">
            <mat-icon>save</mat-icon>
            Salvar Funcionário
          </button>
        </div>
      </mat-card>
    }

    <mat-card class="p-4">
      <div class="flex flex-wrap gap-3 items-center mb-4">
        <mat-form-field appearance="outline" class="flex-1">
          <mat-label>Pesquisar Funcionário por Nome, CPF ou E-mail</mat-label>
          <input matInput [(ngModel)]="termoBusca" (keyup.enter)="pesquisar()" placeholder="Ex.: Daniel Luiz Granado ou Vanderlei Minussi" />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
        <button mat-flat-button color="primary" (click)="pesquisar()">Pesquisar</button>
      </div>

      <div class="table-container">
        <table mat-table [dataSource]="funcionarios()">
          <ng-container matColumnDef="nome">
            <th mat-header-cell *matHeaderCellDef>Nome / Cargo</th>
            <td mat-cell *matCellDef="let f">
              <strong>{{ f.nomeFuncionario }}</strong>
              <div class="text-sm text-muted">{{ f.cargo || 'Funcionário Notarial' }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="cpf">
            <th mat-header-cell *matHeaderCellDef>CPF / RG</th>
            <td mat-cell *matCellDef="let f">
              <div>{{ f.cpfFuncionario | cpfCnpj }}</div>
              <div class="text-sm text-muted">RG: {{ f.rgFuncionario }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="perfil">
            <th mat-header-cell *matHeaderCellDef>Perfil SAEC</th>
            <td mat-cell *matCellDef="let f"><span class="status-badge info">{{ f.nomePerfil }}</span></td>
          </ng-container>

          <ng-container matColumnDef="contato">
            <th mat-header-cell *matHeaderCellDef>E-mail / Telefone</th>
            <td mat-cell *matCellDef="let f">
              <div>{{ f.emailFuncionario }}</div>
              <div class="text-sm text-muted">{{ f.telefoneFuncionario | telefoneBr }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let f">
              <span class="status-badge" [ngClass]="f.statusFuncionario === 'Ativo' || f.statusFuncionario === 1 ? 'ativo' : 'inativo'">
                {{ f.statusFuncionario === 'Ativo' || f.statusFuncionario === 1 ? 'Ativo' : 'Inativo' }}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef class="text-right">Ações</th>
            <td mat-cell *matCellDef="let f" class="text-right">
              <button mat-icon-button color="primary" (click)="editar(f.id)" matTooltip="Editar funcionário">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="inativar(f)" matTooltip="Inativar funcionário">
                <mat-icon>person_off</mat-icon>
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
export class FuncionariosComponent implements OnInit {
  private readonly api = inject(SaecApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly perfis = signal<PerfilDto[]>([]);
  readonly funcionarios = signal<FuncionarioResumoDto[]>([]);
  readonly exibirFormulario = signal(false);
  readonly editandoId = signal<number | null>(null);
  readonly colunas = ['nome', 'cpf', 'perfil', 'contato', 'status', 'acoes'];

  termoBusca = '';
  form = this.criarFormVazio();

  ngOnInit(): void {
    this.api.listarPerfis().subscribe(p => this.perfis.set(p));
    this.pesquisar();
  }

  private criarFormVazio() {
    return {
      idPerfil: 3,
      nomeFuncionario: '',
      dataNascFuncionario: '1988-05-15',
      dataAdmissao: '2026-01-10',
      emailFuncionario: '',
      senha: '',
      confirmarSenha: '',
      cpfFuncionario: '',
      rgFuncionario: '',
      telefoneFuncionario: '',
      cargo: 'Escrevente Autorizado',
      endereco: {
        cep: '13183091',
        pais: 'Brasil',
        estado: 'SP',
        cidade: 'Hortolândia',
        bairro: 'Jardim Nova Hortolândia',
        logradouro: 'Rua Antonio Carlos',
        numero: '100',
        complemento: ''
      }
    };
  }

  pesquisar(): void {
    this.api.listarFuncionarios(this.termoBusca).subscribe(r => this.funcionarios.set(r.itens));
  }

  novoFormulario(): void {
    this.editandoId.set(null);
    this.form = this.criarFormVazio();
    this.exibirFormulario.set(true);
  }

  editar(id: number): void {
    this.api.obterFuncionario(id).subscribe(f => {
      this.editandoId.set(f.id);
      this.form = {
        idPerfil: f.idPerfil,
        nomeFuncionario: f.nomeFuncionario,
        dataNascFuncionario: f.dataNascFuncionario,
        dataAdmissao: f.dataAdmissao || '2020-01-01',
        emailFuncionario: f.emailFuncionario,
        senha: '',
        confirmarSenha: '',
        cpfFuncionario: f.cpfFuncionario,
        rgFuncionario: f.rgFuncionario,
        telefoneFuncionario: f.telefoneFuncionario || '',
        cargo: f.cargo || '',
        endereco: { ...f.endereco, complemento: f.endereco.complemento || '' }
      };
      this.exibirFormulario.set(true);
    });
  }

  buscarCep(): void {
    if (!this.form.endereco.cep) return;
    this.api.consultarCep(this.form.endereco.cep).subscribe(c => {
      this.form.endereco.logradouro = c.logradouro;
      this.form.endereco.bairro = c.bairro;
      this.form.endereco.cidade = c.cidade;
      this.form.endereco.estado = c.estado;
    });
  }

  salvar(): void {
    const id = this.editandoId();
    if (id) {
      this.api.alterarFuncionario(id, {
        id,
        idPerfil: this.form.idPerfil,
        nomeFuncionario: this.form.nomeFuncionario,
        dataNascFuncionario: this.form.dataNascFuncionario,
        dataAdmissao: this.form.dataAdmissao,
        emailFuncionario: this.form.emailFuncionario,
        novaSenha: this.form.senha || null,
        confirmarNovaSenha: this.form.confirmarSenha || null,
        cpfFuncionario: this.form.cpfFuncionario,
        rgFuncionario: this.form.rgFuncionario,
        telefoneFuncionario: this.form.telefoneFuncionario,
        cargo: this.form.cargo,
        endereco: this.form.endereco,
        statusFuncionario: 'Ativo'
      }).subscribe(() => {
        this.snackBar.open('Funcionário alterado com sucesso!', 'OK', { duration: 3500 });
        this.exibirFormulario.set(false);
        this.pesquisar();
      });
    } else {
      this.api.incluirFuncionario({
        ...this.form,
        statusFuncionario: 'Ativo'
      }).subscribe(() => {
        this.snackBar.open('Funcionário cadastrado com sucesso!', 'OK', { duration: 3500 });
        this.exibirFormulario.set(false);
        this.pesquisar();
      });
    }
  }

  inativar(f: FuncionarioResumoDto): void {
    this.dialog.open(ConfirmDialogComponent, {
      data: {
        titulo: 'Inativar Funcionário',
        mensagem: `Deseja realmente inativar o funcionário "${f.nomeFuncionario}"?`,
        corConfirmar: 'warn',
        textoConfirmar: 'Inativar'
      }
    }).afterClosed().subscribe(res => {
      if (res?.confirmado) {
        this.api.inativarFuncionario(f.id).subscribe(() => {
          this.snackBar.open('Funcionário inativado.', 'OK', { duration: 3000 });
          this.pesquisar();
        });
      }
    });
  }
}
