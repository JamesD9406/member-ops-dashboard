import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';

// Material imports
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { MemberService } from '../../../core/services/member.service';
import { Member } from '../../../core/models/member.models';

@Component({
  selector: 'app-member-search',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './member-search.component.html',
  styleUrls: ['./member-search.component.scss'],
})
export class MemberSearchComponent implements OnInit, OnDestroy {
  searchControl = new FormControl('');

  members: Member[] = [];
  displayedColumns: string[] = [
    'memberNumber',
    'name',
    'email',
    'phone',
    'status',
    'flags',
    'joinDate',
  ];

  selectedStatus: string | null = null;

  isLoading = false;

  // Unsubscribe subject
  private destroy$ = new Subject<void>();

  constructor(
    private memberService: MemberService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadMembers();

    // Set up debounced search
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((searchTerm) => {
        this.loadMembers(searchTerm || undefined);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadMembers(search?: string): void {
    this.isLoading = true;

    this.memberService
      .getMembers(search, this.selectedStatus || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (members) => {
          this.members = members;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading members:', error);
          this.isLoading = false;
        },
      });
  }

  filterByStatus(status: string | null): void {
    this.selectedStatus = status;
    this.loadMembers(this.searchControl.value || undefined);
  }

  viewMember(member: Member): void {
    this.router.navigate(['/members', member.id]);
  }

  getActiveFlagCount(member: Member): number {
    return member.flags?.filter((f) => !f.resolvedAt).length || 0;
  }

  clearFilters(): void {
    this.searchControl.setValue('');
    this.selectedStatus = null;
    this.loadMembers();
  }
}
