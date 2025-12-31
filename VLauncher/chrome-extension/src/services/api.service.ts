import { config } from '../config/config';

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

export interface RegisterResponse {
  message: string;
  user: {
    id: number;
    googleEmail: string;
    status: number;
  };
}

export class ApiService {
  private baseUrl: string;

  constructor() {
    this.baseUrl = config.apiBaseUrl;
  }

  async registerUser(email: string): Promise<ApiResponse<RegisterResponse>> {
    try {
      const response = await fetch(`${this.baseUrl}/api/extension/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        return {
          success: false,
          error: errorData.error || 'Registration failed',
        };
      }

      const data = await response.json();
      return {
        success: true,
        data,
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Network error',
      };
    }
  }
}

export const apiService = new ApiService();
