import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService, LoginRequest, RegisterRequest } from '../services/auth.service';

interface SignupData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  username: string;
  role: string;
}

@Component({
  selector: 'app-teacher-auth',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './teacher-auth.component.html',
  styleUrls: ['./teacher-auth.component.scss']
})
export class TeacherAuthComponent {
  activeTab: 'login' | 'signup' = 'login';
  
  // Login form
  loginCredentials: LoginRequest = {
    email: '',
    password: ''
  };
  
  // Signup form
  signupData: SignupData = {
    email: '',
    username: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    role: 'teacher'
  };
  
  errorMessage: string = '';
  signupErrorMessage: string = '';
  isLoading: boolean = false;
  isSignupLoading: boolean = false;
  isLoginFormValid: boolean = false;
  validationErrors = {
    email: '',
    password: ''
  };

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  validateLoginForm(): void {
    // Clear any existing error message when the input changes
    this.errorMessage = '';
    
    // Reset validation errors
    this.validationErrors = {
      email: '',
      password: ''
    };

    // Validate email
    if (!this.loginCredentials.email) {
      this.validationErrors.email = 'Email is required';
    } else if (!this.isValidEmail(this.loginCredentials.email)) {
      this.validationErrors.email = 'Please enter a valid email address';
    }

    // Validate password
    if (!this.loginCredentials.password) {
      this.validationErrors.password = 'Password is required';
    } else if (this.loginCredentials.password.length < 6) {
      this.validationErrors.password = 'Password must be at least 6 characters long';
    }

    // Check if form is valid
    this.isLoginFormValid = !Object.values(this.validationErrors).some(error => error !== '');
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  private resetFormState(): void {
    this.isLoading = false;
    // Don't clear error message here - we want to keep displaying it until user starts typing again
  }

  private resetSignupFormState(): void {
    this.isSignupLoading = false;
    // Don't clear error message here - we want to keep displaying it until user starts typing again
  }

  onLogin(): void {
    if (!this.isLoginFormValid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginCredentials).subscribe({
      next: (response) => {
        if (response.success) {
          // Check if the user has the correct role
          if (response.role.toLowerCase() === 'teacher') {
            this.router.navigate(['/teacher-dashboard']);
            console.log(response);
          } else {
            // Use generic error message for wrong role
            this.errorMessage = 'Invalid username or password';
            this.resetFormState();
          }
        } else {
          this.errorMessage = 'Invalid username or password';
          this.resetFormState();
        }
      },
      error: (error) => {
        this.errorMessage = 'Invalid username or password';
        this.resetFormState();
      },
      complete: () => {
        // Only reset if we haven't already in error/unsuccessful cases
        if (this.isLoading) {
          this.resetFormState();
        }
      }
    });
  }
  
  onSignup(): void {
    // Reset error message
    this.signupErrorMessage = '';
    
    // Basic validation
    if (!this.signupData.firstName || !this.signupData.lastName || 
        !this.signupData.email || !this.signupData.password || 
        !this.signupData.confirmPassword || !this.signupData.username) {
      this.signupErrorMessage = 'Please fill in all fields';
      return;
    }
    
    if (!this.isValidEmail(this.signupData.email)) {
      this.signupErrorMessage = 'Please enter a valid email address';
      return;
    }
    
    if (this.signupData.password.length < 6) {
      this.signupErrorMessage = 'Password must be at least 6 characters long';
      return;
    }
    
    if (this.signupData.password !== this.signupData.confirmPassword) {
      this.signupErrorMessage = 'Passwords do not match';
      return;
    }
    
    // Prepare registration data
    const registerData: RegisterRequest = {
      email: this.signupData.email,
      username: this.signupData.username,
      password: this.signupData.password,
      confirmPassword: this.signupData.confirmPassword,
      firstName: this.signupData.firstName,
      lastName: this.signupData.lastName,
      role: 'teacher' // Set role to teacher for teacher registration
    };
    
    // Start loading state
    this.isSignupLoading = true;
    console.log(registerData);
    // Call registration service
    this.authService.register(registerData).subscribe({
      next: (response) => {
        if (response.success) {
          // Registration successful, navigate to dashboard
          this.router.navigate(['/teacher-dashboard']);
        } else {
          this.signupErrorMessage = response.message || 'Registration failed';
          this.resetSignupFormState();
        }
      },
      error: (error) => {
        this.signupErrorMessage = error.error?.message || 'An error occurred during registration';
        this.resetSignupFormState();
      },
      complete: () => {
        // Only reset if we haven't already in error/unsuccessful cases
        if (this.isSignupLoading) {
          this.resetSignupFormState();
        }
      }
    });
  }
}
