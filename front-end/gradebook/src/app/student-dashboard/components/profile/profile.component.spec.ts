import { CommonModule } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { ProfileComponent } from './profile.component';

describe('ProfileComponent', () => {
  let component: ProfileComponent;
  let fixture: ComponentFixture<ProfileComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  const mockUserData = {
    email: 'test@example.com',
    username: 'testuser',
    userId: 1,
    firstName: 'Test',
    lastName: 'User',
  };

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('AuthService', ['updateProfile']);

    await TestBed.configureTestingModule({
      imports: [FormsModule, CommonModule, ProfileComponent],
      providers: [{ provide: AuthService, useValue: spy }],
    }).compileComponents();

    // Mock localStorage
    spyOn(localStorage, 'getItem').and.returnValue(
      JSON.stringify(mockUserData)
    );

    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    fixture = TestBed.createComponent(ProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load user data from localStorage on init', () => {
    // Expect values loaded from localStorage
    expect(component.userInfo.email).toBe(mockUserData.email);
    expect(component.userInfo.username).toBe(mockUserData.username);
    expect(component.userInfo.userId).toBe(mockUserData.userId);
    expect(component.userData.firstName).toBe(mockUserData.firstName);
    expect(component.userData.lastName).toBe(mockUserData.lastName);
  });

  it('should display user data in the form', () => {
    fixture.detectChanges();

    // Check readonly fields
    const emailField = fixture.debugElement.query(
      By.css('input[type="email"]')
    ).nativeElement;
    const usernameField = fixture.debugElement.query(
      By.css('input[type="text"][readonly]')
    ).nativeElement;

    expect(emailField.value).toBe(mockUserData.email);
    expect(usernameField.value).toBe(mockUserData.username);

    // Check editable fields
    const firstNameField = fixture.debugElement.query(
      By.css('#firstName')
    ).nativeElement;
    const lastNameField = fixture.debugElement.query(
      By.css('#lastName')
    ).nativeElement;

    expect(firstNameField.value).toBe(mockUserData.firstName);
    expect(lastNameField.value).toBe(mockUserData.lastName);
  });

  it('should call updateProfile service method on form submit', () => {
    // Setup the response
    const mockResponse = {
      success: true,
      message: 'Profile updated successfully',
    };
    authServiceSpy.updateProfile.and.returnValue(of(mockResponse));

    // Set form values
    component.userData = {
      firstName: 'Updated',
      lastName: 'Name',
      phone: '1234567890',
      address: '123 Test St',
    };

    // Submit form
    component.updateProfile();
    fixture.detectChanges();

    // Verify service was called with correct data
    expect(authServiceSpy.updateProfile).toHaveBeenCalledWith(
      component.userData
    );
    expect(component.successMessage).toContain('Profile updated successfully');
    expect(component.errorMessage).toBe('');
  });

  it('should handle error when update profile fails', () => {
    // Setup the error response
    const errorResponse = { status: 400, error: { message: 'Invalid data' } };
    authServiceSpy.updateProfile.and.returnValue(
      throwError(() => errorResponse)
    );

    // Submit form
    component.updateProfile();
    fixture.detectChanges();

    // Verify error is displayed
    expect(component.successMessage).toBe('');
    expect(component.errorMessage).not.toBe('');
    expect(component.isLoading).toBe(false);
  });

  it('should handle method not allowed error specifically', () => {
    // Setup the error response for method not allowed (405)
    const errorResponse = { status: 405 };
    authServiceSpy.updateProfile.and.returnValue(
      throwError(() => errorResponse)
    );

    // Submit form
    component.updateProfile();
    fixture.detectChanges();

    // Verify specific error message
    expect(component.errorMessage).toContain('Method Not Allowed');
    expect(component.isLoading).toBe(false);
  });
});
