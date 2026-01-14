import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';

import { AuditLogService } from '../../core/services/audit-log.service';
import { AuditLog } from '../../core/models/member.models';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
  ],
  templateUrl: './audit-log.component.html',
  styleUrls: ['./audit-log.component.scss'],
})
export class AuditLogComponent implements OnInit, OnDestroy {
  displayedColumns: string[] = [
    'timestamp',
    'actor',
    'action',
    'member',
    'details',
  ];
  dataSource: AuditLog[] = [];
  loading = false;
  error: string | null = null;

  filterForm: FormGroup;
  actionTypes = [
    'Flag Created',
    'Flag Resolved',
    'Account Locked',
    'Account Unlocked',
    'Service Request Created',
    'Service Request Completed',
    'Notes Updated',
    'Status Changed'
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private auditLogService: AuditLogService,
    private fb: FormBuilder,
  ) {
    this.filterForm = this.fb.group({
      startDate: [null],
      endDate: [null],
      action: [''],
      actor: [''],
    });

    // Add validation: endDate must be >= startDate
    this.filterForm.get('startDate')?.valueChanges.subscribe(() => {
      this.filterForm.get('endDate')?.updateValueAndValidity({ emitEvent: false });
    });
  }

  // Validation function for end date
  endDateFilter = (date: Date | null): boolean => {
    if (!date) {
      return true;
    }

    const today = new Date();
    today.setHours(23, 59, 59, 999); // End of today

    const startDate = this.filterForm.get('startDate')?.value;

    // Date must be <= today AND >= startDate (if startDate is selected)
    if (startDate) {
      return date >= startDate && date <= today;
    }

    return date <= today;
  };

  // Validation function for start date
  startDateFilter = (date: Date | null): boolean => {
    if (!date) {
      return true;
    }

    const today = new Date();
    today.setHours(23, 59, 59, 999); // End of today

    const endDate = this.filterForm.get('endDate')?.value;

    // Date must be <= today AND <= endDate (if endDate is selected)
    if (endDate) {
      return date <= endDate && date <= today;
    }

    return date <= today;
  };

  ngOnInit(): void {
    this.loadAuditLogs();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadAuditLogs(): void {
    this.loading = true;
    this.error = null;

    const filters = this.buildFilters();

    this.auditLogService
      .getAuditLogs(filters)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (logs) => {
          this.dataSource = logs;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading audit logs:', err);
          this.error = 'Failed to load audit logs. Please try again.';
          this.loading = false;
        },
      });
  }

  private buildFilters() {
    const formValue = this.filterForm.value;
    const filters: any = {};

    if (formValue.startDate) {
      filters.startDate = formValue.startDate.toISOString().split('T')[0];
    }
    if (formValue.endDate) {
      filters.endDate = formValue.endDate.toISOString().split('T')[0];
    }
    if (formValue.action) {
      filters.action = formValue.action;
    }
    if (formValue.actor?.trim()) {
      filters.actor = formValue.actor.trim();
    }

    return filters;
  }

  applyFilters(): void {
    this.loadAuditLogs();
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.loadAuditLogs();
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString();
  }

  getMemberDisplay(log: AuditLog): string {
    if (log.member) {
      return `${log.member.firstName} ${log.member.lastName} (${log.member.memberNumber})`;
    }
    return `Member ID: ${log.memberId}`;
  }
}
