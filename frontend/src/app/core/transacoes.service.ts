import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Transacao, TransacaoInput } from './api.models';

const API = '/api/transacoes';

@Injectable({ providedIn: 'root' })
export class TransacoesService {
  private readonly http = inject(HttpClient);

  listar(): Observable<Transacao[]> {
    return this.http.get<Transacao[]>(API);
  }

  criar(transacao: TransacaoInput): Observable<Transacao> {
    return this.http.post<Transacao>(API, transacao);
  }

  atualizar(id: string, transacao: TransacaoInput): Observable<Transacao> {
    return this.http.put<Transacao>(`${API}/${id}`, transacao);
  }

  remover(id: string): Observable<void> {
    return this.http.delete<void>(`${API}/${id}`);
  }
}
