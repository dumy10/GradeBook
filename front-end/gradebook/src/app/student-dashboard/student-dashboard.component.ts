import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, PasswordChangeRequest, ProfileUpdateRequest } from '../services/auth.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './student-dashboard.component.html',
  styleUrls: ['./student-dashboard.component.scss']
})
export class StudentDashboardComponent implements OnInit {
  activeTab: 'profile' | 'password' = 'profile';
  userData: ProfileUpdateRequest = {
    firstName: '',
    lastName: '',
    phone: '',
    address: ''
  };

  userInfo = {
    email: '',
    username: ''
  };

  passwordData: PasswordChangeRequest = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  successMessage = '';
  errorMessage = '';
  isLoading = false;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    // Check if user is logged in
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/student']);
      return;
    }

    // Check if user role is student
    const userRole = this.authService.getUserRole();
    if (userRole?.toUpperCase() !== 'STUDENT') {
      this.authService.logout();
      this.router.navigate(['/']);
      return;
    }

    // Get user data from localStorage
    const userData = localStorage.getItem('currentUser');
    if (userData) {
      const parsedData = JSON.parse(userData);
      this.userInfo.email = parsedData.email || '';
      this.userInfo.username = parsedData.username || '';
      
      // Initial values for first and last name
      // You would typically get these from an API call
      this.userData.firstName = parsedData.firstName || '';
      this.userData.lastName = parsedData.lastName || '';
    }
  }

  changeTab(tab: 'profile' | 'password'): void {
    this.activeTab = tab;
    this.successMessage = '';
    this.errorMessage = '';
  }

  updateProfile(): void {
    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    // Log the token to check availability
    const token = this.authService.getToken();
    console.log('Authorization token available:', !!token);
    console.log('Token:', token);

    this.authService.updateProfile(this.userData).subscribe({
      next: (response) => {
        console.log('Profile update response:', response);
        if (response.success) {
          this.successMessage = 'Profile updated successfully!';
          // Note: We don't update localStorage here since email/username don't change
        } else {
          this.errorMessage = response.message || 'Failed to update profile';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Profile update error:', error);
        this.errorMessage = error.error?.message || 'An error occurred while updating profile';
        this.isLoading = false;
      }
    });
  }

  changePassword(): void {
    // Validation
    if (!this.passwordData.currentPassword || 
        !this.passwordData.newPassword || 
        !this.passwordData.confirmPassword) {
      this.errorMessage = 'Please fill in all password fields';
      return;
    }

    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.errorMessage = 'New passwords do not match';
      return;
    }

    if (this.passwordData.newPassword.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters long';
      return;
    }

    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    // Log the token to check availability
    const token = this.authService.getToken();
    console.log('Authorization token available for password change:', !!token);
    console.log('Token for password change:', token);

    this.authService.changePassword(this.passwordData).subscribe({
      next: (response) => {
        console.log('Password change response:', response);
        if (response.success) {
          this.successMessage = 'Password changed successfully!';
          this.passwordData = {
            currentPassword: '',
            newPassword: '',
            confirmPassword: ''
          };
        } else {
          this.errorMessage = response.message || 'Failed to change password';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Password change error:', error);
        console.error('Status:', error.status);
        console.error('Status Text:', error.statusText);
        console.error('Error details:', error.error);
        
        if (error.status === 400) {
          this.errorMessage = 'Invalid password data. The current password may be incorrect.';
        } else if (error.status === 401) {
          this.errorMessage = 'Unauthorized. Please log in again.';
        } else {
          this.errorMessage = error.error?.message || 'An error occurred while changing password';
        }
        
        this.isLoading = false;
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
} 