import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Student {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface Class {
  classId: number;
  courseId: number;
  semester: string;
  academicYear: string;
  startDate: string;
  endDate: string;
  createdAt: string;
  updatedAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class ClassService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  searchStudents(searchTerm: string): Observable<Student[]> {
    return this.http.get<Student[]>(
      `${this.apiUrl}/api/Teacher/users/search/${searchTerm}`
    );
  }

  searchClasses(searchTerm: string): Observable<Class[]> {
    if (!searchTerm || searchTerm.trim() === '') {
      return this.getAllClasses();
    }
    return this.http.get<Class[]>(
      `${this.apiUrl}/api/Teacher/classes/search/${searchTerm}`
    );
  }

  getAllClasses(): Observable<Class[]> {
    return this.http.get<Class[]>(`${this.apiUrl}/api/Teacher/classes`);
  }

  addStudentToClass(classId: number, studentId: number): Observable<any> {
    const request = { studentId: studentId };
    return this.http.post(
      `${this.apiUrl}/api/Teacher/classes/${classId}/students`,
      request
    );
  }

  removeStudentFromClass(classId: number, studentId: number): Observable<any> {
    return this.http.delete(
      `${this.apiUrl}/api/Teacher/classes/${classId}/students/${studentId}`
    );
  }

  getStudentsInClass(classId: number): Observable<Student[]> {
    return this.http.get<Student[]>(
      `${this.apiUrl}/api/Teacher/classes/${classId}/students`
    );
  }
}
