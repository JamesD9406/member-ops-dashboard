import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { FormsModule } from '@angular/forms';

// Material imports
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ServiceRequestService } from '../../../core/services/service-request.service';
import { ServiceRequest } from '../../../core/models';

@Component({
  selector: 'app-service-request-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTooltipModule,
  ],
  templateUrl: './service-request-list.component.html',
  styleUrls: ['./service-request-list.component.scss'],
})
export class ServiceRequestListComponent implements OnInit, OnDestroy {
  dataSource = new MatTableDataSource<ServiceRequest>([]);
  isLoading = false;
  errorMessage: string | null = null;

  statusFilter: string = '';
  priorityFilter: string = '';

  displayedColumns: string[] = [
    'id',
    'member',
    'requestType',
    'priority',
    'status',
    'assignedTo',
    'createdAt',
    'actions',
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private serviceRequestService: ServiceRequestService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadServiceRequests();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadServiceRequests(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.serviceRequestService
      .getServiceRequests(
        this.statusFilter || undefined,
        this.priorityFilter || undefined,
      )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (requests) => {
          this.dataSource.data = requests;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading service requests:', error);
          this.errorMessage = 'Failed to load service requests';
          this.isLoading = false;
        },
      });
  }

  applyFilters(): void {
    this.loadServiceRequests();
  }

  clearFilters(): void {
    this.statusFilter = '';
    this.priorityFilter = '';
    this.loadServiceRequests();
  }

  viewDetails(request: ServiceRequest): void {
    this.router.navigate(['/service-requests', request.id]);
  }

  getMemberName(request: ServiceRequest): string {
    if (request.member) {
      return `${request.member.firstName} ${request.member.lastName}`;
    }
    return `Member #${request.memberId}`;
  }

  getAssignedToName(request: ServiceRequest): string {
    return request.assignedTo?.displayName || 'Unassigned';
  }
}
