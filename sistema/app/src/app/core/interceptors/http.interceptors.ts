import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { TokenService, UsuarioService } from '../services/auth-core.services';

/**
 * RequestInterceptor: injeta cabeçalhos de auditoria do usuário atual (Mock/Bypass)
 * e prepara o cabeçalho Authorization Bearer para quando o Login JWT for ativado.
 */
export const requestInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const usuarioService = inject(UsuarioService);
  const user = usuarioService.usuarioAtual();
  const token = tokenService.obterToken();

  const cloned = req.clone({
    setHeaders: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      'X-Usuario-Id': String(user.id),
      'X-Usuario-Login': user.login,
      'X-Usuario-Nome': user.nome,
      'X-Usuario-Perfil': user.perfil
    }
  });

  return next(cloned);
};

/**
 * ErrorInterceptor: captura notificações de erro do Notification Pattern do backend (.NET 10)
 * e exibe feedback padronizado via MatSnackBar.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const errosBackend: string[] | undefined = error?.error?.erros;
      if (errosBackend && errosBackend.length > 0) {
        snackBar.open(errosBackend.join(' | '), 'Fechar', {
          duration: 6000,
          panelClass: ['snackbar-erro']
        });
      }
      return throwError(() => error);
    })
  );
};
