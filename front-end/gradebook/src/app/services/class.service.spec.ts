import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Class, ClassService, Student } from './class.service';

describe('ClassService', () => {
  let service: ClassService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ClassService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ClassService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Verify that no unmatched requests are outstanding
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('searchStudents', () => {
    it('should return an array of students when given a search term', () => {
      const mockStudents: Student[] = [
        {
          userId: 1,
          email: 'john.doe@example.com',
          firstName: 'John',
          lastName: 'Doe',
          role: 'Student',
        },
        {
          userId: 2,
          email: 'jane.smith@example.com',
          firstName: 'Jane',
          lastName: 'Smith',
          role: 'Student',
        },
      ];
      const searchTerm = 'john';

      service.searchStudents(searchTerm).subscribe((students) => {
        expect(students).toEqual(mockStudents);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/api/Teacher/users/search/${searchTerm}`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockStudents);
    });
  });

  describe('searchClasses', () => {
    it('should return all classes when search term is empty', () => {
      const mockClasses: Class[] = [
        {
          classId: 1,
          courseId: 1,
          semester: 'Spring',
          academicYear: '2025',
          startDate: '2025-01-15',
          endDate: '2025-05-15',
          createdAt: '2024-12-01',
          updatedAt: '2024-12-01',
        },
      ];

      service.searchClasses('').subscribe((classes) => {
        expect(classes).toEqual(mockClasses);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/api/Teacher/classes`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockClasses);
    });

    it('should search for classes with the provided term', () => {
      const mockClasses: Class[] = [
        {
          classId: 1,
          courseId: 1,
          semester: 'Spring',
          academicYear: '2025',
          startDate: '2025-01-15',
          endDate: '2025-05-15',
          createdAt: '2024-12-01',
          updatedAt: '2024-12-01',
        },
      ];
      const searchTerm = 'Spring';

      service.searchClasses(searchTerm).subscribe((classes) => {
        expect(classes).toEqual(mockClasses);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/api/Teacher/classes/search/${searchTerm}`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockClasses);
    });
  });

  describe('getAllClasses', () => {
    it('should return all classes', () => {
      const mockClasses: Class[] = [
        {
          classId: 1,
          courseId: 1,
          semester: 'Spring',
          academicYear: '2025',
          startDate: '2025-01-15',
          endDate: '2025-05-15',
          createdAt: '2024-12-01',
          updatedAt: '2024-12-01',
        },
        {
          classId: 2,
          courseId: 2,
          semester: 'Fall',
          academicYear: '2025',
          startDate: '2025-08-15',
          endDate: '2025-12-15',
          createdAt: '2025-06-01',
          updatedAt: '2025-06-01',
        },
      ];

      service.getAllClasses().subscribe((classes) => {
        expect(classes).toEqual(mockClasses);
        expect(classes.length).toBe(2);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/api/Teacher/classes`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockClasses);
    });
  });

  describe('addStudentToClass', () => {
    it('should add a student to a class', () => {
      const classId = 1;
      const studentId = 101;
      const mockResponse = { success: true };

      service.addStudentToClass(classId, studentId).subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/api/Teacher/classes/${classId}/students`
      );
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ studentId: studentId });
      req.flush(mockResponse);
    });
  });

  describe('removeStudentFromClass', () => {
    it('should remove a student from a class', () => {
      const classId = 1;
      const studentId = 101;
      const mockResponse = { success: true };

      service
        .removeStudentFromClass(classId, studentId)
        .subscribe((response) => {
          expect(response).toEqual(mockResponse);
        });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/api/Teacher/classes/${classId}/students/${studentId}`
      );
      expect(req.request.method).toBe('DELETE');
      req.flush(mockResponse);
    });
  });

  describe('getStudentsInClass', () => {
    it('should get all students in a class', () => {
      const classId = 1;
      const mockStudents: Student[] = [
        {
          userId: 1,
          email: 'john.doe@example.com',
          firstName: 'John',
          lastName: 'Doe',
          role: 'Student',
        },
        {
          userId: 2,
          email: 'jane.smith@example.com',
          firstName: 'Jane',
          lastName: 'Smith',
          role: 'Student',
        },
      ];

      service.getStudentsInClass(classId).subscribe((students) => {
        expect(students).toEqual(mockStudents);
        expect(students.length).toBe(2);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/api/Teacher/classes/${classId}/students`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockStudents);
    });
  });
});
