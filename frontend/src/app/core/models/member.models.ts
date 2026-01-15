import { AccountFlag } from './account-flag.models';
import { ServiceRequest } from './service-request.models';

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
  flags?: AccountFlag[];
  serviceRequests?: ServiceRequest[];
}
