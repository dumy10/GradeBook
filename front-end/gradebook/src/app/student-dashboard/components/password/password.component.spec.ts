import { CommonModule } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { of, Subject, throwError } from 'rxjs';
import {
  AuthService,
  PasswordChangeRequest,
} from '../../../services/auth.service';
import { PasswordComponent } from './password.component';

describe('PasswordComponent', () => {
  let component: PasswordComponent;
  let fixture: ComponentFixture<PasswordComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('AuthService', ['changePassword']);

    await TestBed.configureTestingModule({
      imports: [FormsModule, CommonModule, PasswordComponent],
      providers: [{ provide: AuthService, useValue: spy }],
    }).compileComponents();

    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    fixture = TestBed.createComponent(PasswordComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty form fields', () => {
    expect(component.passwordData.currentPassword).toBe('');
    expect(component.passwordData.newPassword).toBe('');
    expect(component.passwordData.confirmPassword).toBe('');
    expect(component.isLoading).toBeFalse();
    expect(component.successMessage).toBe('');
    expect(component.errorMessage).toBe('');
  });

  it('should validate form fields before submission', () => {
    // Leave all fields empty
    component.changePassword();
    fixture.detectChanges();

    expect(component.errorMessage).toBe('Please fill in all password fields');
    expect(authServiceSpy.changePassword).not.toHaveBeenCalled();

    // Set only current password
    component.passwordData = {
      currentPassword: 'current',
      newPassword: '',
      confirmPassword: '',
    };
    component.changePassword();
    fixture.detectChanges();

    expect(component.errorMessage).toBe('Please fill in all password fields');
    expect(authServiceSpy.changePassword).not.toHaveBeenCalled();
  });

  it('should validate that new passwords match', () => {
    // Set different passwords
    component.passwordData = {
      currentPassword: 'current',
      newPassword: 'new123',
      confirmPassword: 'different123',
    };

    component.changePassword();
    fixture.detectChanges();

    expect(component.errorMessage).toBe('New passwords do not match');
    expect(authServiceSpy.changePassword).not.toHaveBeenCalled();
  });

  it('should validate minimum password length', () => {
    // Set password that's too short
    component.passwordData = {
      currentPassword: 'current',
      newPassword: 'short',
      confirmPassword: 'short',
    };

    component.changePassword();
    fixture.detectChanges();

    expect(component.errorMessage).toBe(
      'Password must be at least 6 characters long'
    );
    expect(authServiceSpy.changePassword).not.toHaveBeenCalled();
  });

  it('should call service when validation passes', () => {
    // Set valid passwords
    const validData: PasswordChangeRequest = {
      currentPassword: 'current123',
      newPassword: 'newpass123',
      confirmPassword: 'newpass123',
    };

    component.passwordData = validData;
    authServiceSpy.changePassword.and.returnValue(
      of({ success: true, message: 'Password changed successfully' })
    );

    component.changePassword();
    fixture.detectChanges();

    expect(authServiceSpy.changePassword).toHaveBeenCalledWith(validData);
    expect(component.isLoading).toBeFalse();
    expect(component.successMessage).toBe('Password changed successfully!');
    expect(component.errorMessage).toBe('');

    // Form fields should be reset
    expect(component.passwordData.currentPassword).toBe('');
    expect(component.passwordData.newPassword).toBe('');
    expect(component.passwordData.confirmPassword).toBe('');
  });

  it('should handle API error - invalid current password (400)', () => {
    component.passwordData = {
      currentPassword: 'wrongpassword',
      newPassword: 'newpass123',
      confirmPassword: 'newpass123',
    };

    // Mock 400 error for invalid password
    authServiceSpy.changePassword.and.returnValue(
      throwError(() => ({ status: 400 }))
    );

    component.changePassword();
    fixture.detectChanges();

    expect(component.isLoading).toBeFalse();
    expect(component.successMessage).toBe('');
    expect(component.errorMessage).toBe(
      'Invalid password data. The current password may be incorrect.'
    );
  });

  it('should handle API error - unauthorized (401)', () => {
    component.passwordData = {
      currentPassword: 'current123',
      newPassword: 'newpass123',
      confirmPassword: 'newpass123',
    };

    // Mock 401 error for unauthorized
    authServiceSpy.changePassword.and.returnValue(
      throwError(() => ({ status: 401 }))
    );

    component.changePassword();
    fixture.detectChanges();

    expect(component.isLoading).toBeFalse();
    expect(component.successMessage).toBe('');
    expect(component.errorMessage).toBe('Unauthorized. Please log in again.');
  });

  it('should handle generic API errors', () => {
    component.passwordData = {
      currentPassword: 'current123',
      newPassword: 'newpass123',
      confirmPassword: 'newpass123',
    };

    // Mock generic error
    authServiceSpy.changePassword.and.returnValue(
      throwError(() => ({
        status: 500,
        error: { message: 'Server error occurred' },
      }))
    );

    component.changePassword();
    fixture.detectChanges();

    expect(component.isLoading).toBeFalse();
    expect(component.successMessage).toBe('');
    expect(component.errorMessage).toBe('Server error occurred');
  });

  it('should handle API failure with message', () => {
    component.passwordData = {
      currentPassword: 'current123',
      newPassword: 'newpass123',
      confirmPassword: 'newpass123',
    };

    // Mock success: false with message
    authServiceSpy.changePassword.and.returnValue(
      of({
        success: false,
        message: 'Password change not allowed at this time',
      })
    );

    component.changePassword();
    fixture.detectChanges();

    expect(component.isLoading).toBeFalse();
    expect(component.successMessage).toBe('');
    expect(component.errorMessage).toBe(
      'Password change not allowed at this time'
    );
  });

  it('should handle API failure without message', () => {
    component.passwordData = {
      currentPassword: 'current123',
      newPassword: 'newpass123',
      confirmPassword: 'newpass123',
    };

    // Mock success: false without message
    authServiceSpy.changePassword.and.returnValue(
      of({
        success: false,
      })
    );

    component.changePassword();
    fixture.detectChanges();

    expect(component.isLoading).toBeFalse();
    expect(component.successMessage).toBe('');
    expect(component.errorMessage).toBe('Failed to change password');
  });

  it('should show loading state during API call', () => {
    component.passwordData = {
      currentPassword: 'current123',
      newPassword: 'newpass123',
      confirmPassword: 'newpass123',
    };

    // Instead of creating a promise that never resolves, we'll use a subject
    // that we can control when it emits
    const subject = new Subject();
    authServiceSpy.changePassword.and.returnValue(subject);

    // Call the method that should set loading state to true
    component.changePassword();
    fixture.detectChanges();

    // Verify loading state before the observable completes
    expect(component.isLoading).toBeTrue();

    // Check if the button shows loading text
    const button = fixture.debugElement.query(
      By.css('button[type="submit"]')
    ).nativeElement;
    expect(button.textContent.trim()).toBe('Changing Password...');
    expect(button.disabled).toBeTrue();

    // Complete the subject to finish the test
    subject.next({ success: true, message: 'Password changed successfully' });
    subject.complete();
  });
});
