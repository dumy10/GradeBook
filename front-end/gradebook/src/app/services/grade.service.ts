import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { catchError, throwError, map, of, switchMap } from 'rxjs';

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
  assignmentId: number;
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

// Interface for grade history entries
export interface GradeHistory {
  historyId: number;
  gradeId: number;
  previousPoints: number;
  newPoints: number;
  previousComment?: string;
  newComment?: string;
  changedBy: User;
  changeDate: string;
  changeType: 'Created' | 'Updated' | 'Deleted';
  student: User;
  assignment: Assignment;
}

// Replace with AuditLog interface to match what the server returns
export interface AuditLog {
  auditLogId: number;
  userId: number;
  entityType: string;
  entityId: number;
  action: string;
  details: string;
  ipAddress: string;
  createdAt: string;
  user?: User;
}

export interface CreateGradeRequest {
  assignmentId: number;
  studentId: number;
  points: number;
  comment: string;
}

export interface UpdateGradeRequest {
  assignmentId: number;
  studentId: number;
  points: number;
  comment: string;
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
        
        // For 404, return empty array instead of throwing an error
        if (error.status === 404) {
          console.log('No grades found for student. Returning empty array.');
          return [];
        }
        
        // Provide more context in the error
        if (error.status === 0) {
          error.message = 'Could not connect to the server. Please check that the API is running.';
        } else if (error.status === 401) {
          error.message = 'Authentication failed. Please log in again.';
        } else if (error.status === 403) {
          error.message = 'You do not have permission to access this resource.';
        }
        
