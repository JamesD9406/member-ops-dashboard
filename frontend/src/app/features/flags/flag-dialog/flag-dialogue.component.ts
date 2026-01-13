import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
  MatDialogModule,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';

export interface FlagDialogData {
  memberId: number;
  memberName: string;
}

export interface FlagDialogResult {
  flagType: 'FraudReview' | 'IDVerification' | 'PaymentIssue' | 'GeneralReview';
  description: string;
}

@Component({
  selector: 'app-flag-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './flag-dialog.component.html',
  styleUrls: ['./flag-dialog.component.scss'],
})
export class FlagDialogComponent {
  flagForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<FlagDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: FlagDialogData,
  ) {
    this.flagForm = this.fb.group({
      flagType: ['', Validators.required],
      description: ['', [Validators.required, Validators.minLength(10)]],
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.flagForm.valid) {
      const result: FlagDialogResult = this.flagForm.value;
      this.dialogRef.close(result);
    }
  }
}
