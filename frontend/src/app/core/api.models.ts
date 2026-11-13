export type TipoTransacao = 'Receita' | 'Despesa';

export interface Transacao {
  id: string;
  descricao: string;
  valor: number;
  tipo: TipoTransacao;
  categoria: string | null;
  data: string;
  criadoEm: string;
  atualizadoEm: string | null;
}

export interface TransacaoInput {
  descricao: string;
  valor: number;
  tipo: TipoTransacao;
  categoria: string | null;
  data: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
}

export interface Usuario {
  id: string;
  nome: string;
  email: string;
  criadoEmUtc: string;
  atualizadoEmUtc: string;
}

export interface StatusServico {
  nome: string;
  disponivel: boolean;
  detalhe: string | null;
}

export interface Orcamento { id: string; categoria: string; limiteMensal: number; gastoNoMes: number; percentualUsado: number; }
export interface Recorrencia { id: string; descricao: string; valor: number; categoria: string; proximoVencimento: string; }
export interface MetaEconomia { id: string; nome: string; valorAlvo: number; valorGuardado: number; prazo: string | null; }
export interface CategoriaRelatorio { categoria: string; receitas: number; despesas: number; }
export interface MesRelatorio { mes: string; receitas: number; despesas: number; }
export interface Relatorio { mes: string; receitas: number; despesas: number; saldo: number; categorias: CategoriaRelatorio[]; historico: MesRelatorio[]; }
