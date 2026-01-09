export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  displayName: string;
  role: string;
  expiresAt: string;
}

export interface User {
  username: string;
  displayName: string;
  role: 'Staff' | 'Supervisor' | 'Admin';
}
