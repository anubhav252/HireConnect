export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  role: string;
  fullName: string;
}

export interface AuthResponse {
  token: string;
  userId: number;
  email: string;
  role: string;
}

export interface UserCredential {
  userId: number;
  email: string;
  role: string;
}
