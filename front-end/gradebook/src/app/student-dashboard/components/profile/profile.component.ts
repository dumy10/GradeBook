import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  AuthService,
  ProfileUpdateRequest,
} from '../../../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  userData: ProfileUpdateRequest = {
    firstName: '',
    lastName: '',
    phone: '',
    address: '',
  };

  userInfo = {
    email: '',
    username: '',
    userId: 0,
  };

  successMessage = '';
  errorMessage = '';
  isLoading = false;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    // Get user data from localStorage
    const userData = localStorage.getItem('currentUser');
    if (userData) {
      const parsedData = JSON.parse(userData);
      this.userInfo.email = parsedData.email || '';
      this.userInfo.username = parsedData.username || '';
      this.userInfo.userId = parsedData.userId || 0;

      // Initial values for first and last name
      this.userData.firstName = parsedData.firstName || '';
      this.userData.lastName = parsedData.lastName || '';
    }
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
          this.errorMessage =
            "Method Not Allowed: The server doesn't support this request method for this endpoint.";
        } else {
          this.errorMessage =
            error.error?.message || 'An error occurred while updating profile';
        }

        this.isLoading = false;
      },
    });
  }
}
