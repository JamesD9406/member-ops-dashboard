import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccountFlag } from '../models/member.models';
import { environment } from '../../../environments/environment';

export interface CreateFlagRequest {
  flagType: 'FraudReview' | 'IDVerification' | 'PaymentIssue' | 'GeneralReview';
  description: string;
}

export interface ResolveFlagRequest {
  resolutionNotes?: string;
}

@Injectable({
  providedIn: 'root',
})
export class AccountFlagService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getMemberFlags(memberId: number): Observable<AccountFlag[]> {
    return this.http.get<AccountFlag[]>(
      `${this.apiUrl}/members/${memberId}/flags`,
    );
  }

  createFlag(
    memberId: number,
    request: CreateFlagRequest,
  ): Observable<AccountFlag> {
    return this.http.post<AccountFlag>(
      `${this.apiUrl}/members/${memberId}/flags`,
      request,
    );
  }

  resolveFlag(
    memberId: number,
    flagId: number,
    request: ResolveFlagRequest,
  ): Observable<AccountFlag> {
    return this.http.put<AccountFlag>(
      `${this.apiUrl}/members/${memberId}/flags/${flagId}/resolve`,
      request,
    );
  }
}
