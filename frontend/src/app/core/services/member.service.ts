import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Member } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class MemberService {
  private apiUrl = `${environment.apiUrl}/members`;

  constructor(private http: HttpClient) {}

  getMembers(search?: string, status?: string): Observable<Member[]> {
    let params = new HttpParams();

    if (search) {
      params = params.set('search', search);
    }

    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<Member[]>(this.apiUrl, { params });
  }

  getMemberById(id: number): Observable<Member> {
    return this.http.get<Member>(`${this.apiUrl}/${id}`);
  }

  lockMember(id: number): Observable<Member> {
    return this.http.put<Member>(`${this.apiUrl}/${id}/lock`, {});
  }

  unlockMember(id: number): Observable<Member> {
    return this.http.put<Member>(`${this.apiUrl}/${id}/unlock`, {});
  }

  updateNotes(id: number, notes: string | null): Observable<Member> {
    return this.http.put<Member>(`${this.apiUrl}/${id}/notes`, { notes });
  }
}
