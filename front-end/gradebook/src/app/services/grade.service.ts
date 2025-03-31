import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { catchError, throwError } from 'rxjs';

export interface User {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  profilePicture: string;
}

export interface AssignmentType {
  weight: number;
  name: string;
  description: string;
}

export interface Course {
  name: string;
  description: string;
}

export interface Class {
  semester: string;
  academicYear: string;
  course: Course;
}

export interface Assignment {
  title: string;
  description: string;
  maxPoints: number;
  minPoints: number;
  dueDate: string;
  assignmentType: AssignmentType;
  class: Class;
}

export interface Grade {
  gradeId: number;
  comment: string;
  points: number;
  createdAt: string;
  updatedAt: string;
  grader: User;
  student: User;
  assignment: Assignment;
}

@Injectable({
  providedIn: 'root'
})
export class GradeService {
  private readonly API_URL = 'https://localhost:7203/api';

  constructor(private http: HttpClient, private authService: AuthService) { }

  getStudentGrades(studentId: number): Observable<Grade[]> {
    const headers = this.authService.getAuthHeaders();
    console.log(`Fetching grades for student ID: ${studentId}`);
    console.log(`Request URL: ${this.API_URL}/Grade/student/${studentId}`);
    console.log('Headers:', headers);
    console.log('Authorization header:', headers.get('Authorization'));
    
    return this.http.get<Grade[]>(
      `${this.API_URL}/Grade/student/${studentId}`,
      { headers }
    ).pipe(
      catchError(error => {
        console.error('Error in GradeService.getStudentGrades:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.message);
        
        // Provide more context in the error
        if (error.status === 0) {
          error.message = 'Could not connect to the server. Please check that the API is running.';
        } else if (error.status === 401) {
          error.message = 'Authentication failed. Please log in again.';
        } else if (error.status === 403) {
          error.message = 'You do not have permission to access this resource.';
        } else if (error.status === 404) {
          error.message = 'No grades found for this student.';
        }
        
        return throwError(() => error);
      })
    );
  }
} 