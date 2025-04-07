import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Grade, GradeService } from '../../../services/grade.service';

@Component({
  selector: 'app-grades',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './grades.component.html',
  styleUrl: './grades.component.scss',
})
export class GradesComponent implements OnInit {
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
        this.grades = [];
        this.gradesLoading = false; // Always clear loading state
        this.gradesError = 'Failed to load grades. Please try again later.';

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

  // Group grades by course
  getGradesByCourse(): { courseName: string; grades: Grade[] }[] {
    const courseMap = new Map<string, Grade[]>();

    this.grades.forEach((grade) => {
      const courseName = grade.assignment.class.course.name;
      if (!courseMap.has(courseName)) {
        courseMap.set(courseName, []);
      }
      courseMap.get(courseName)?.push(grade);
    });

    return Array.from(courseMap).map(([courseName, grades]) => ({
      courseName,
      grades,
    }));
  }

  // Calculate overall average for all grades
  calculateOverallAverage(): number {
    if (this.grades.length === 0) return 0;

    const totalPoints = this.grades.reduce(
      (sum, grade) => sum + grade.points,
      0
    );
    const totalMaxPoints = this.grades.reduce(
      (sum, grade) => sum + grade.assignment.maxPoints,
      0
    );

    return totalMaxPoints > 0
      ? Math.round((totalPoints / totalMaxPoints) * 100)
      : 0;
  }

  // Calculate course average
  calculateCourseAverage(grades: Grade[]): number {
    if (grades.length === 0) return 0;

    const totalPoints = grades.reduce((sum, grade) => sum + grade.points, 0);
    const totalMaxPoints = grades.reduce(
      (sum, grade) => sum + grade.assignment.maxPoints,
      0
    );

    return totalMaxPoints > 0
      ? Math.round((totalPoints / totalMaxPoints) * 100)
      : 0;
  }
}
