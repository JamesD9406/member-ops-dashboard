import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Member } from '../models'; // Changed from '../models/member.models'
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class MemberService {
  private apiUrl = `${environment.apiUrl}/members`;

  constructor(private http: HttpClient) {}

  /**
   * Get all members with optional search and status filtering
   * @param search - Search term for member number, name, email, or phone
   * @param status - Filter by status: 'Active', 'Locked', or 'Closed'
   */
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

  /**
   * Get a single member by ID with related data (flags, service requests)
   * @param id - Member ID
   */
  getMemberById(id: number): Observable<Member> {
    return this.http.get<Member>(`${this.apiUrl}/${id}`);
  }
}
