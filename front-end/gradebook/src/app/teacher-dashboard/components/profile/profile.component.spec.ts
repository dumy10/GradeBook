import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { ProfileComponent } from './profile.component';

describe('ProfileComponent', () => {
  let component: ProfileComponent;
  let fixture: ComponentFixture<ProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfileComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ProfileComponent);
    component = fixture.componentInstance;

    // Set up default test input values
    component.userData = {
      firstName: 'John',
      lastName: 'Doe',
      phone: '555-123-4567',
      address: '123 Main St',
    };
    component.userInfo = {
      email: 'john.doe@example.com',
      username: 'johndoe',
    };

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display user info correctly', () => {
    fixture.detectChanges();
    const emailElement = fixture.debugElement.query(By.css('.user-email'));
    const usernameElement = fixture.debugElement.query(By.css('.username'));

    expect(emailElement.nativeElement.value).toContain('john.doe@example.com');
    expect(usernameElement.nativeElement.value).toContain('johndoe');
  });

  it('should display user data in form inputs', () => {
    fixture.detectChanges();

    const firstNameInput = fixture.debugElement.query(
      By.css('input[name="firstName"]')
    );
    const lastNameInput = fixture.debugElement.query(
      By.css('input[name="lastName"]')
    );
    const phoneInput = fixture.debugElement.query(
      By.css('input[name="phone"]')
    );
    const addressInput = fixture.debugElement.query(
      By.css('textarea[name="address"]')
    );

    expect(firstNameInput.nativeElement.value).toBe('John');
    expect(lastNameInput.nativeElement.value).toBe('Doe');
    expect(phoneInput.nativeElement.value).toBe('555-123-4567');
    expect(addressInput.nativeElement.value).toBe('123 Main St');
  });

  it('should emit updateProfileEvent when form is submitted', () => {
    spyOn(component.updateProfileEvent, 'emit');

    // Trigger form submission
    const form = fixture.debugElement.query(By.css('form'));
    form.triggerEventHandler('submit', null);

    expect(component.updateProfileEvent.emit).toHaveBeenCalledWith(
      component.userData
    );
  });

  it('should update userData when form inputs change', () => {
    const firstNameInput = fixture.debugElement.query(
      By.css('input[name="firstName"]')
    );
    firstNameInput.nativeElement.value = 'Jane';
    firstNameInput.nativeElement.dispatchEvent(new Event('input'));

    fixture.detectChanges();

    expect(component.userData.firstName).toBe('Jane');
  });

  it('should display success message when provided', () => {
    component.successMessage = 'Profile updated successfully';
    fixture.detectChanges();

    const successAlert = fixture.debugElement.query(By.css('.alert-success'));
    expect(successAlert.nativeElement.textContent.trim()).toContain(
      'Profile updated successfully'
    );
  });

  it('should display error message when provided', () => {
    component.errorMessage = 'Failed to update profile';
    fixture.detectChanges();

    const errorAlert = fixture.debugElement.query(By.css('.alert-danger'));
    expect(errorAlert.nativeElement.textContent.trim()).toContain(
      'Failed to update profile'
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
});
