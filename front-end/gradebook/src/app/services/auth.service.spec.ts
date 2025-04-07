import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  AuthResponse,
  AuthService,
  LoginRequest,
  PasswordChangeRequest,
  ProfileUpdateRequest,
  RegisterRequest,
} from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const apiUrl = 'https://localhost:7203/api';

  const mockAuthResponse: AuthResponse = {
    userId: 1,
    message: 'Login successful',
    token: 'fake-jwt-token',
    username: 'testuser',
    email: 'test@example.com',
    role: 'Student',
    success: true,
  };

  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Verify that no unmatched requests are outstanding
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should authenticate user and store token on successful login', () => {
      const loginRequest: LoginRequest = {
        email: 'test@example.com',
        password: 'password123',
      };

      service.login(loginRequest).subscribe((response) => {
        expect(response).toEqual(mockAuthResponse);
        expect(service.isLoggedIn()).toBeTrue();
        expect(service.getToken()).toBe('fake-jwt-token');
        expect(service.getUserRole()).toBe('Student');

        // Check that the user was stored in localStorage
        expect(localStorage.getItem('currentUser')).toBeTruthy();
        expect(JSON.parse(localStorage.getItem('currentUser')!)).toEqual(
          mockAuthResponse
        );
      });

      const req = httpMock.expectOne(`${apiUrl}/Auth/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(loginRequest);
      req.flush(mockAuthResponse);
    });

    it('should not update stored user data on failed login', () => {
      const loginRequest: LoginRequest = {
        email: 'test@example.com',
        password: 'wrong-password',
      };

      const failedResponse = {
        userId: 0,
        message: 'Invalid credentials',
        token: '',
        username: '',
        email: '',
        role: '',
        success: false,
      };

      service.login(loginRequest).subscribe((response) => {
        expect(response).toEqual(failedResponse);
        expect(service.isLoggedIn()).toBeFalse();
        expect(service.getToken()).toBeNull();

        // Check that no user was stored in localStorage
        expect(localStorage.getItem('currentUser')).toBeNull();
      });

      const req = httpMock.expectOne(`${apiUrl}/Auth/login`);
      req.flush(failedResponse);
    });
  });

  describe('register', () => {
    it('should register a new user and store token on success', () => {
      const registerRequest: RegisterRequest = {
        email: 'newuser@example.com',
        username: 'newuser',
        password: 'password123',
        confirmPassword: 'password123',
        firstName: 'New',
        lastName: 'User',
        role: 'Student',
      };

      const registerResponse: AuthResponse = {
        userId: 2,
        message: 'Registration successful',
        token: 'new-user-token',
        username: 'newuser',
        email: 'newuser@example.com',
        role: 'Student',
        success: true,
      };

      service.register(registerRequest).subscribe((response) => {
        expect(response).toEqual(registerResponse);
        expect(service.isLoggedIn()).toBeTrue();
        expect(service.getToken()).toBe('new-user-token');

        // Check that the user was stored in localStorage
        expect(localStorage.getItem('currentUser')).toBeTruthy();
        expect(JSON.parse(localStorage.getItem('currentUser')!)).toEqual(
          registerResponse
        );
      });

      const req = httpMock.expectOne(`${apiUrl}/Auth/register`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(registerRequest);
      req.flush(registerResponse);
    });

    it('should not store user data on failed registration', () => {
      const registerRequest: RegisterRequest = {
        email: 'existing@example.com',
        username: 'existinguser',
        password: 'password123',
        confirmPassword: 'password123',
        firstName: 'Existing',
        lastName: 'User',
        role: 'Student',
      };

      const failedResponse = {
        userId: 0,
        message: 'User with this email already exists',
        token: '',
        username: '',
        email: '',
        role: '',
        success: false,
      };

      service.register(registerRequest).subscribe((response) => {
        expect(response).toEqual(failedResponse);
        expect(service.isLoggedIn()).toBeFalse();

        // Check that no user was stored in localStorage
        expect(localStorage.getItem('currentUser')).toBeNull();
      });

      const req = httpMock.expectOne(`${apiUrl}/Auth/register`);
      req.flush(failedResponse);
    });
  });

  describe('logout', () => {
    it('should clear stored user data on logout', () => {
      // First set up a logged in user
      localStorage.setItem('currentUser', JSON.stringify(mockAuthResponse));

      // Create a new service instance to pick up the localStorage data
      service = TestBed.inject(AuthService);

      // Force a login to ensure the BehaviorSubject is updated
      const loginRequest: LoginRequest = {
        email: 'test@example.com',
        password: 'password123',
      };

      service.login(loginRequest).subscribe();

      const loginReq = httpMock.expectOne(`${apiUrl}/Auth/login`);
      loginReq.flush(mockAuthResponse);

      // Verify the user is logged in
      expect(service.isLoggedIn()).toBeTrue();
      expect(service.getToken()).toBe('fake-jwt-token');

      // Call logout
      service.logout();

      // Verify the user is logged out
      expect(service.isLoggedIn()).toBeFalse();
      expect(service.getToken()).toBeNull();
      expect(localStorage.getItem('currentUser')).toBeNull();
    });
  });

  describe('getAuthHeaders', () => {
    it('should return headers with auth token when logged in', () => {
      // Set up a logged in user
      localStorage.setItem('currentUser', JSON.stringify(mockAuthResponse));

      // Create a new service instance to pick up the localStorage data
      service = TestBed.inject(AuthService);

      // Force a login to ensure the BehaviorSubject is updated
      const loginRequest: LoginRequest = {
        email: 'test@example.com',
        password: 'password123',
      };

      service.login(loginRequest).subscribe();

      const loginReq = httpMock.expectOne(`${apiUrl}/Auth/login`);
      loginReq.flush(mockAuthResponse);

      const headers = service.getAuthHeaders();
      expect(headers.get('Content-Type')).toBe('application/json');
      expect(headers.get('Authorization')).toBe('Bearer fake-jwt-token');
    });

    it('should return headers without auth token when not logged in', () => {
      const headers = service.getAuthHeaders();
      expect(headers.get('Content-Type')).toBe('application/json');
      expect(headers.get('Authorization')).toBeNull();
    });
  });

  describe('changePassword', () => {
    it('should send request to change password with auth headers', () => {
      // Set up a logged in user
      localStorage.setItem('currentUser', JSON.stringify(mockAuthResponse));

      // Create a new service instance to pick up the localStorage data
      service = TestBed.inject(AuthService);

      // Force a login to ensure the BehaviorSubject is updated
      const loginRequest: LoginRequest = {
        email: 'test@example.com',
        password: 'password123',
      };

      service.login(loginRequest).subscribe();

      const loginReq = httpMock.expectOne(`${apiUrl}/Auth/login`);
      loginReq.flush(mockAuthResponse);

      const passwordRequest: PasswordChangeRequest = {
        currentPassword: 'oldpassword',
        newPassword: 'newpassword',
        confirmPassword: 'newpassword',
      };

      const expectedResponse = {
        success: true,
        message: 'Password changed successfully',
      };

      service.changePassword(passwordRequest).subscribe((response) => {
        expect(response).toEqual(expectedResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/User/change-password`);
      expect(req.request.method).toBe('POST');
      expect(req.request.headers.get('Authorization')).toBe(
        'Bearer fake-jwt-token'
      );
      expect(req.request.body).toEqual({
        currentPassword: 'oldpassword',
        newPassword: 'newpassword',
        confirmPassword: 'newpassword',
      });

      req.flush(expectedResponse);
    });
  });

  describe('updateProfile', () => {
    it('should send request to update profile with auth headers', () => {
      // Set up a logged in user
      localStorage.setItem('currentUser', JSON.stringify(mockAuthResponse));

      // Create a new service instance to pick up the localStorage data
      service = TestBed.inject(AuthService);

      // Force a login to ensure the BehaviorSubject is updated
      const loginRequest: LoginRequest = {
        email: 'test@example.com',
        password: 'password123',
      };

      service.login(loginRequest).subscribe();

      const loginReq = httpMock.expectOne(`${apiUrl}/Auth/login`);
      loginReq.flush(mockAuthResponse);

      const profileRequest: ProfileUpdateRequest = {
        firstName: 'Updated',
        lastName: 'User',
        phone: '555-123-4567',
        address: '123 Main St',
      };

      const expectedResponse = {
        success: true,
        message: 'Profile updated successfully',
      };

      service.updateProfile(profileRequest).subscribe((response) => {
        expect(response).toEqual(expectedResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/User/profile`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.headers.get('Authorization')).toBe(
        'Bearer fake-jwt-token'
      );
      expect(req.request.body).toEqual(profileRequest);

      req.flush(expectedResponse);
    });
  });
});
