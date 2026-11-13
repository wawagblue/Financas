import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { StatusServico } from './api.models';

@Injectable({ providedIn: 'root' })
export class ServicosService {
  private readonly http = inject(HttpClient);

  status(): Observable<StatusServico[]> {
    return this.http.get<StatusServico[]>('/api/servicos/status');
  }
}
