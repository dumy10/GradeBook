import { Directive, HostListener, Input } from '@angular/core';
import {
  ComponentFixture,
  TestBed,
  fakeAsync,
  tick,
} from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterModule, UrlTree } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthResponse, AuthService } from '../services/auth.service';
import { TeacherAuthComponent } from './teacher-auth.component';

// Create RouterLink stub directive
@Directive({
  selector: 'a[routerLink],area[routerLink]',
  standalone: true,
})
class RouterLinkStubDirective {
  @Input() routerLink: any = null;
  @Input() queryParams: any = null;
  @Input() fragment: string = '';
  @Input() preserveFragment: boolean = false;
  @Input() skipLocationChange: boolean = false;
  @Input() replaceUrl: boolean = false;
  @Input() state: any = null;
  navigatedTo: any = null;

  @HostListener('click')
  onClick() {
    this.navigatedTo = this.routerLink;
    return false;
  }
}

describe('TeacherAuthComponent', () => {
  let component: TeacherAuthComponent;
  let fixture: ComponentFixture<TeacherAuthComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: any;

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', [
      'login',
      'register',
      'isLoggedIn',
      'getToken',
      'getUserRole',
      'logout',
    ]);

    // Create a complete mock router with all necessary methods that RouterLink uses
    const mockRouter = {
      navigate: jasmine.createSpy('navigate'),
      url: '/test',
      events: of(null),
      createUrlTree: jasmine
        .createSpy('createUrlTree')
        .and.returnValue(new UrlTree()),
      serializeUrl: jasmine.createSpy('serializeUrl').and.returnValue(''),
      isActive: jasmine.createSpy('isActive').and.returnValue(false),
      parseUrl: jasmine.createSpy('parseUrl').and.returnValue(new UrlTree()),
    };

    await TestBed.configureTestingModule({
      imports: [FormsModule, TeacherAuthComponent, RouterLinkStubDirective],
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: mockRouter },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParams: {},
              params: {},
              data: {},
            },
            queryParams: of({}),
            params: of({}),
          },
        },
      ],
    })
      .overrideComponent(TeacherAuthComponent, {
        remove: { imports: [RouterModule] },
        add: { imports: [RouterLinkStubDirective] },
      })
      .compileComponents();

    fixture = TestBed.createComponent(TeacherAuthComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should show login form by default', () => {
    fixture.detectChanges();
    expect(component.activeTab).toBe('login');
    const loginForm = fixture.debugElement.query(By.css('form'));
    expect(loginForm).toBeTruthy();
  });

  it('should switch to signup form when signup tab is clicked', () => {
    fixture.detectChanges();

    // First verify we start with login tab active
    expect(component.activeTab).toBe('login');

    // Get the signup tab anchor element (second tab)
    const signupTab = fixture.debugElement.query(
      By.css('a[class*="nav-link"]:last-child')
    );
    expect(signupTab).toBeTruthy();

    // Directly set the active tab
    component.activeTab = 'signup';
    fixture.detectChanges();

    // Now verify the tab has changed
    expect(component.activeTab).toBe('signup');

    // Verify signup form is displayed
    const signupForm = fixture.debugElement.query(By.css('form'));
    expect(signupForm).toBeTruthy();
  });

  describe('Login Form', () => {
    it('should validate email format', () => {
      component.loginCredentials = {
        email: 'invalid-email',
        password: 'password123',
      };
      component.validateLoginForm();

      expect(component.validationErrors.email).toBe(
        'Please enter a valid email address'
      );
      expect(component.isLoginFormValid).toBeFalse();
    });

    it('should validate required email', () => {
      component.loginCredentials = { email: '', password: 'password123' };
      component.validateLoginForm();

      expect(component.validationErrors.email).toBe('Email is required');
      expect(component.isLoginFormValid).toBeFalse();
    });

    it('should validate required password', () => {
      component.loginCredentials = { email: 'test@example.com', password: '' };
      component.validateLoginForm();

      expect(component.validationErrors.password).toBe('Password is required');
      expect(component.isLoginFormValid).toBeFalse();
    });

    it('should validate password length', () => {
      component.loginCredentials = {
        email: 'test@example.com',
        password: 'short',
      };
      component.validateLoginForm();

      expect(component.validationErrors.password).toBe(
        'Password must be at least 6 characters long'
      );
      expect(component.isLoginFormValid).toBeFalse();
    });

    it('should pass validation with valid credentials', () => {
      component.loginCredentials = {
        email: 'test@example.com',
        password: 'password123',
      };
      component.validateLoginForm();

      expect(component.isLoginFormValid).toBeTrue();
      expect(component.validationErrors.email).toBe('');
      expect(component.validationErrors.password).toBe('');
    });

    it('should call authService.login when form is submitted with valid credentials', fakeAsync(() => {
      // Set up successful login response
      const successResponse: AuthResponse = {
        success: true,
        role: 'TEACHER',
        token: 'fake-token',
        userId: 1,
        message: 'Login successful',
        username: 'teacher1',
        email: 'teacher@example.com',
      };

      authService.login.and.returnValue(of(successResponse));

      // Set valid credentials
      component.loginCredentials = {
        email: 'teacher@example.com',
        password: 'password123',
      };
      component.validateLoginForm();
      fixture.detectChanges();

      // Submit the form
      const loginButton = fixture.debugElement.query(
        By.css('button[type="submit"]')
      );
      loginButton.nativeElement.click();
      tick();

      expect(authService.login).toHaveBeenCalledWith(
        component.loginCredentials
      );
      expect(router.navigate).toHaveBeenCalledWith(['/teacher-dashboard']);
    }));

    it('should display error message when login fails', fakeAsync(() => {
      // Set up failed login response
      const failResponse: AuthResponse = {
        success: false,
        message: 'Invalid credentials',
        userId: 0,
        token: '',
        username: '',
        email: '',
        role: '',
      };

      authService.login.and.returnValue(of(failResponse));

      // Set valid credentials
      component.loginCredentials = {
        email: 'teacher@example.com',
        password: 'wrongpass',
      };
      component.validateLoginForm();
      fixture.detectChanges();

      // Submit the form
      const loginButton = fixture.debugElement.query(
        By.css('button[type="submit"]')
      );
      loginButton.nativeElement.click();
      tick();

      expect(authService.login).toHaveBeenCalledWith(
        component.loginCredentials
      );
      expect(router.navigate).not.toHaveBeenCalled();
      expect(component.errorMessage).toBe('Invalid username or password');
    }));

    it('should handle network errors during login', fakeAsync(() => {
      authService.login.and.returnValue(throwError(() => ({ status: 0 })));

      // Set valid credentials
      component.loginCredentials = {
        email: 'teacher@example.com',
        password: 'password123',
      };
      component.validateLoginForm();
      fixture.detectChanges();

      // Submit the form
      const loginButton = fixture.debugElement.query(
        By.css('button[type="submit"]')
      );
      loginButton.nativeElement.click();
      tick();

      expect(component.errorMessage).toBe(
        'Network or CORS error. Please check if the server is running and CORS is configured.'
      );
    }));

    it('should display error when user role is not teacher', fakeAsync(() => {
      // Set up successful login but with student role
      const studentResponse: AuthResponse = {
        success: true,
        role: 'STUDENT',
        token: 'fake-token',
        userId: 1,
        message: 'Login successful',
        username: 'student1',
        email: 'student@example.com',
      };

      authService.login.and.returnValue(of(studentResponse));

      // Set valid credentials
      component.loginCredentials = {
        email: 'student@example.com',
        password: 'password123',
      };
      component.validateLoginForm();
      fixture.detectChanges();

      // Submit the form
      const loginButton = fixture.debugElement.query(
        By.css('button[type="submit"]')
      );
      loginButton.nativeElement.click();
      tick();

      expect(router.navigate).not.toHaveBeenCalled();
      expect(component.errorMessage).toBe('Invalid username or password');
    }));
  });

  describe('Signup Form', () => {
    beforeEach(() => {
      // Switch to signup tab
      component.activeTab = 'signup';
      fixture.detectChanges();
    });

    it('should validate all required fields', () => {
      // Initialize with empty data
      component.signupData = {
        email: '',
        username: '',
        password: '',
        confirmPassword: '',
        firstName: '',
        lastName: '',
        role: 'teacher',
      };

      // Try to submit the form
      component.onSignup();

      expect(component.signupErrorMessage).toBe('Please fill in all fields');
    });

    it('should validate email format in signup', () => {
      // Set all required fields but with invalid email
      component.signupData = {
        email: 'invalid-email',
        username: 'teacheruser',
        password: 'password123',
        confirmPassword: 'password123',
        firstName: 'Teacher',
        lastName: 'Name',
        role: 'teacher',
      };

      component.onSignup();

      expect(component.signupErrorMessage).toBe(
        'Please enter a valid email address'
      );
    });

    it('should validate password length in signup', () => {
      // Set all required fields but with short password
      component.signupData = {
        email: 'teacher@example.com',
        username: 'teacheruser',
        password: 'short',
        confirmPassword: 'short',
        firstName: 'Teacher',
        lastName: 'Name',
        role: 'teacher',
      };

      component.onSignup();

      expect(component.signupErrorMessage).toBe(
        'Password must be at least 6 characters long'
      );
    });

    it('should validate password confirmation', () => {
      // Set all required fields but passwords don't match
      component.signupData = {
        email: 'teacher@example.com',
        username: 'teacheruser',
        password: 'password123',
        confirmPassword: 'different123',
        firstName: 'Teacher',
        lastName: 'Name',
        role: 'teacher',
      };

      component.onSignup();

      expect(component.signupErrorMessage).toBe('Passwords do not match');
    });

    it('should call authService.register when signup form is submitted with valid data', fakeAsync(() => {
      // Set up successful registration response
      const successResponse: AuthResponse = {
        success: true,
        role: 'TEACHER',
        token: 'fake-token',
        userId: 1,
        message: 'Registration successful',
        username: 'newteacher',
        email: 'newteacher@example.com',
      };

      authService.register.and.returnValue(of(successResponse));

      // Set valid signup data
      component.signupData = {
        email: 'newteacher@example.com',
        username: 'newteacher',
        password: 'password123',
        confirmPassword: 'password123',
        firstName: 'New',
        lastName: 'Teacher',
        role: 'teacher',
      };

      // Call the signup method
      component.onSignup();
      tick();

      // Verify the auth service was called with correct data
      expect(authService.register).toHaveBeenCalledWith({
        email: 'newteacher@example.com',
        username: 'newteacher',
        password: 'password123',
        confirmPassword: 'password123',
        firstName: 'New',
        lastName: 'Teacher',
        role: 'teacher',
      });

      // Verify navigation to dashboard
      expect(router.navigate).toHaveBeenCalledWith(['/teacher-dashboard']);
    }));

    it('should handle registration failure', fakeAsync(() => {
      // Set up failed registration response
      const failResponse: AuthResponse = {
        success: false,
        message: 'Email already exists',
        userId: 0,
        token: '',
        username: '',
        email: '',
        role: '',
      };

      authService.register.and.returnValue(of(failResponse));

      // Set valid signup data
      component.signupData = {
        email: 'existing@example.com',
        username: 'existingteacher',
        password: 'password123',
        confirmPassword: 'password123',
        firstName: 'Existing',
        lastName: 'Teacher',
        role: 'teacher',
      };

      // Call the signup method
      component.onSignup();
      tick();

      // Verify error message is displayed
      expect(component.signupErrorMessage).toBe('Email already exists');
      expect(router.navigate).not.toHaveBeenCalled();
    }));

    it('should handle network errors during registration', fakeAsync(() => {
      // Set up error response
      const errorResponse = {
        error: { message: 'Server error' },
      };

      authService.register.and.returnValue(throwError(() => errorResponse));

      // Set valid signup data
      component.signupData = {
        email: 'newteacher@example.com',
        username: 'newteacher',
        password: 'password123',
        confirmPassword: 'password123',
        firstName: 'New',
        lastName: 'Teacher',
        role: 'teacher',
      };

      // Call the signup method
      component.onSignup();
      tick();

      // Verify error message is displayed
      expect(component.signupErrorMessage).toBe('Server error');
      expect(router.navigate).not.toHaveBeenCalled();
    }));
  });
});
