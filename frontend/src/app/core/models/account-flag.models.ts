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
