import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-core.services';

/**
 * Guard de rota da área restrita do CartorioTcc.
 * Ressalva de Arquitetura: A estrutura de guards e validação de perfis está criada e roteada,
 * porém em modo bypass temporário (autenticação inativa por enquanto, alinhada ao backend .NET 10).
 */
export const authGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.autenticacaoBypassAtivo()) {
    return true;
  }

  const perfisRota = route.data?.['perfis'] as string[] | undefined;
  if (authService.estaAutenticado() && authService.possuiPermissaoPerfil(perfisRota)) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
