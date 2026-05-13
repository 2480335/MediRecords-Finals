export interface UserView {
  userId: number;
  name: string;
  email: string;
  phone?: string | null;
  roleName: string;
  status: string;
}

export interface UserRegisterRequest {
  name: string;
  roleId: number;
  email: string;
  phone?: string;
  password: string;
  status: boolean;
}

export interface UserUpdateRequest {
  userID: number;
  name: string;
  phone: string;
  email: string;
  roleID: number;
  status: boolean;
}

export interface UserUpdateResponse {
  userID: number;
  name: string;
  roleID: number;
  email: string;
  phone: string;
  status: boolean;
}

/**
 * Live role from the backend's UserRole table.
 * The roleId values match the database primary keys, so we can avoid
 * hard-coding the mapping and stay correct even if rows are reseeded.
 */
export interface RoleOption {
  roleId: number;
  name: string;
}

export interface ProviderLookup {
  providerId: number;
  name: string;
  email: string;
}

const ROLE_DESCRIPTIONS: Record<string, string> = {
  Admin: 'Full system access',
  Physician: 'Clinical workspace',
  Nurse: 'Vitals & nursing notes',
  LabTech: 'Lab results & imaging reports',
  FrontDesk: 'Patient registration & appointments'
};

export function describeRole(name: string): string {
  return ROLE_DESCRIPTIONS[name] ?? '';
}
