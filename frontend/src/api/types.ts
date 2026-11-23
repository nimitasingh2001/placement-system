export type Role = 'Student' | 'Admin';

// what POST /api/auth/login returns
export type AuthResponse = {
  token: string;
  expiresAtUtc: string;
  role: Role;
  studentId: number | null;
};

// only the part of GET /api/me/profile that we need so far
export type StudentProfile = {
  studentId: number;
  firstName: string;
  lastName: string | null;
};