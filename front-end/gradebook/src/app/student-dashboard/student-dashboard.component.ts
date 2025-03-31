import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, PasswordChangeRequest, ProfileUpdateRequest } from '../services/auth.service';
import { GradeService, Grade } from '../services/grade.service';
import { NgLetDirective } from './nglet.directive';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, NgLetDirective],
  templateUrl: './student-dashboard.component.html',
  styleUrls: ['./student-dashboard.component.scss']
})
export class StudentDashboardComponent implements OnInit {
  activeTab: 'profile' | 'password' | 'grades' | 'history' = 'grades';
  userData: ProfileUpdateRequest = {
    firstName: '',
    lastName: '',
    phone: '',
    address: ''
  };

  userInfo = {
    email: '',
    username: '',
    userId: 0
  };

  passwordData: PasswordChangeRequest = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  grades: Grade[] = [];
  gradesLoading = false;
  gradesError = '';
  lastErrorDetails: any = null;

  successMessage = '';
  errorMessage = '';
  isLoading = false;

  constructor(
    private authService: AuthService, 
    private gradeService: GradeService,
    private router: Router
  ) {}

  ngOnInit(): void {
    console.log('StudentDashboardComponent initialized');
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
      this.userInfo.userId = parsedData.userId || 0;
      
      // Initial values for first and last name
      this.userData.firstName = parsedData.firstName || '';
      this.userData.lastName = parsedData.lastName || '';
    }
    
    // Set initial tab
    console.log('Setting default tab to:', this.activeTab);
    
    // Load grades on init since the default tab is 'grades'
    this.loadGrades();
  }

  changeTab(tab: 'profile' | 'password' | 'grades' | 'history'): void {
    console.log('Changing tab to:', tab);
    this.activeTab = tab;
    this.successMessage = '';
    this.errorMessage = '';
    
    // Load grades for both grades and history tabs
    if ((tab === 'grades' || tab === 'history') && this.grades.length === 0) {
      this.loadGrades();
    }
  }

  loadGrades(): void {
    if (!this.userInfo.userId) {
      this.gradesError = 'User ID not found. Please try logging in again.';
      return;
    }
    
    this.gradesLoading = true;
    this.gradesError = '';
    this.lastErrorDetails = null;
    
    console.log('Starting to load grades for user ID:', this.userInfo.userId);
    
    this.gradeService.getStudentGrades(this.userInfo.userId).subscribe({
      next: (grades) => {
        console.log('Grades loaded successfully:', grades);
        console.log('Number of grades:', grades.length);
        console.log('First grade (if exists):', grades.length > 0 ? grades[0] : 'No grades');
        this.grades = grades;
        this.gradesLoading = false;
      },
      error: (error) => {
        console.error('Error loading grades:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.message);
        console.error('Error details:', error.error);
        this.gradesError = 'Failed to load grades. Please try again later.';
        this.gradesLoading = false;
        
        // Store error details for display
        this.lastErrorDetails = {
          status: error.status,
          message: error.message,
          details: error.error
        };
      }
    });
  }

  // Calculate the percentage score for a grade
  calculatePercentage(points: number, maxPoints: number): number {
    return Math.round((points / maxPoints) * 100);
  }

  // Format a date string to a more readable format
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  // Group grades by course
  getGradesByCourse(): { courseName: string, grades: Grade[] }[] {
    const courseMap = new Map<string, Grade[]>();
    
    this.grades.forEach(grade => {
      const courseName = grade.assignment.class.course.name;
      if (!courseMap.has(courseName)) {
        courseMap.set(courseName, []);
      }
      courseMap.get(courseName)?.push(grade);
    });
    
    return Array.from(courseMap).map(([courseName, grades]) => ({
      courseName,
      grades
    }));
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
        console.error('Status:', error.status);
        console.error('Status Text:', error.statusText);
        console.error('Error details:', error.error);
        
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

  // Calculate overall average for all grades
  calculateOverallAverage(): number {
    if (this.grades.length === 0) return 0;
    
    const totalPoints = this.grades.reduce((sum, grade) => sum + grade.points, 0);
    const totalMaxPoints = this.grades.reduce((sum, grade) => sum + grade.assignment.maxPoints, 0);
    
    return totalMaxPoints > 0 ? Math.round((totalPoints / totalMaxPoints) * 100) : 0;
  }

  // Calculate course average
  calculateCourseAverage(grades: Grade[]): number {
    if (grades.length === 0) return 0;
    
    const totalPoints = grades.reduce((sum, grade) => sum + grade.points, 0);
    const totalMaxPoints = grades.reduce((sum, grade) => sum + grade.assignment.maxPoints, 0);
    
    return totalMaxPoints > 0 ? Math.round((totalPoints / totalMaxPoints) * 100) : 0;
  }

  // Add a method to get grades organized by date
  getGradeHistory(): { date: string, grades: Grade[] }[] {
    // Clone and sort grades by createdAt date (newest first)
    const sortedGrades = [...this.grades].sort((a, b) => 
      new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    );
    
    // Group by date
    const dateMap = new Map<string, Grade[]>();
    
    sortedGrades.forEach(grade => {
      const dateKey = this.formatDate(grade.createdAt);
      if (!dateMap.has(dateKey)) {
        dateMap.set(dateKey, []);
      }
      dateMap.get(dateKey)?.push(grade);
    });
    
    return Array.from(dateMap).map(([date, grades]) => ({
      date,
      grades
    }));
  }
  
  // Get the time part from a datetime string
  formatTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit'
    });
  }
  
  // Calculate the day difference between two dates
  getDaysSince(dateString: string): number {
    const today = new Date();
    const date = new Date(dateString);
    const diffTime = Math.abs(today.getTime() - date.getTime());
    return Math.floor(diffTime / (1000 * 60 * 60 * 24));
  }
  
  // Get a relative date description for a date
  getRelativeDate(dateString: string): string {
    const days = this.getDaysSince(dateString);
    
    if (days === 0) {
      return 'Today';
    } else if (days === 1) {
      return 'Yesterday';
    } else if (days <= 7) {
      return `${days} days ago`;
    } else {
      return this.formatDate(dateString);
    }
  }
  
  // Get color class based on changes between grades
  getChangeColorClass(current: number, previous: number): string {
    if (!previous) return '';
    
    if (current > previous) {
      return 'text-success';
    } else if (current < previous) {
      return 'text-danger';
    } else {
      return 'text-muted';
    }
  }
  
  // Find previous grade for the same assignment
  findPreviousGrade(grade: Grade): Grade | null {
    // Sort by date, oldest first
    const sortedGrades = [...this.grades]
      .filter(g => g.assignment.title === grade.assignment.title) // Same assignment
      .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
    
    if (sortedGrades.length <= 1) return null;
    
    const currentIndex = sortedGrades.findIndex(g => g.gradeId === grade.gradeId);
    return currentIndex > 0 ? sortedGrades[currentIndex - 1] : null;
  }
} 