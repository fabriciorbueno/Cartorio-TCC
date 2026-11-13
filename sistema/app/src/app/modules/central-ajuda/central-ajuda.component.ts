import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-central-ajuda',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatExpansionModule,
    MatIconModule,
    MatButtonModule
  ],
  template: `
    <div class="page-header">
      <div>
        <h1><mat-icon>help_center</mat-icon> Central de Ajuda, Fluxos Operacionais e Documentação do TCC (2015 → 2026)</h1>
        <p>SAEC — Sistema Automatizador de Escrituração para Cartórios | Estudo de Caso: 2º Tabelionato de Notas de Campinas/SP</p>
      </div>
    </div>

    <div class="form-grid-2 mb-4">
      <mat-card class="p-6">
        <h3 class="text-primary" style="margin-top:0">Fluxo ANTES do SAEC (Processo Manual em Word/Excel)</h3>
        <ul class="flow-list">
          <li>Redação manual de escrituras em arquivos avulsos do Microsoft Word sem padronização;</li>
          <li>Cálculo manual de emolumentos consultando tabelas físicas do Colégio Notarial do Brasil (CNB/SP);</li>
          <li>Digitação manual do Índice do Livro ao final de cada livro físico de 200 folhas;</li>
          <li>Redigitação manual dos dados da escritura no programa DOI da Receita Federal em até 7 dias;</li>
          <li>Clientes precisavam telefonar ao cartório para saber o andamento do processo.</li>
        </ul>
      </mat-card>

      <mat-card class="p-6">
        <h3 class="text-primary" style="margin-top:0">Fluxo DEPOIS do SAEC (.NET 10 + Angular 21)</h3>
        <ul class="flow-list">
          <li>Simulação instantânea de Orçamentos pela Tabela CNB/SP com descontos legais (UC-06);</li>
          <li>Geração automática de Protocolo de 10 dígitos na recepção e acompanhamento Web/Mobile (UC-07 / UC-12);</li>
          <li>Cadastro único de Clientes, Cônjuges, Certidões e Documentos Digitalizados em PDF (UC-08);</li>
          <li>Geração automática da Minuta, Índice de Livros, Certidão PDF (UC-11) e XML DOI (UC-10);</li>
          <li>Backups Criptografados AES-256 + SHA-256 agendados e manuais (UC-03 / UC-05).</li>
        </ul>
      </mat-card>
    </div>

    <mat-card class="p-6">
      <h3 class="text-primary" style="margin-top:0">Perguntas Frequentes e Regras de Negócio dos 12 Casos de Uso</h3>
      <mat-accordion>
        <mat-expansion-panel>
          <mat-expansion-panel-header>
            <mat-panel-title>Como funciona a consulta pública de andamento pelo cliente (UC-12)?</mat-panel-title>
          </mat-expansion-panel-header>
          <p>
            Ao entregar a documentação na recepção (UC-07), o SAEC gera um protocolo alfanumérico de 10 caracteres (ex.: <strong>4119914266</strong>).
            O cliente acessa a tela pública/mobile de Consulta de Andamento e visualiza em qual das 4 fases sua escritura se encontra:
            1) Aguardando documentação, 2) Em processo de escrituração, 3) Em processo de impressão, ou 4) Finalizado – aguardando retirada.
          </p>
        </mat-expansion-panel>

        <mat-expansion-panel>
          <mat-expansion-panel-header>
            <mat-panel-title>Por que escrituras não podem ser excluídas fisicamente (UC-09)?</mat-panel-title>
          </mat-expansion-panel-header>
          <p>
            Por força da segurança jurídica notarial e exigência de auditoria definida no TCC, uma escritura nunca sofre exclusão física.
            Caso haja desistência ou erro material insanável antes da assinatura, ela é <strong>Inativada</strong> mediante preenchimento obrigatório da justificativa, que fica gravada no Histórico de Auditoria.
          </p>
        </mat-expansion-panel>

        <mat-expansion-panel>
          <mat-expansion-panel-header>
            <mat-panel-title>Como é gerado o arquivo XML para a Declaração sobre Operações Imobiliárias - DOI (UC-10)?</mat-panel-title>
          </mat-expansion-panel-header>
          <p>
            Toda escritura lavrada com transferência imobiliária deve ser declarada à Receita Federal em até 7 dias.
            No módulo UC-10 (ou diretamente na listagem de escrituras), o botão "Exportar XML DOI" gera o arquivo padronizado com a nomenclatura oficial <code>yyyy-MM-dd-numeroProtocolo.xml</code>.
          </p>
        </mat-expansion-panel>
      </mat-accordion>
    </mat-card>
  `,
  styles: [`
    .flow-list {
      margin: 0;
      padding-left: 1.2rem;
      line-height: 1.75;
    }
  `]
})
export class CentralAjudaComponent {}