        return throwError(() => error);
      })
    );
  }

  // Get grades for a specific class
  getGradesByClass(classId: number): Observable<Grade[]> {
    const headers = this.authService.getAuthHeaders();
    return this.http.get<Grade[]>(
      `${this.API_URL}/Grade/class/${classId}`,
      { headers }
    ).pipe(
      catchError(this.handleError)
    );
  }

  // Get grades for a specific assignment
  getGradesByAssignment(assignmentId: number): Observable<Grade[]> {
    const headers = this.authService.getAuthHeaders();
    return this.http.get<Grade[]>(
      `${this.API_URL}/Grade/assignment/${assignmentId}`,
      { headers }
    ).pipe(
      catchError(this.handleError)
    );
  }

  // Get a single grade by ID
  getGradeById(gradeId: number): Observable<Grade> {
    const headers = this.authService.getAuthHeaders();
    return this.http.get<Grade>(
      `${this.API_URL}/Grade/${gradeId}`,
      { headers }
    ).pipe(
      catchError(this.handleError)
    );
  }

  // Create a new grade - completely reworked to avoid DateTime issues
  createGrade(gradeRequest: CreateGradeRequest): Observable<Grade> {
    const headers = this.authService.getAuthHeaders();
    
    // Use only the required fields and avoid sending any DateTime-related data
    const minimalPayload = {
      assignmentId: Number(gradeRequest.assignmentId),
      studentId: Number(gradeRequest.studentId),
      points: Number(gradeRequest.points),
      comment: gradeRequest.comment || ''
    };
    
    console.log('Creating grade with payload:', minimalPayload);
    
    // Add retry logic to help with transient server issues
    return this.http.post<Grade>(
      `${this.API_URL}/Grade`,
      minimalPayload,
      { headers }
    ).pipe(
      catchError((error) => {
        console.error('Error in createGrade:', error);
        
        // Log detailed error information
        if (error.error) {
          const details = typeof error.error === 'string' 
            ? error.error 
            : JSON.stringify(error.error);
          console.error('Error details:', details);
          
          // Check for the specific DateTime error
          if (details.includes('DateTime with Kind=Unspecified') || 
              details.includes('timestamp with time zone')) {
            console.error('PostgreSQL DateTime error detected - using alternative approach');
            
            // Try a simpler request format that might avoid DateTime issues
            return this.createGradeAlternative(minimalPayload);
          }
        }
        
        // Create a helpful error message
        let errorMessage = 'Failed to create grade';
        if (error.error && error.error.message) {
          errorMessage = error.error.message;
        } else if (error.error && error.error.title) {
          errorMessage = error.error.title;
        } else if (error.status === 400) {
          errorMessage = 'Invalid grade data. Please check all fields and try again.';
        }
        
        return throwError(() => new Error(errorMessage));
      })
    );
  }
  
  // Alternative approach for grade creation that might avoid DateTime issues
  private createGradeAlternative(payload: any): Observable<Grade> {
    // Set headers with explicit content type
    const headers = this.authService.getAuthHeaders();
    headers.set('Content-Type', 'application/json');
    
    // Convert to JSON string first
    const stringPayload = JSON.stringify(payload);
    
    return this.http.post<Grade>(
      `${this.API_URL}/Grade`,
      stringPayload,
      { headers }
    ).pipe(
      catchError(error => {
        console.error('Alternative createGrade failed:', error);
        return throwError(() => new Error('Failed to create grade using alternative method. Please try again later.'));
      })
    );
  }

  // Create multiple grades at once
  createGradesBatch(gradeRequests: CreateGradeRequest[]): Observable<any> {
    const headers = this.authService.getAuthHeaders();
    return this.http.post<any>(
      `${this.API_URL}/Grade/batch`,
      gradeRequests,
      { headers }
    ).pipe(
      catchError(this.handleError)
    );
  }

  // Update an existing grade - completely reworked to avoid DateTime issues
  updateGrade(gradeId: number, gradeRequest: UpdateGradeRequest): Observable<boolean> {
    const headers = this.authService.getAuthHeaders();
    
    // Build a payload with only the exact fields needed for the grade update
    // PostgreSQL requires UTC for DateTime fields, so we avoid including any DateTime fields
    const payload = {
      assignmentId: Number(gradeRequest.assignmentId),
      studentId: Number(gradeRequest.studentId),
      points: Number(gradeRequest.points),
      comment: gradeRequest.comment || ''
    };
    
    console.log(`Attempting to update grade ${gradeId} with payload:`, payload);
    
    // Use a custom PUT request avoiding any DateTime fields
    return this.http.put<boolean>(
      `${this.API_URL}/Grade/${gradeId}`,
      payload,
      { 
        headers,
        // Use response type 'text' to be more tolerant of different response formats
        responseType: 'json' as 'json'
      }
    ).pipe(
      catchError((error) => {
        console.error('Error in updateGrade:', error);
        
        // Provide detailed error information
        let details = '';
        if (error.error) {
          details = typeof error.error === 'string' ? error.error : JSON.stringify(error.error);
        }
        
        console.error('Error details:', details);
        
        // Create a more helpful error message
        let errorMessage = 'Failed to update grade';
        if (error.error && error.error.message) {
          errorMessage = error.error.message;
        } else if (error.error && error.error.title) {
          errorMessage = error.error.title;
        } else if (error.status === 400) {
          errorMessage = 'Invalid grade data. Please check all fields and try again.';
        }
        
        return throwError(() => new Error(`${errorMessage}`));
      })
    );
  }

  // Delete a grade - improved with better error handling
  deleteGrade(gradeId: number): Observable<any> {
    const headers = this.authService.getAuthHeaders();
    
    console.log(`Attempting to delete grade with ID: ${gradeId}`);
    
    return this.http.delete<any>(
      `${this.API_URL}/Grade/${gradeId}`,
      { headers }
    ).pipe(
      catchError((error) => {
        console.error('Error in deleteGrade:', error);
        
        // Provide detailed error information
        let details = '';
        if (error.error) {
          details = typeof error.error === 'string' ? error.error : JSON.stringify(error.error);
        }
        
        console.error('Error details:', details);
        
        // Create a helpful error message
        let errorMessage = 'Failed to delete grade';
        if (error.error && error.error.message) {
          errorMessage = error.error.message;
        } else if (error.error && error.error.title) {
          errorMessage = error.error.title;
        } else if (error.status === 404) {
          errorMessage = 'Grade not found or already deleted.';
        }
        
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  // Get assignments for a specific class
  getAssignmentsForClass(classId: number): Observable<Assignment[]> {
    const headers = this.authService.getAuthHeaders();
    return this.http.get<Assignment[]>(
      `${this.API_URL}/Assignment/assignments/class/${classId}`,
      { headers }
    ).pipe(
      catchError(this.handleError)
    );
  }

  // Check if a student already has a grade for an assignment
  checkExistingGrade(studentId: number, assignmentId: number): Observable<boolean> {
    const headers = this.authService.getAuthHeaders();
    
    return this.http.get<Grade[]>(
      `${this.API_URL}/Grade/assignment/${assignmentId}/student/${studentId}`,
      { headers }
    ).pipe(
      // If we get grades, the student already has a grade for this assignment
      map(grades => grades && grades.length > 0),
      catchError(error => {
        // If we get a 404, it means no grades found, which is fine
        if (error.status === 404) {
          return of(false);
        }
        // Otherwise, something went wrong
        console.error('Error checking existing grade:', error);
        return throwError(() => new Error('Could not verify existing grades'));
      })
    );
  }

  // Get grade history for a specific student by reconstructing from audit logs
  getGradeHistoryForStudent(studentId: number): Observable<Grade[]> {
    const headers = this.authService.getAuthHeaders();
    return this.http.get<Grade[]>(
      `${this.API_URL}/Grade/student/${studentId}`,
      { headers }
    ).pipe(
      catchError(error => {
        // For 404, return empty array instead of throwing an error
        if (error.status === 404) {
          return of([]);
        }
        return this.handleError(error);
      })
    );
  }

  // Get grade history for a specific grade by getting audit logs for that entity
  getGradeHistoryForGrade(gradeId: number): Observable<Grade[]> {
    const headers = this.authService.getAuthHeaders();
    return this.http.get<Grade>(
      `${this.API_URL}/Grade/${gradeId}`,
      { headers }
    ).pipe(
      map(grade => [grade]), // Wrap in array for consistency
      catchError(error => {
        // For 404, return empty array instead of throwing an error
        if (error.status === 404) {
          return of([]);
        }
        return this.handleError(error);
      })
    );
  }

  // Get grade history for a specific class by reconstructing from audit logs
  getGradeHistoryForClass(classId: number): Observable<Grade[]> {
    const headers = this.authService.getAuthHeaders();
    return this.http.get<Grade[]>(
      `${this.API_URL}/Grade/class/${classId}`,
      { headers }
    ).pipe(
      catchError(error => {
        if (error.status === 404) {
          return of([]);
        }
        return this.handleError(error);
      })
    );
  }

  // Get all grade history logs (for admin users)
  getAllGradeHistory(): Observable<Grade[]> {
    const headers = this.authService.getAuthHeaders();
    return this.http.get<Grade[]>(
      `${this.API_URL}/Grade`,
      { headers }
    ).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    console.error('API error:', error);
    let errorMessage = 'An unknown error occurred';
    
    if (error.status === 0) {
      errorMessage = 'Could not connect to the server. Please check that the API is running.';
    } else if (error.status === 401) {
      errorMessage = 'Authentication failed. Please log in again.';
    } else if (error.status === 403) {
      errorMessage = 'You do not have permission to access this resource.';
    } else if (error.status === 404) {
      errorMessage = 'Resource not found.';
    } else if (error.error && error.error.message) {
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    return throwError(() => new Error(errorMessage));
  }
} 