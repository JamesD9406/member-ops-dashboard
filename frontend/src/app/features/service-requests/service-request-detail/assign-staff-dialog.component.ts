import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  MatDialogModule,
  MatDialogRef,
  MAT_DIALOG_DATA,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Staff } from '../../../core/models';
import { StaffService } from '../../../core/services/staff.service';

export interface AssignStaffDialogData {
  serviceRequestId: number;
  currentAssignedToId?: number;
}

export interface AssignStaffDialogResult {
  assignedToId: number;
}

@Component({
  selector: 'app-assign-staff-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './assign-staff-dialog.component.html',
  styleUrls: ['./assign-staff-dialog.component.scss'],
})
export class AssignStaffDialogComponent implements OnInit {
  staffList: Staff[] = [];
  selectedStaffId: number | null = null;
  loading = true;
  error: string | null = null;

  constructor(
    private staffService: StaffService,
    public dialogRef: MatDialogRef<AssignStaffDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AssignStaffDialogData,
  ) {
    if (data.currentAssignedToId) {
      this.selectedStaffId = data.currentAssignedToId;
    }
  }

  ngOnInit(): void {
    this.loadStaff();
  }

  loadStaff(): void {
    this.loading = true;
    this.error = null;

    this.staffService.getStaff().subscribe({
      next: (staff) => {
        this.staffList = staff;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load staff list';
        this.loading = false;
        console.error('Error loading staff:', err);
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onAssign(): void {
    if (this.selectedStaffId) {
      this.dialogRef.close({ assignedToId: this.selectedStaffId });
    }
  }
}
