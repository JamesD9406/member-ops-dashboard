import { Member } from './member.models';

export interface AuditLog {
  id: number;
  memberId: number;
  member?: Member;
  actor: string;
  action: string;
  details?: string;
  timestamp: string;
}
