import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { PasswordChangeComponent } from './password-change.component';

describe('PasswordChangeComponent', () => {
  let component: PasswordChangeComponent;
  let fixture: ComponentFixture<PasswordChangeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PasswordChangeComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(PasswordChangeComponent);
    component = fixture.componentInstance;

    // Initialize test data
    component.passwordData = {
      currentPassword: 'oldPassword123',
      newPassword: 'newPassword123',
      confirmPassword: 'newPassword123',
    };

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display form inputs with correct values', () => {
    fixture.detectChanges();

    const currentPasswordInput = fixture.debugElement.query(
      By.css('input[name="currentPassword"]')
    );
    const newPasswordInput = fixture.debugElement.query(
      By.css('input[name="newPassword"]')
    );
    const confirmPasswordInput = fixture.debugElement.query(
      By.css('input[name="confirmPassword"]')
    );

    expect(currentPasswordInput.nativeElement.value).toBe('oldPassword123');
    expect(newPasswordInput.nativeElement.value).toBe('newPassword123');
    expect(confirmPasswordInput.nativeElement.value).toBe('newPassword123');
  });

  it('should emit changePasswordEvent when form is submitted', () => {
    spyOn(component.changePasswordEvent, 'emit');

    const form = fixture.debugElement.query(By.css('form'));
    form.triggerEventHandler('submit', null);

    expect(component.changePasswordEvent.emit).toHaveBeenCalledWith(
      component.passwordData
    );
  });

  it('should update passwordData when form inputs change', () => {
    const newPasswordInput = fixture.debugElement.query(
      By.css('input[name="newPassword"]')
    );
    newPasswordInput.nativeElement.value = 'updatedPassword123';
    newPasswordInput.nativeElement.dispatchEvent(new Event('input'));

    fixture.detectChanges();

    expect(component.passwordData.newPassword).toBe('updatedPassword123');
  });

  it('should display success message when provided', () => {
    component.successMessage = 'Password changed successfully';
    fixture.detectChanges();

    const successAlert = fixture.debugElement.query(By.css('.alert-success'));
    expect(successAlert.nativeElement.textContent).toContain(
      'Password changed successfully'
    );
  });

  it('should display error message when provided', () => {
    component.errorMessage = 'Failed to change password';
    fixture.detectChanges();

    const errorAlert = fixture.debugElement.query(By.css('.alert-danger'));
    expect(errorAlert.nativeElement.textContent).toContain(
      'Failed to change password'
    );
  });

  it('should show loading spinner when isLoading is true', () => {
    component.isLoading = true;
    fixture.detectChanges();

    const spinner = fixture.debugElement.query(By.css('.spinner-border'));
    expect(spinner).toBeTruthy();
  });

  it('should disable submit button when loading', () => {
    component.isLoading = true;
    fixture.detectChanges();

    const submitButton = fixture.debugElement.query(
      By.css('button[type="submit"]')
    );
    expect(submitButton.nativeElement.disabled).toBeTruthy();
  });

  it('should disable submit button when passwords do not match', () => {
    component.passwordData.confirmPassword = 'mismatchedPassword';
    fixture.detectChanges();

    const submitButton = fixture.debugElement.query(
      By.css('button[type="submit"]')
    );
    expect(submitButton.nativeElement.disabled).toBeTruthy();
  });

  it('should show error message when passwords do not match', () => {
    component.passwordData.confirmPassword = 'mismatchedPassword';
    fixture.detectChanges();

    const passwordMismatchError = fixture.debugElement.query(
      By.css('.password-mismatch-error')
    );
    expect(passwordMismatchError).toBeTruthy();
  });
});
