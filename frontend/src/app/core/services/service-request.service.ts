import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ServiceRequest, ServiceRequestComment } from '../models'; // Changed from '../models/member.models'
import { environment } from '../../../environments/environment';

export interface CreateServiceRequestRequest {
  memberId: number;
  requestType: string;
  description: string;
  priority: ServiceRequest['priority'];
}

export interface AssignServiceRequestRequest {
  assignedToId: number;
}

export interface UpdateStatusRequest {
  status: ServiceRequest['status'];
}

export interface ResolveServiceRequestRequest {
  resolutionType: NonNullable<ServiceRequest['resolutionType']>;
  resolutionNotes?: string;
}

export interface AddCommentRequest {
  commentText: string;
}

@Injectable({
  providedIn: 'root',
})
export class ServiceRequestService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getServiceRequests(
    status?: string,
    priority?: string,
    assignedToId?: number,
    memberId?: number,
  ): Observable<ServiceRequest[]> {
    let params = new HttpParams();

    if (status) params = params.set('status', status);
    if (priority) params = params.set('priority', priority);
    if (assignedToId)
      params = params.set('assignedToId', assignedToId.toString());
    if (memberId) params = params.set('memberId', memberId.toString());

    return this.http.get<ServiceRequest[]>(`${this.apiUrl}/service-requests`, {
      params,
    });
  }

  getServiceRequestById(id: number): Observable<ServiceRequest> {
    return this.http.get<ServiceRequest>(
      `${this.apiUrl}/service-requests/${id}`,
    );
  }

  createServiceRequest(
    request: CreateServiceRequestRequest,
  ): Observable<ServiceRequest> {
    return this.http.post<ServiceRequest>(
      `${this.apiUrl}/service-requests`,
      request,
    );
  }

  assignServiceRequest(
    id: number,
    request: AssignServiceRequestRequest,
  ): Observable<ServiceRequest> {
    return this.http.put<ServiceRequest>(
      `${this.apiUrl}/service-requests/${id}/assign`,
      request,
    );
  }

  updateStatus(
    id: number,
    request: UpdateStatusRequest,
  ): Observable<ServiceRequest> {
    return this.http.put<ServiceRequest>(
      `${this.apiUrl}/service-requests/${id}/status`,
      request,
    );
  }

  resolveServiceRequest(
    id: number,
    request: ResolveServiceRequestRequest,
  ): Observable<ServiceRequest> {
    return this.http.put<ServiceRequest>(
      `${this.apiUrl}/service-requests/${id}/resolve`,
      request,
    );
  }

  addComment(
    id: number,
    request: AddCommentRequest,
  ): Observable<ServiceRequestComment> {
    return this.http.post<ServiceRequestComment>(
      `${this.apiUrl}/service-requests/${id}/comments`,
      request,
    );
  }
}
