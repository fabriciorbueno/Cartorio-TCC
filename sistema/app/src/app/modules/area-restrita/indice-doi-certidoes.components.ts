import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SaecApiService } from '../../core/services/saec-api.service';
import { IndiceEscrituraDto } from '../../shared/models/saec.models';
import { CpfCnpjPipe } from '../../shared/pipes/documento.pipe';

/**
 * Índice Automático de Livros e Notas (Requisito Funcional 1.1 / 1.3 do TCC)
 */
@Component({
  selector: 'app-indice-livros',
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
    MatTooltipModule,
    CpfCnpjPipe
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>menu_book</mat-icon> Índice Automático de Livros e Notas Notariais</h1>
        <p>Gerado automaticamente ao finalizar a lavratura das escrituras, eliminando a digitação manual ao final de cada livro.</p>
      </div>
    </div>

    <mat-card class="p-4">
      <div class="flex flex-wrap gap-3 items-center mb-4">
        <mat-form-field appearance="outline" style="max-width:180px">
          <mat-label>Filtrar Nº do Livro</mat-label>
          <input matInput type="number" [(ngModel)]="numeroLivro" (keyup.enter)="carregar()" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="flex-1">
          <mat-label>Pesquisar por Outorgante, Outorgado, Protocolo ou Matrícula</mat-label>
          <input matInput [(ngModel)]="termo" (keyup.enter)="carregar()" />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>

        <button mat-flat-button color="primary" (click)="carregar()">Consultar Índice</button>
      </div>

      <div class="table-container">
        <table mat-table [dataSource]="itens()">
          <ng-container matColumnDef="livroFolha">
            <th mat-header-cell *matHeaderCellDef>Livro / Folhas</th>
            <td mat-cell *matCellDef="let i">
              <strong>Livro {{ i.numeroLivro }}</strong> | Fls. {{ i.folhaInicial }} a {{ i.folhaFinal }}
              <div class="text-sm text-muted">Protocolo: {{ i.numeroProtocolo }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="ato">
            <th mat-header-cell *matHeaderCellDef>Ato Lavrado</th>
            <td mat-cell *matCellDef="let i">
              <div>{{ i.tipoAto }}</div>
              <div class="text-sm text-muted">Lavrada em {{ i.dataLavratura | date:'dd/MM/yyyy' }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="partes">
            <th mat-header-cell *matHeaderCellDef>Partes (Outorgante / Outorgado)</th>
            <td mat-cell *matCellDef="let i">
              <div><strong>Outorgante:</strong> {{ i.nomeOutorgante }} ({{ i.cpfOutorgante | cpfCnpj }})</div>
              <div><strong>Outorgado:</strong> {{ i.nomeOutorgado }} ({{ i.cpfOutorgado | cpfCnpj }})</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="valor">
            <th mat-header-cell *matHeaderCellDef>Matrícula / Valor</th>
            <td mat-cell *matCellDef="let i">
              <div>Matrícula: {{ i.matriculaImovel || '-' }}</div>
              <div class="font-bold">{{ i.valorOperacao | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="acoes">
            <th mat-header-cell *matHeaderCellDef class="text-right">Certidão / DOI</th>
            <td mat-cell *matCellDef="let i" class="text-right">
              <a mat-icon-button color="primary" [href]="api.urlCertidaoEscrituraPdf(i.idEscritura)" target="_blank" matTooltip="Emitir Certidão PDF">
                <mat-icon>picture_as_pdf</mat-icon>
              </a>
              <a mat-icon-button color="primary" [href]="api.urlExportarDoiXml(i.idEscritura)" target="_blank" matTooltip="Exportar XML DOI">
                <mat-icon>code</mat-icon>
              </a>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="colunas"></tr>
          <tr mat-row *matRowDef="let row; columns: colunas;"></tr>
        </table>
      </div>
    </mat-card>
  `
})
export class IndiceLivrosComponent implements OnInit {
  readonly api = inject(SaecApiService);
  readonly itens = signal<IndiceEscrituraDto[]>([]);
  readonly colunas = ['livroFolha', 'ato', 'partes', 'valor', 'acoes'];
  numeroLivro?: number;
  termo = '';

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.api.consultarIndiceLivros(this.termo, this.numeroLivro).subscribe(r => this.itens.set(r.itens));
  }
}

/**
 * UC-10: Gerar Arquivo para Envio DOI (Declaração sobre Operações Imobiliárias - Receita Federal)
 */
@Component({
  selector: 'app-envio-doi',
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
    MatTooltipModule,
    CpfCnpjPipe
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>upload_file</mat-icon> Gerar Arquivo XML para Envio DOI — Receita Federal (UC-10)</h1>
        <p>Exportação automatizada de escrituras lavradas no formato XML oficial da Receita Federal ("yyyy-MM-dd-numeroProtocolo.xml").</p>
      </div>
    </div>

    <mat-card class="p-4">
      <div class="flex flex-wrap gap-3 items-center mb-4">
        <mat-form-field appearance="outline" class="flex-1">
          <mat-label>Pesquisar Escritura Lavrada por Protocolo (ex.: 4119914266) ou Nome das Partes</mat-label>
          <input matInput [(ngModel)]="termo" (keyup.enter)="carregar()" />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
        <button mat-flat-button color="primary" (click)="carregar()">Pesquisar</button>
      </div>

      <div class="table-container">
        <table mat-table [dataSource]="itens()">
          <ng-container matColumnDef="protocolo">
            <th mat-header-cell *matHeaderCellDef>Protocolo / Escritura</th>
            <td mat-cell *matCellDef="let i">
              <strong class="text-primary">{{ i.numeroProtocolo }}</strong>
              <div class="text-sm text-muted">Livro {{ i.numeroLivro }} | Fls. {{ i.folhaInicial }}-{{ i.folhaFinal }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="partes">
            <th mat-header-cell *matHeaderCellDef>Alienante → Adquirente</th>
            <td mat-cell *matCellDef="let i">
              <div><strong>{{ i.nomeOutorgante }}</strong> ({{ i.cpfOutorgante | cpfCnpj }})</div>
              <div class="text-sm text-muted">→ {{ i.nomeOutorgado }} ({{ i.cpfOutorgado | cpfCnpj }})</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="valor">
            <th mat-header-cell *matHeaderCellDef>Matrícula / Valor</th>
            <td mat-cell *matCellDef="let i">
              <div>Matrícula: {{ i.matriculaImovel }}</div>
              <div class="font-bold">{{ i.valorOperacao | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</div>
            </td>
          </ng-container>

          <ng-container matColumnDef="statusDoi">
            <th mat-header-cell *matHeaderCellDef>Status DOI</th>
            <td mat-cell *matCellDef="let i">
              <span class="status-badge" [ngClass]="i.doiGerada ? 'sucesso' : 'alerta'">
                {{ i.doiGerada ? 'XML Gerado' : 'Pendente de Exportação' }}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="exportar">
            <th mat-header-cell *matHeaderCellDef class="text-right">Exportar XML DOI</th>
            <td mat-cell *matCellDef="let i" class="text-right">
              <a mat-flat-button color="primary" [href]="api.urlExportarDoiXml(i.idEscritura)" target="_blank">
                <mat-icon>download</mat-icon>
                Gerar XML DOI
              </a>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="['protocolo', 'partes', 'valor', 'statusDoi', 'exportar']"></tr>
          <tr mat-row *matRowDef="let row; columns: ['protocolo', 'partes', 'valor', 'statusDoi', 'exportar'];"></tr>
        </table>
      </div>
    </mat-card>
  `
})
export class EnvioDoiComponent implements OnInit {
  readonly api = inject(SaecApiService);
  readonly itens = signal<IndiceEscrituraDto[]>([]);
  termo = '';

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.api.consultarIndiceLivros(this.termo).subscribe(r => this.itens.set(r.itens));
  }
}

/**
 * UC-11: Gerar Certidões de Escrituras Lavradas em PDF
 */
@Component({
  selector: 'app-certidoes',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>workspace_premium</mat-icon> Emitir Certidões de Escrituras em PDF (UC-11)</h1>
        <p>Emissão imediata de certidão/traslado em PDF a partir do número de protocolo ou pesquisa no acervo digitalizado.</p>
      </div>
    </div>

    <mat-card class="p-6 mb-4">
      <h3 class="text-primary" style="margin-top:0">Emissão Direta por Número de Protocolo (Caso de Teste CT-11: 4119914266)</h3>
      <div class="flex flex-wrap gap-3 items-center">
        <mat-form-field appearance="outline" class="flex-1">
          <mat-label>Número do Protocolo (10 caracteres)</mat-label>
          <input matInput [(ngModel)]="protocoloDireto" maxlength="10" placeholder="4119914266" />
        </mat-form-field>
        <a mat-flat-button color="primary" [href]="api.urlCertidaoPorProtocoloPdf(protocoloDireto)" target="_blank" style="height:54px">
          <mat-icon>picture_as_pdf</mat-icon>
          Emitir Certidão em PDF
        </a>
      </div>
    </mat-card>

    <mat-card class="p-4">
      <h3 class="text-primary" style="margin-top:0">Acervo de Escrituras Disponíveis para Emissão de Certidão</h3>
      <div class="table-container">
        <table mat-table [dataSource]="itens()">
          <ng-container matColumnDef="protocolo">
            <th mat-header-cell *matHeaderCellDef>Protocolo / Livro</th>
            <td mat-cell *matCellDef="let i">
              <strong>{{ i.numeroProtocolo }}</strong> — Livro {{ i.numeroLivro }} (Fls. {{ i.folhaInicial }}-{{ i.folhaFinal }})
            </td>
          </ng-container>
          <ng-container matColumnDef="partes">
            <th mat-header-cell *matHeaderCellDef>Outorgante / Outorgado</th>
            <td mat-cell *matCellDef="let i">{{ i.nomeOutorgante }} → {{ i.nomeOutorgado }}</td>
          </ng-container>
          <ng-container matColumnDef="emitir">
            <th mat-header-cell *matHeaderCellDef class="text-right">Certidão PDF</th>
            <td mat-cell *matCellDef="let i" class="text-right">
              <a mat-stroked-button color="primary" [href]="api.urlCertidaoEscrituraPdf(i.idEscritura)" target="_blank">
                <mat-icon>picture_as_pdf</mat-icon>
                Baixar Certidão PDF
              </a>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="['protocolo', 'partes', 'emitir']"></tr>
          <tr mat-row *matRowDef="let row; columns: ['protocolo', 'partes', 'emitir'];"></tr>
        </table>
      </div>
    </mat-card>
  `
})
export class CertidoesComponent implements OnInit {
  readonly api = inject(SaecApiService);
  readonly itens = signal<IndiceEscrituraDto[]>([]);
  protocoloDireto = '4119914266';

  ngOnInit(): void {
    this.api.consultarIndiceLivros('').subscribe(r => this.itens.set(r.itens));
  }
}
