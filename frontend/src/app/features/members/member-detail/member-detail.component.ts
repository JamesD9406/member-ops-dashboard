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
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { MemberService } from '../../../core/services/member.service';
import {
  Member,
  AccountFlag,
  ServiceRequest,
} from '../../../core/models/member.models';
import { AccountFlagService } from '../../../core/services/account-flag.service';
import {
  FlagDialogComponent,
  FlagDialogResult,
} from '../../flags/flag-dialog/flag-dialogue.component';

@Component({
  selector: 'app-member-detail',
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
  ],
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.scss'],
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  member: Member | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private memberService: MemberService,
    private flagService: AccountFlagService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (id) {
      this.loadMember(+id);
    } else {
      this.errorMessage = 'Invalid member ID';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadMember(id: number): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.memberService
      .getMemberById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (member) => {
          this.member = member;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading member:', error);
          this.errorMessage =
            error.status === 404
              ? 'Member not found'
              : 'Failed to load member details';
          this.isLoading = false;
        },
      });
  }

  goBack(): void {
    this.router.navigate(['/members']);
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Active':
        return 'primary';
      case 'Locked':
        return 'warn';
      case 'Closed':
        return 'accent';
      default:
        return '';
    }
  }

  getFlagColor(flagType: string): string {
    switch (flagType) {
      case 'FraudReview':
        return 'warn';
      case 'IDVerification':
        return 'accent';
      case 'PaymentIssue':
        return 'warn';
      case 'GeneralReview':
        return 'primary';
      default:
        return '';
    }
  }

  getRequestStatusColor(status: string): string {
    switch (status) {
      case 'Open':
        return 'accent';
      case 'InProgress':
        return 'primary';
      case 'Completed':
        return '';
      default:
        return '';
    }
  }

  getActiveFlags(): AccountFlag[] {
    return this.member?.flags?.filter((f) => !f.resolvedAt) || [];
  }

  getResolvedFlags(): AccountFlag[] {
    return this.member?.flags?.filter((f) => f.resolvedAt) || [];
  }

  getOpenRequests(): ServiceRequest[] {
    return (
      this.member?.serviceRequests?.filter((r) => r.status !== 'Completed') ||
      []
    );
  }

  getCompletedRequests(): ServiceRequest[] {
    return (
      this.member?.serviceRequests?.filter((r) => r.status === 'Completed') ||
      []
    );
  }

  openAddFlagDialog(): void {
    if (!this.member) return;

    const dialogRef = this.dialog.open(FlagDialogComponent, {
      width: '500px',
      data: {
        memberId: this.member.id,
        memberName: `${this.member.firstName} ${this.member.lastName}`,
      },
    });

    dialogRef
      .afterClosed()
      .subscribe((result: FlagDialogResult | undefined) => {
        if (result && this.member) {
          this.createFlag(result);
        }
      });
  }

  createFlag(flagData: FlagDialogResult): void {
    if (!this.member) return;

    this.flagService
      .createFlag(this.member.id, flagData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('Flag added successfully', 'Close', {
            duration: 3000,
          });
          this.loadMember(this.member!.id);
        },
        error: (error) => {
          console.error('Error creating flag:', error);
          this.snackBar.open('Failed to add flag', 'Close', {
            duration: 3000,
          });
        },
      });
  }

  resolveFlag(flag: AccountFlag): void {
    if (
      !this.member ||
      !confirm('Are you sure you want to resolve this flag?')
    ) {
      return;
    }

    this.flagService
      .resolveFlag(this.member.id, flag.id, {
        resolutionNotes: 'Resolved by staff member',
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('Flag resolved successfully', 'Close', {
            duration: 3000,
          });
          this.loadMember(this.member!.id);
        },
        error: (error) => {
          console.error('Error resolving flag:', error);
          this.snackBar.open('Failed to resolve flag', 'Close', {
            duration: 3000,
          });
        },
      });
  }
}
