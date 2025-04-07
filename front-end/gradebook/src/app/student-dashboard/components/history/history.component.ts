import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Grade, GradeService } from '../../../services/grade.service';
import { NgLetDirective } from '../../directives/nglet.directive';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule, NgLetDirective],
  templateUrl: './history.component.html',
  styleUrl: './history.component.scss',
})
export class HistoryComponent implements OnInit {
  grades: Grade[] = [];
  gradesLoading = false;
  gradesError = '';
  lastErrorDetails: any = null;
  userId = 0;

  constructor(private gradeService: GradeService) {}

  ngOnInit(): void {
    // Get user data from localStorage
    const userData = localStorage.getItem('currentUser');
    if (userData) {
      const parsedData = JSON.parse(userData);
      this.userId = parsedData.userId || 0;

      if (this.userId) {
        this.loadGrades();
      }
    }
  }

  loadGrades(): void {
    if (!this.userId) {
      this.gradesError = 'User ID not found. Please try logging in again.';
      return;
    }

    this.gradesLoading = true;
    this.gradesError = '';
    this.lastErrorDetails = null;

    this.gradeService.getStudentGrades(this.userId).subscribe({
      next: (grades) => {
        this.grades = grades || []; // Ensure we always have an array, even if null is returned
        this.gradesLoading = false; // Always clear loading state
      },
      error: (error) => {
        // This should only be hit for non-404 errors now
        this.grades = [];
        this.gradesLoading = false; // Always clear loading state
        this.gradesError =
          'Failed to load grade history. Please try again later.';

        // Store error details for debugging
        this.lastErrorDetails = {
          status: error.status,
          message: error.message,
          details: error.error,
        };
      },
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
      day: 'numeric',
    });
  }

  // Get the time part from a datetime string
  formatTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
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

  // Add a method to get grades organized by date
  getGradeHistory(): { date: string; grades: Grade[] }[] {
    // Clone and sort grades by createdAt date (newest first)
    const sortedGrades = [...this.grades].sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    );

    // Group by date
    const dateMap = new Map<string, Grade[]>();

    sortedGrades.forEach((grade) => {
      const dateKey = this.formatDate(grade.createdAt);
      if (!dateMap.has(dateKey)) {
        dateMap.set(dateKey, []);
      }
      dateMap.get(dateKey)?.push(grade);
    });

    return Array.from(dateMap).map(([date, grades]) => ({
      date,
      grades,
    }));
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
      .filter((g) => g.assignment.title === grade.assignment.title) // Same assignment
      .sort(
        (a, b) =>
          new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
      );

    if (sortedGrades.length <= 1) return null;

    const currentIndex = sortedGrades.findIndex(
      (g) => g.gradeId === grade.gradeId
    );
    return currentIndex > 0 ? sortedGrades[currentIndex - 1] : null;
  }
}
