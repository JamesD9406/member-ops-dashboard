import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

// Material imports
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog } from '@angular/material/dialog';

import { ServiceRequestService } from '../../../core/services/service-request.service';
import { ServiceRequest } from '../../../core/models';
import { AuthService } from '../../../core/services/auth.service';
import {
  AssignStaffDialogComponent,
  AssignStaffDialogResult,
} from './assign-staff-dialog.component';

@Component({
  selector: 'app-service-request-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatListModule,
    MatMenuModule,
  ],
  templateUrl: './service-request-detail.component.html',
  styleUrls: ['./service-request-detail.component.scss'],
})
export class ServiceRequestDetailComponent implements OnInit, OnDestroy {
  serviceRequest: ServiceRequest | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private serviceRequestService: ServiceRequestService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (id) {
      this.loadServiceRequest(+id);
    } else {
      this.errorMessage = 'Invalid service request ID';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadServiceRequest(id: number): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.serviceRequestService
      .getServiceRequestById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (request) => {
          this.serviceRequest = request;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading service request:', error);
          this.errorMessage =
            error.status === 404
              ? 'Service request not found'
              : 'Failed to load service request details';
          this.isLoading = false;
        },
      });
  }

  goBack(): void {
    this.router.navigate(['/service-requests']);
  }

  viewMember(): void {
    if (this.serviceRequest) {
      this.router.navigate(['/members', this.serviceRequest.memberId]);
    }
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'New':
        return 'accent';
      case 'InProgress':
        return 'primary';
      case 'Resolved':
        return '';
      case 'Cancelled':
        return 'warn';
      default:
        return '';
    }
  }

  getPriorityColor(priority: string): string {
    switch (priority) {
      case 'Urgent':
        return 'warn';
      case 'High':
        return 'warn';
      case 'Medium':
        return 'accent';
      case 'Low':
        return '';
      default:
        return '';
    }
  }

  getMemberName(): string {
    if (this.serviceRequest?.member) {
      return `${this.serviceRequest.member.firstName} ${this.serviceRequest.member.lastName}`;
    }
    return `Member #${this.serviceRequest?.memberId}`;
  }

  canAssign(): boolean {
    return this.authService.hasRole(['Supervisor', 'Admin']);
  }

  canResolve(): boolean {
    return (
      this.serviceRequest?.status !== 'Resolved' &&
      this.serviceRequest?.status !== 'Cancelled'
    );
  }

  updateStatus(status: 'New' | 'InProgress' | 'Cancelled'): void {
    if (!this.serviceRequest) return;

    this.serviceRequestService
      .updateStatus(this.serviceRequest.id, { status })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('Status updated successfully', 'Close', {
            duration: 3000,
          });
          this.loadServiceRequest(this.serviceRequest!.id);
        },
        error: (error) => {
          console.error('Error updating status:', error);
          this.snackBar.open('Failed to update status', 'Close', {
            duration: 3000,
          });
        },
      });
  }

  openAssignDialog(): void {
    if (!this.serviceRequest) return;

    const dialogRef = this.dialog.open(AssignStaffDialogComponent, {
      width: '400px',
      data: {
        serviceRequestId: this.serviceRequest.id,
        currentAssignedToId: this.serviceRequest.assignedToId,
      },
    });

    dialogRef
      .afterClosed()
      .subscribe((result: AssignStaffDialogResult | undefined) => {
        if (result && this.serviceRequest) {
          this.assignToStaff(result.assignedToId);
        }
      });
  }

  private assignToStaff(assignedToId: number): void {
    if (!this.serviceRequest) return;

    this.serviceRequestService
      .assignServiceRequest(this.serviceRequest.id, { assignedToId })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('Service request assigned successfully', 'Close', {
            duration: 3000,
          });
          this.loadServiceRequest(this.serviceRequest!.id);
        },
        error: (error) => {
          console.error('Error assigning service request:', error);
          this.snackBar.open('Failed to assign service request', 'Close', {
            duration: 3000,
          });
        },
      });
  }
}
