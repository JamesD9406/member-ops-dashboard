import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLog } from '../models/member.models';

@Injectable({
  providedIn: 'root',
})
export class AuditLogService {
  private apiUrl = `${environment.apiUrl}/audit-log`;

  constructor(private http: HttpClient) {}

  getAuditLogs(filters?: {
    startDate?: string;
    endDate?: string;
    memberId?: number;
    action?: string;
    actor?: string;
  }): Observable<AuditLog[]> {
    let params = new HttpParams();

    if (filters?.startDate) {
      params = params.set('startDate', filters.startDate);
    }
    if (filters?.endDate) {
      params = params.set('endDate', filters.endDate);
    }
    if (filters?.memberId) {
      params = params.set('memberId', filters.memberId.toString());
    }
    if (filters?.action) {
      params = params.set('action', filters.action);
    }
    if (filters?.actor) {
      params = params.set('actor', filters.actor);
    }

    return this.http.get<AuditLog[]>(this.apiUrl, { params });
  }
}
