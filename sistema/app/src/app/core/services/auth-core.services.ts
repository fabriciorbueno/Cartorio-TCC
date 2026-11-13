import { Injectable, signal, computed } from '@angular/core';

export interface UsuarioSessao {
  id: number;
  login: string;
  nome: string;
  perfil: 'Administrador' | 'Tabelião' | 'Escrevente' | 'Auxiliar de Cartório';
  cargo: string;
}

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly STORAGE_KEY = 'saec_jwt_token_placeholder';

  obterToken(): string | null {
    return localStorage.getItem(this.STORAGE_KEY) ?? 'bypass-token-saec-temporario';
  }

  salvarToken(token: string): void {
    localStorage.setItem(this.STORAGE_KEY, token);
  }

  limparToken(): void {
    localStorage.removeItem(this.STORAGE_KEY);
  }
}

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  readonly usuariosDisponiveis: UsuarioSessao[] = [
    { id: 1, login: 'admin@saec.com', nome: 'Administrador SAEC', perfil: 'Administrador', cargo: 'Administrador de Sistemas' },
    { id: 4, login: 'wemeson@saec.com', nome: 'Wemeson Carvalho', perfil: 'Tabelião', cargo: 'Tabelião Titular' },
    { id: 2, login: 'daniel.granado@saec.com', nome: 'Daniel Luiz Granado', perfil: 'Escrevente', cargo: 'Escrevente Autorizado' },
    { id: 3, login: 'vanderlei@saec.com', nome: 'Vanderlei Minussi', perfil: 'Auxiliar de Cartório', cargo: 'Auxiliar de Cartório' }
  ];

  readonly usuarioAtual = signal<UsuarioSessao>(this.usuariosDisponiveis[0]);

  alternarUsuarioSimulado(id: number): void {
    const encontrado = this.usuariosDisponiveis.find(u => u.id === id);
    if (encontrado) {
      this.usuarioAtual.set(encontrado);
    }
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  /**
   * Ressalva de Arquitetura:
   * A estrutura de autenticação está criada, injetada e roteada, porém inativa por enquanto (bypass).
   */
  readonly autenticacaoBypassAtivo = signal<boolean>(true);
  readonly estaAutenticado = computed(() => this.autenticacaoBypassAtivo() || !!this.tokenService.obterToken());

  constructor(
    private readonly tokenService: TokenService,
    private readonly usuarioService: UsuarioService
  ) {}

  possuiPermissaoPerfil(perfisPermitidos?: string[]): boolean {
    if (this.autenticacaoBypassAtivo() || !perfisPermitidos || perfisPermitidos.length === 0) {
      return true;
    }
    return perfisPermitidos.includes(this.usuarioService.usuarioAtual().perfil);
  }
}
