export interface Member {
  id: number;
  memberNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  status: 'Active' | 'Locked' | 'Closed';
  joinDate: string;
  notes?: string;
}

export interface AccountFlag {
  id: number;
  memberId: number;
  flagType: 'FraudReview' | 'IDVerification' | 'PaymentIssue' | 'GeneralReview';
  description: string;
  createdBy: string;
  createdAt: string;
  resolvedBy?: string;
  resolvedAt?: string;
  resolutionNotes?: string;
}

export interface ServiceRequest {
  id: number;
  memberId: number;
  requestType: 'CardReplacement' | 'StatementRequest' | 'AddressChange' | 'Question';
  description: string;
  status: 'Open' | 'InProgress' | 'Completed';
  createdBy: string;
  createdAt: string;
  completedBy?: string;
  completedAt?: string;
}

export interface AuditLog {
  id: number;
  memberId: number;
  actor: string;
  action: string;
  details?: string;
  timestamp: string;
}
