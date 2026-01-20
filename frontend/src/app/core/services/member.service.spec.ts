import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { MemberService } from './member.service';
import { Member } from '../models';
import { environment } from '../../../environments/environment';

describe('MemberService', () => {
  let service: MemberService;
  let httpMock: HttpTestingController;

  const apiUrl = `${environment.apiUrl}/members`;

  const mockMembers: Member[] = [
    {
      id: 1,
      memberNumber: 'M-100001',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com',
      phone: '555-0101',
      status: 'Active',
      joinDate: '2020-01-15',
    },
    {
      id: 2,
      memberNumber: 'M-100002',
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@example.com',
      phone: '555-0102',
      status: 'Locked',
      joinDate: '2019-06-20',
    },
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        MemberService,
      ],
    });

    service = TestBed.inject(MemberService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('getMembers', () => {
    it('should fetch all members without params', () => {
      service.getMembers().subscribe((members) => {
        expect(members.length).toBe(2);
        expect(members).toEqual(mockMembers);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockMembers);
    });

    it('should include search param when provided', () => {
      service.getMembers('John').subscribe();

      const req = httpMock.expectOne(`${apiUrl}?search=John`);
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('search')).toBe('John');
      req.flush(mockMembers);
    });

    it('should include status param when provided', () => {
      service.getMembers(undefined, 'Active').subscribe();

      const req = httpMock.expectOne(`${apiUrl}?status=Active`);
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('status')).toBe('Active');
      req.flush(mockMembers);
    });

    it('should include both search and status params when provided', () => {
      service.getMembers('John', 'Active').subscribe();

      const req = httpMock.expectOne(`${apiUrl}?search=John&status=Active`);
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('search')).toBe('John');
      expect(req.request.params.get('status')).toBe('Active');
      req.flush(mockMembers);
    });

    it('should return empty array when no members found', () => {
      service.getMembers('NonExistent').subscribe((members) => {
        expect(members.length).toBe(0);
        expect(members).toEqual([]);
      });

      const req = httpMock.expectOne(`${apiUrl}?search=NonExistent`);
      req.flush([]);
    });
  });

  describe('getMemberById', () => {
    it('should fetch a single member by id', () => {
      service.getMemberById(1).subscribe((member) => {
        expect(member).toEqual(mockMembers[0]);
        expect(member.id).toBe(1);
      });

      const req = httpMock.expectOne(`${apiUrl}/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockMembers[0]);
    });

    it('should return member with flags and service requests', () => {
      const memberWithDetails: Member = {
        ...mockMembers[0],
        flags: [
          {
            id: 1,
            memberId: 1,
            flagType: 'FraudReview',
            description: 'Test',
            createdBy: 'admin',
            createdAt: '2026-01-01',
          },
        ],
        serviceRequests: [],
      };

      service.getMemberById(1).subscribe((member) => {
        expect(member.flags?.length).toBe(1);
      });

      const req = httpMock.expectOne(`${apiUrl}/1`);
      req.flush(memberWithDetails);
    });

    it('should handle 404 error for non-existent member', () => {
      service.getMemberById(999).subscribe({
        next: () => fail('should have failed with 404'),
        error: (error) => {
          expect(error.status).toBe(404);
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/999`);
      req.flush('Member not found', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('lockMember', () => {
    it('should send PUT request to lock endpoint', () => {
      const lockedMember: Member = { ...mockMembers[0], status: 'Locked' };

      service.lockMember(1).subscribe((member) => {
        expect(member.status).toBe('Locked');
      });

      const req = httpMock.expectOne(`${apiUrl}/1/lock`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({});
      req.flush(lockedMember);
    });

    it('should handle unauthorized error (403) for non-supervisor', () => {
      service.lockMember(1).subscribe({
        next: () => fail('should have failed with 403'),
        error: (error) => {
          expect(error.status).toBe(403);
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/1/lock`);
      req.flush('Forbidden', { status: 403, statusText: 'Forbidden' });
    });
  });

  describe('unlockMember', () => {
    it('should send PUT request to unlock endpoint', () => {
      const unlockedMember: Member = { ...mockMembers[0], status: 'Active' };

      service.unlockMember(1).subscribe((member) => {
        expect(member.status).toBe('Active');
      });

      const req = httpMock.expectOne(`${apiUrl}/1/unlock`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({});
      req.flush(unlockedMember);
    });

    it('should handle unauthorized error (403) for non-supervisor', () => {
      service.unlockMember(1).subscribe({
        next: () => fail('should have failed with 403'),
        error: (error) => {
          expect(error.status).toBe(403);
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/1/unlock`);
      req.flush('Forbidden', { status: 403, statusText: 'Forbidden' });
    });
  });

  describe('updateNotes', () => {
    it('should send PUT request with notes payload', () => {
      const updatedMember: Member = {
        ...mockMembers[0],
        notes: 'Updated notes',
      };

      service.updateNotes(1, 'Updated notes').subscribe((member) => {
        expect(member.notes).toBe('Updated notes');
      });

      const req = httpMock.expectOne(`${apiUrl}/1/notes`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({ notes: 'Updated notes' });
      req.flush(updatedMember);
    });

    it('should handle null notes (clearing notes)', () => {
      const updatedMember: Member = { ...mockMembers[0], notes: undefined };

      service.updateNotes(1, null).subscribe((member) => {
        expect(member.notes).toBeUndefined();
      });

      const req = httpMock.expectOne(`${apiUrl}/1/notes`);
      expect(req.request.body).toEqual({ notes: null });
      req.flush(updatedMember);
    });

    it('should handle unauthorized error (403) for non-supervisor', () => {
      service.updateNotes(1, 'New notes').subscribe({
        next: () => fail('should have failed with 403'),
        error: (error) => {
          expect(error.status).toBe(403);
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/1/notes`);
      req.flush('Forbidden', { status: 403, statusText: 'Forbidden' });
    });
  });
});
