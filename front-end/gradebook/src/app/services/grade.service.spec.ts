import { HttpHeaders, provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import {
  CreateGradeRequest,
  Grade,
  GradeService,
  UpdateGradeRequest,
} from './grade.service';

describe('GradeService', () => {
  let service: GradeService;
  let httpMock: HttpTestingController;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  const mockAuthHeaders = new HttpHeaders({
    'Content-Type': 'application/json',
    Authorization: 'Bearer fake-token',
  });

  const mockGrade: Grade = {
    gradeId: 1,
    comment: 'Good work',
    points: 85,
    createdAt: '2025-01-01T10:00:00Z',
    updatedAt: '2025-01-01T10:00:00Z',
    grader: {
      userId: 10,
      firstName: 'Teacher',
      lastName: 'Smith',
      email: 'teacher@example.com',
      profilePicture: '',
    },
    student: {
      userId: 20,
      firstName: 'Student',
      lastName: 'Jones',
      email: 'student@example.com',
      profilePicture: '',
    },
    assignment: {
      assignmentId: 30,
      title: 'Midterm Exam',
      description: 'Comprehensive exam covering chapters 1-5',
      maxPoints: 100,
      minPoints: 0,
      dueDate: '2025-03-15T23:59:59Z',
      assignmentType: {
        weight: 0.3,
        name: 'Exam',
        description: 'Formal examination',
      },
      class: {
        semester: 'Spring',
        academicYear: '2025',
        course: {
          name: 'Programming 101',
          description: 'Introduction to Programming',
        },
      },
    },
  };

  const mockGrades: Grade[] = [mockGrade];
  const apiUrl = 'https://localhost:7203/api';

  beforeEach(() => {
    const spy = jasmine.createSpyObj('AuthService', ['getAuthHeaders']);

    TestBed.configureTestingModule({
      providers: [
        GradeService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: spy },
      ],
    });

    service = TestBed.inject(GradeService);
    httpMock = TestBed.inject(HttpTestingController);
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;

    // Setup default behavior for auth service
    authServiceSpy.getAuthHeaders.and.returnValue(mockAuthHeaders);
  });

  afterEach(() => {
    httpMock.verify(); // Verify that no unmatched requests are outstanding
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getStudentGrades', () => {
    it('should return grades for a student', () => {
      const studentId = 20;

      service.getStudentGrades(studentId).subscribe((grades) => {
        expect(grades).toEqual(mockGrades);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/student/${studentId}`);
      expect(req.request.method).toBe('GET');
      expect(req.request.headers).toEqual(mockAuthHeaders);

      req.flush(mockGrades);
    });

    it('should return empty array for 404 responses', () => {
      const studentId = 999;

      service.getStudentGrades(studentId).subscribe((grades) => {
        expect(grades).toEqual([]);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/student/${studentId}`);
      req.flush(null, { status: 404, statusText: 'Not Found' });
    });

    it('should handle server errors with appropriate message', () => {
      const studentId = 20;

      service.getStudentGrades(studentId).subscribe({
        next: () => fail('should have failed with server error'),
        error: (error) => {
          expect(error.message).toContain('Could not connect to the server');
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/student/${studentId}`);
      req.flush('Server error', { status: 0, statusText: 'Unknown Error' });
    });
  });

  describe('getGradesByClass', () => {
    it('should return grades for a class', () => {
      const classId = 100;

      service.getGradesByClass(classId).subscribe((grades) => {
        expect(grades).toEqual(mockGrades);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/class/${classId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockGrades);
    });

    it('should handle error when fetching grades by class', () => {
      const classId = 100;

      service.getGradesByClass(classId).subscribe({
        next: () => fail('should have failed with error'),
        error: (error) => {
          expect(error.message).toBeTruthy();
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/class/${classId}`);
      req.flush('Error', { status: 500, statusText: 'Server Error' });
    });
  });

  describe('getGradesByAssignment', () => {
    it('should return grades for an assignment', () => {
      const assignmentId = 30;

      service.getGradesByAssignment(assignmentId).subscribe((grades) => {
        expect(grades).toEqual(mockGrades);
      });

      const req = httpMock.expectOne(
        `${apiUrl}/Grade/assignment/${assignmentId}`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockGrades);
    });
  });

  describe('getGradeById', () => {
    it('should return a grade by ID', () => {
      const gradeId = 1;

      service.getGradeById(gradeId).subscribe((grade) => {
        expect(grade).toEqual(mockGrade);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/${gradeId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockGrade);
    });
  });

  describe('createGrade', () => {
    it('should create a new grade', () => {
      const gradeRequest: CreateGradeRequest = {
        assignmentId: 30,
        studentId: 20,
        points: 85,
        comment: 'Good work',
      };

      const expectedPayload = {
        assignmentId: 30,
        studentId: 20,
        points: 85,
        comment: 'Good work',
      };

      service.createGrade(gradeRequest).subscribe((grade) => {
        expect(grade).toEqual(mockGrade);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(expectedPayload);
      req.flush(mockGrade);
    });

    it('should handle DateTime error and try alternative approach', () => {
      const gradeRequest: CreateGradeRequest = {
        assignmentId: 30,
        studentId: 20,
        points: 85,
        comment: 'Good work',
      };

      service.createGrade(gradeRequest).subscribe({
        next: (grade) => {
          expect(grade).toEqual(mockGrade);
        },
      });

      // First request fails with DateTime error
      const req1 = httpMock.expectOne(`${apiUrl}/Grade`);
      req1.flush(
        {
          error:
            'DateTime with Kind=Unspecified cannot be converted to timestamp with time zone',
        },
        { status: 400, statusText: 'Bad Request' }
      );

      // Alternative request succeeds
      const req2 = httpMock.expectOne(`${apiUrl}/Grade`);
      expect(req2.request.method).toBe('POST');
      req2.flush(mockGrade);
    });
  });

  describe('updateGrade', () => {
    it('should update an existing grade', () => {
      const gradeId = 1;
      const updateRequest: UpdateGradeRequest = {
        assignmentId: 30,
        studentId: 20,
        points: 90, // Updated points
        comment: 'Excellent work after revision',
      };

      service.updateGrade(gradeId, updateRequest).subscribe((result) => {
        expect(result).toBe(true);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/${gradeId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body.points).toBe(90);
      expect(req.request.body.comment).toBe('Excellent work after revision');
      req.flush(true);
    });
  });

  describe('deleteGrade', () => {
    it('should delete a grade', () => {
      const gradeId = 1;

      service.deleteGrade(gradeId).subscribe((result) => {
        expect(result).toBe(true);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/${gradeId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(true);
    });

    it('should handle 404 when deleting non-existent grade', () => {
      const gradeId = 999;

      service.deleteGrade(gradeId).subscribe({
        next: () => fail('should fail with 404'),
        error: (error) => {
          expect(error.message).toContain('Grade not found');
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/${gradeId}`);
      req.flush(
        { message: 'Grade not found' },
        { status: 404, statusText: 'Not Found' }
      );
    });
  });

  describe('createGradesBatch', () => {
    it('should create multiple grades at once', () => {
      const batchRequests: CreateGradeRequest[] = [
        {
          assignmentId: 30,
          studentId: 20,
          points: 85,
          comment: 'Good work',
        },
        {
          assignmentId: 30,
          studentId: 21,
          points: 90,
          comment: 'Excellent work',
        },
      ];

      service.createGradesBatch(batchRequests).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/batch`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.length).toBe(2);
      req.flush({ success: true });
    });
  });

  describe('checkExistingGrade', () => {
    it('should return true if student has grade for assignment', () => {
      const studentId = 20;
      const assignmentId = 30;

      service
        .checkExistingGrade(studentId, assignmentId)
        .subscribe((result) => {
          expect(result).toBeTrue();
        });

      const req = httpMock.expectOne(
        `${apiUrl}/Grade/assignment/${assignmentId}/student/${studentId}`
      );
      expect(req.request.method).toBe('GET');
      req.flush([mockGrade]);
    });

    it('should return false if student has no grade for assignment (404)', () => {
      const studentId = 20;
      const assignmentId = 30;

      service
        .checkExistingGrade(studentId, assignmentId)
        .subscribe((result) => {
          expect(result).toBeFalse();
        });

      const req = httpMock.expectOne(
        `${apiUrl}/Grade/assignment/${assignmentId}/student/${studentId}`
      );
      req.flush(null, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('getAssignmentsForClass', () => {
    it('should return assignments for a class', () => {
      const classId = 100;
      const mockAssignments = [mockGrade.assignment];

      service.getAssignmentsForClass(classId).subscribe((assignments) => {
        expect(assignments).toEqual(mockAssignments);
      });

      const req = httpMock.expectOne(
        `${apiUrl}/Assignment/assignments/class/${classId}`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockAssignments);
    });
  });

  describe('getGradeHistoryForStudent', () => {
    it('should return grade history for a student', () => {
      const studentId = 20;

      service.getGradeHistoryForStudent(studentId).subscribe((grades) => {
        expect(grades).toEqual(mockGrades);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/student/${studentId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockGrades);
    });

    it('should return empty array for 404 when getting student grade history', () => {
      const studentId = 999;

      service.getGradeHistoryForStudent(studentId).subscribe((grades) => {
        expect(grades).toEqual([]);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/student/${studentId}`);
      req.flush(null, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('getGradeHistoryForGrade', () => {
    it('should return grade history for a specific grade', () => {
      const gradeId = 1;

      service.getGradeHistoryForGrade(gradeId).subscribe((grades) => {
        expect(grades).toEqual([mockGrade]);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/${gradeId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockGrade);
    });
  });

  describe('getGradeHistoryForClass', () => {
    it('should return grade history for a class', () => {
      const classId = 100;

      service.getGradeHistoryForClass(classId).subscribe((grades) => {
        expect(grades).toEqual(mockGrades);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade/class/${classId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockGrades);
    });
  });

  describe('getAllGradeHistory', () => {
    it('should return all grade history', () => {
      service.getAllGradeHistory().subscribe((grades) => {
        expect(grades).toEqual(mockGrades);
      });

      const req = httpMock.expectOne(`${apiUrl}/Grade`);
      expect(req.request.method).toBe('GET');
      req.flush(mockGrades);
    });
  });
});
