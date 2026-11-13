import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { MetaEconomia, Orcamento, Recorrencia, Relatorio } from './api.models';

@Injectable({ providedIn: 'root' })
export class PlanejamentoService {
  private readonly http = inject(HttpClient);
  orcamentos(mes: string): Observable<Orcamento[]> { return this.http.get<Orcamento[]>(`/api/orcamentos?mes=${mes}`); }
  criarOrcamento(categoria: string, limiteMensal: number): Observable<Orcamento> { return this.http.post<Orcamento>('/api/orcamentos', { categoria, limiteMensal }); }
  removerOrcamento(id: string): Observable<void> { return this.http.delete<void>(`/api/orcamentos/${id}`); }
  recorrencias(): Observable<Recorrencia[]> { return this.http.get<Recorrencia[]>('/api/recorrencias'); }
  criarRecorrencia(dados: { descricao: string; valor: number; categoria: string; proximoVencimento: string }): Observable<Recorrencia> { return this.http.post<Recorrencia>('/api/recorrencias', dados); }
  pagarRecorrencia(id: string): Observable<Recorrencia> { return this.http.post<Recorrencia>(`/api/recorrencias/${id}/pagar`, {}); }
  removerRecorrencia(id: string): Observable<void> { return this.http.delete<void>(`/api/recorrencias/${id}`); }
  metas(): Observable<MetaEconomia[]> { return this.http.get<MetaEconomia[]>('/api/metas'); }
  criarMeta(dados: { nome: string; valorAlvo: number; prazo: string | null }): Observable<MetaEconomia> { return this.http.post<MetaEconomia>('/api/metas', dados); }
  depositar(id: string, valor: number): Observable<MetaEconomia> { return this.http.post<MetaEconomia>(`/api/metas/${id}/depositos`, { valor }); }
  removerMeta(id: string): Observable<void> { return this.http.delete<void>(`/api/metas/${id}`); }
  relatorio(mes: string): Observable<Relatorio> { return this.http.get<Relatorio>(`/api/relatorios?mes=${mes}`); }
}
