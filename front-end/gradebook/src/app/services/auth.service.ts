import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface AuthResponse {
  userId: number;
  message: string;
  token: string;
  username: string;
  email: string;
  role: string;
  success: boolean;
}

export interface PasswordChangeRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ProfileUpdateRequest {
  firstName: string;
  lastName: string;
  phone: string;
  address: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  // Make sure the URL has the correct protocol
  private readonly API_URL = 'https://localhost:7203/api';
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Check if there's a stored user session
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.API_URL}/Auth/login`, credentials)
      .pipe(
        tap((response) => {
          console.log('Login response from server:', response);
          if (response.success) {
            // Store user details in localStorage
            localStorage.setItem('currentUser', JSON.stringify(response));
            this.currentUserSubject.next(response);
            console.log(
              'User data stored in localStorage and BehaviorSubject updated'
            );
          }
        })
      );
  }

  register(registerData: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.API_URL}/Auth/register`, registerData)
      .pipe(
        tap((response) => {
          console.log('Register response from server:', response);
          if (response.success) {
            // Store user details in localStorage
            localStorage.setItem('currentUser', JSON.stringify(response));
            this.currentUserSubject.next(response);
            console.log(
              'User data stored in localStorage and BehaviorSubject updated'
            );
          }
        })
      );
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }

  getToken(): string | null {
    return this.currentUserSubject.value?.token || null;
  }

  getUserRole(): string | null {
    return this.currentUserSubject.value?.role || null;
  }

  // Get the auth headers for API requests
  getAuthHeaders(): HttpHeaders {
    const token = this.getToken();
    if (token) {
      return new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      });
    }
    return new HttpHeaders({
      'Content-Type': 'application/json',
    });
  }

  // Change password
  changePassword(passwordData: PasswordChangeRequest): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.post<any>(
      `${this.API_URL}/User/change-password`,
      {
        currentPassword: passwordData.currentPassword,
        newPassword: passwordData.newPassword,
        confirmPassword: passwordData.confirmPassword,
      },
      { headers }
    );
  }

  // Update profile
  updateProfile(profileData: ProfileUpdateRequest): Observable<any> {
    const headers = this.getAuthHeaders();

    return this.http.put<any>(`${this.API_URL}/User/profile`, profileData, {
      headers,
    });
  }
}
