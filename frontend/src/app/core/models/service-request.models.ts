import { Staff } from './staff.models';

export interface ServiceRequest {
  id: number;
  memberId: number;
  member?: {
    id: number;
    memberNumber: string;
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    status: 'Active' | 'Locked' | 'Closed';
    joinDate: string;
    notes?: string;
  };
  requestType: string;
  description: string;
  status: 'New' | 'InProgress' | 'Resolved' | 'Cancelled';
  priority: 'Low' | 'Medium' | 'High' | 'Urgent';
  createdById: number;
  createdBy?: Staff;
  assignedToId?: number;
  assignedTo?: Staff;
  resolutionType?:
    | 'Resolved'
    | 'MoreInfoNeeded'
    | 'Transferred'
    | 'Duplicate'
    | 'CannotResolve'
    | 'Cancelled';
  resolutionNotes?: string;
  resolvedAt?: string;
  resolvedById?: number;
  resolvedBy?: Staff;
  createdAt: string;
  updatedAt: string;
  comments?: ServiceRequestComment[];
}

export interface ServiceRequestComment {
  id: number;
  serviceRequestId: number;
  staffId: number;
  staff?: Staff;
  commentText: string;
  createdAt: string;
}
