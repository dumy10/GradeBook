import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, PasswordChangeRequest, ProfileUpdateRequest } from '../services/auth.service';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrls: ['./teacher-dashboard.component.scss']
})
export class TeacherDashboardComponent implements OnInit {
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
      this.router.navigate(['/teacher']);
      return;
    }

    // Check if user role is teacher
    const userRole = this.authService.getUserRole();
    if (userRole?.toUpperCase() !== 'TEACHER') {
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

    this.authService.updateProfile(this.userData).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Profile updated successfully!';
        } else {
          this.errorMessage = response.message || 'Failed to update profile';
        }
        this.isLoading = false;
      },
      error: (error) => {
        if (error.status === 405) {
          this.errorMessage = 'Method Not Allowed: The server doesn\'t support this request method for this endpoint.';
        } else {
          this.errorMessage = error.error?.message || 'An error occurred while updating profile';
        }
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

    this.authService.changePassword(this.passwordData).subscribe({
      next: (response) => {
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