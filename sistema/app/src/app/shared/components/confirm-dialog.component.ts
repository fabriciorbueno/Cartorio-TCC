import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  titulo: string;
  mensagem: string;
  exigirMotivo?: boolean;
  labelMotivo?: string;
  textoConfirmar?: string;
  corConfirmar?: 'primary' | 'warn';
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title class="flex items-center gap-2">
      <mat-icon [color]="data.corConfirmar || 'primary'">help_outline</mat-icon>
      {{ data.titulo }}
    </h2>
    <mat-dialog-content>
      <p class="mb-4">{{ data.mensagem }}</p>
      @if (data.exigirMotivo) {
        <mat-form-field appearance="outline" class="w-full">
          <mat-label>{{ data.labelMotivo || 'Informe o motivo / justificativa (obrigatório)' }}</mat-label>
          <textarea matInput rows="3" [(ngModel)]="motivo" required placeholder="Descreva o motivo para registro em auditoria..."></textarea>
        </mat-form-field>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button (click)="fechar(false)">Cancelar</button>
      <button
        mat-flat-button
        [color]="data.corConfirmar || 'primary'"
        [disabled]="data.exigirMotivo && motivo.trim().length < 5"
        (click)="fechar(true)">
        {{ data.textoConfirmar || 'Confirmar' }}
      </button>
    </mat-dialog-actions>
  `
})
export class ConfirmDialogComponent {
  readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
  motivo = '';

  fechar(confirmado: boolean): void {
    if (!confirmado) {
      this.dialogRef.close(null);
      return;
    }
    this.dialogRef.close({ confirmado: true, motivo: this.motivo.trim() });
  }
}
