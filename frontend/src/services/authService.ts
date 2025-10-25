import apiClient from './api';
import { AuthResponse, LoginCredentials, RegisterCredentials, User } from '../types/auth.types';

export const authService = {
  async login(credentials: LoginCredentials): Promise<{ token: string; user: User }> {
    const response = await apiClient.post<AuthResponse>('/auth/login', credentials);
    const data = response.data;
    
    // Transform backend response to frontend format
    return {
      token: data.token,
      user: {
        id: String(data.userId),
        email: data.email,
        createdAt: new Date().toISOString()
      }
    };
  },

  async register(credentials: RegisterCredentials): Promise<{ token: string; user: User }> {
    // Remove confirmPassword before sending to backend
    const { confirmPassword, ...registerData } = credentials;
    
    const response = await apiClient.post<AuthResponse>('/auth/register', registerData);
    const data = response.data;
    
    // Transform backend response to frontend format
    return {
      token: data.token,
      user: {
        id: String(data.userId),
        email: data.email,
        createdAt: new Date().toISOString()
      }
    };
  },

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getStoredToken(): string | null {
    return localStorage.getItem('token');
  },

  getStoredUser(): User | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  storeAuth(token: string, user: User): void {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
  }
};
