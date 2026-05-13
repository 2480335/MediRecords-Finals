export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expires: string;
}

export interface ForgotPasswordRequest {
  email: string;
  newPassword: string;
  confirmPassword: string;
}

export type UserRole = 'Admin' | 'Physician' | 'Nurse' | 'LabTech' | 'FrontDesk';

export interface AuthUser {
  userId: number;
  name: string;
  email: string;
  role: UserRole | null;
  expiresAt: number;
}
