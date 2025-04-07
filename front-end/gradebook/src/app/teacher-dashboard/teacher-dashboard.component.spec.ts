import { CommonModule } from '@angular/common';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import {
  ComponentFixture,
  TestBed,
  fakeAsync,
  tick,
} from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Class, ClassService, Student } from '../services/class.service';
import { Assignment, Grade, GradeService } from '../services/grade.service';
import { ClassManagementComponent } from './components/class-management/class-management.component';
import { GradeManagementComponent } from './components/grade-management/grade-management.component';
import { PasswordChangeComponent } from './components/password-change/password-change.component';
import { ProfileComponent } from './components/profile/profile.component';
import { TeacherDashboardComponent } from './teacher-dashboard.component';

describe('TeacherDashboardComponent', () => {
  let component: TeacherDashboardComponent;
  let fixture: ComponentFixture<TeacherDashboardComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let classServiceSpy: jasmine.SpyObj<ClassService>;
  let gradeServiceSpy: jasmine.SpyObj<GradeService>;
  let router: jasmine.SpyObj<Router>;

  const mockUserData = {
    username: 'testteacher',
    email: 'teacher@test.com',
    firstName: 'Test',
    lastName: 'Teacher',
    phone: '1234567890',
    address: '123 Test St',
    role: 'TEACHER',
  };

  const mockClass: Class = {
    classId: 1,
    courseId: 1,
    semester: 'Fall',
    academicYear: '2023',
    startDate: '2023-09-01',
    endDate: '2023-12-31',
    createdAt: '2023-01-01',
    updatedAt: '2023-01-01',
  };

  const mockStudents: Student[] = [
    {
      userId: 1,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      role: 'STUDENT',
    },
    {
      userId: 2,
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane@example.com',
      role: 'STUDENT',
    },
  ];

  const mockAssignmentType = {
    weight: 1,
    name: 'Quiz',
    description: 'In-class quiz',
  };

  const mockCourse = {
    name: 'Mathematics',
    description: 'Introduction to Mathematics',
  };

  const mockClassForAssignment = {
    semester: 'Fall',
    academicYear: '2023',
    course: mockCourse,
  };

  const mockAssignments: Assignment[] = [
    {
      assignmentId: 1,
      title: 'Quiz 1',
      description: 'First quiz',
      maxPoints: 100,
      minPoints: 0,
      dueDate: '2023-09-15',
      assignmentType: mockAssignmentType,
      class: mockClassForAssignment,
    },
    {
      assignmentId: 2,
      title: 'Homework 1',
      description: 'First homework',
      maxPoints: 50,
      minPoints: 0,
      dueDate: '2023-09-30',
      assignmentType: mockAssignmentType,
      class: mockClassForAssignment,
    },
  ];

  const mockGrader = {
    userId: 3,
    firstName: 'Test',
    lastName: 'Teacher',
    email: 'teacher@test.com',
    profilePicture: '',
  };

  const mockGrades: Grade[] = [
    {
      gradeId: 1,
      points: 85,
      comment: 'Good job',
      createdAt: '2023-09-16',
      updatedAt: '2023-09-16',
      grader: mockGrader,
      student: {
        userId: mockStudents[0].userId,
        firstName: mockStudents[0].firstName,
        lastName: mockStudents[0].lastName,
        email: mockStudents[0].email,
        profilePicture: '',
      },
      assignment: mockAssignments[0],
    },
    {
      gradeId: 2,
      points: 90,
      comment: 'Excellent work',
      createdAt: '2023-09-16',
      updatedAt: '2023-09-16',
      grader: mockGrader,
      student: {
        userId: mockStudents[1].userId,
        firstName: mockStudents[1].firstName,
        lastName: mockStudents[1].lastName,
        email: mockStudents[1].email,
        profilePicture: '',
      },
      assignment: mockAssignments[0],
    },
  ];

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', [
      'isLoggedIn',
      'getUserRole',
      'logout',
      'updateProfile',
      'changePassword',
    ]);
    classServiceSpy = jasmine.createSpyObj('ClassService', [
      'searchClasses',
      'getStudentsInClass',
      'searchStudents',
      'addStudentToClass',
      'removeStudentFromClass',
    ]);
    gradeServiceSpy = jasmine.createSpyObj('GradeService', [
      'getGradesByClass',
      'getStudentGrades',
      'getAssignmentsForClass',
      'createGrade',
      'updateGrade',
      'deleteGrade',
      'checkExistingGrade',
      'getGradesByAssignment',
      'createGradesBatch',
    ]);

    // Mock localStorage
    spyOn(localStorage, 'getItem').and.returnValue(
      JSON.stringify(mockUserData)
    );

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        FormsModule,
        TeacherDashboardComponent,
        ProfileComponent,
        PasswordChangeComponent,
        ClassManagementComponent,
        GradeManagementComponent,
      ],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: ClassService, useValue: classServiceSpy },
        { provide: GradeService, useValue: gradeServiceSpy },
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: Router,
          useValue: jasmine.createSpyObj('Router', ['navigate']),
        },
      ],
    }).compileComponents();

    // Get the router spy from TestBed
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Default auth service responses
    authServiceSpy.isLoggedIn.and.returnValue(true);
    authServiceSpy.getUserRole.and.returnValue('TEACHER');

    // Default class service responses
    classServiceSpy.searchClasses.and.returnValue(of([mockClass]));
    classServiceSpy.getStudentsInClass.and.returnValue(of(mockStudents));
    classServiceSpy.searchStudents.and.returnValue(of(mockStudents));

    // Default grade service responses
    gradeServiceSpy.getGradesByClass.and.returnValue(of(mockGrades));
    gradeServiceSpy.getAssignmentsForClass.and.returnValue(of(mockAssignments));
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGrades));
    gradeServiceSpy.checkExistingGrade.and.returnValue(of(false));
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TeacherDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should redirect if user is not logged in', () => {
    authServiceSpy.isLoggedIn.and.returnValue(false);
    component.ngOnInit();
    expect(router.navigate).toHaveBeenCalledWith(['/teacher/login']);
  });

  it('should redirect if user role is not teacher', () => {
    authServiceSpy.getUserRole.and.returnValue('STUDENT');
    component.ngOnInit();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should load user data from localStorage on init', () => {
    expect(component.userData.firstName).toBe(mockUserData.firstName);
    expect(component.userData.lastName).toBe(mockUserData.lastName);
    expect(component.userInfo.email).toBe(mockUserData.email);
  });

  it('should change active tab', () => {
    component.changeTab('password');
    expect(component.activeTab).toBe('password');

    component.changeTab('classes');
    expect(component.activeTab).toBe('classes');
    expect(classServiceSpy.searchClasses).toHaveBeenCalled();

    component.changeTab('grades');
    expect(component.activeTab).toBe('grades');
  });

  it('should logout user', () => {
    component.logout();
    expect(authServiceSpy.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  describe('Profile management', () => {
    it('should update profile successfully', fakeAsync(() => {
      authServiceSpy.updateProfile.and.returnValue(of({ success: true }));
      component.updateProfile(component.userData);
      tick();
      expect(component.successMessage).toContain(
        'Profile updated successfully'
      );
      expect(component.isLoading).toBeFalse();
    }));

    it('should handle profile update error', fakeAsync(() => {
      authServiceSpy.updateProfile.and.returnValue(
        throwError(() => ({ error: { message: 'Update failed' } }))
      );
      component.updateProfile(component.userData);
      tick();
      expect(component.errorMessage).toContain('Update failed');
      expect(component.isLoading).toBeFalse();
    }));
  });

  describe('Password management', () => {
    it('should validate password fields', () => {
      component.passwordData = {
        currentPassword: '',
        newPassword: 'new',
        confirmPassword: 'different',
      };

      component.changePassword(component.passwordData);
      expect(component.errorMessage).toContain(
        'All password fields are required'
      );

      component.passwordData.currentPassword = 'current';
      component.changePassword(component.passwordData);
      expect(component.errorMessage).toContain('New passwords do not match');

      component.passwordData.confirmPassword = 'new';
      component.changePassword(component.passwordData);
      expect(component.errorMessage).toContain(
        'Password must be at least 6 characters'
      );
    });

    it('should change password successfully', fakeAsync(() => {
      authServiceSpy.changePassword.and.returnValue(of({ success: true }));
      component.passwordData = {
        currentPassword: 'current',
        newPassword: 'newpassword',
        confirmPassword: 'newpassword',
      };

      component.changePassword(component.passwordData);
      tick();
      expect(component.successMessage).toContain(
        'Password changed successfully'
      );
      expect(component.passwordData.currentPassword).toBe('');
    }));

    it('should handle password change error', fakeAsync(() => {
      authServiceSpy.changePassword.and.returnValue(
        throwError(() => ({ error: { message: 'Wrong current password' } }))
      );
      component.passwordData = {
        currentPassword: 'wrong',
        newPassword: 'newpassword',
        confirmPassword: 'newpassword',
      };

      component.changePassword(component.passwordData);
      tick();
      expect(component.errorMessage).toContain('Wrong current password');
    }));
  });

  describe('Class management', () => {
    it('should search classes', fakeAsync(() => {
      component.searchClasses('Math');
      tick();
      expect(classServiceSpy.searchClasses).toHaveBeenCalledWith('Math');
      expect(component.classes.length).toBe(1);
    }));

    it('should search students', fakeAsync(() => {
      component.searchStudentTerm = 'John';
      component.searchStudents();
      tick();
      expect(classServiceSpy.searchStudents).toHaveBeenCalledWith('John');
      expect(component.searchResults.length).toBe(2);
    }));

    it('should select class and load related data', fakeAsync(() => {
      component.selectClass(mockClass);
      tick();
      expect(component.selectedClass).toBe(mockClass);
      expect(classServiceSpy.getStudentsInClass).toHaveBeenCalledWith(
        mockClass.classId
      );
      expect(gradeServiceSpy.getAssignmentsForClass).toHaveBeenCalledWith(
        mockClass.classId
      );
      expect(gradeServiceSpy.getGradesByClass).toHaveBeenCalledWith(
        mockClass.classId
      );
    }));

    it('should add student to class', fakeAsync(() => {
      const newStudent: Student = {
        userId: 3,
        firstName: 'New',
        lastName: 'Student',
        email: 'new@example.com',
        role: 'STUDENT',
      };

      // Set up initial state
      component.selectedClass = mockClass;
      component.studentsInClass = [...mockStudents]; // Make a copy

      // Configure the spy to return success
      classServiceSpy.addStudentToClass.and.returnValue(of({ success: true }));

      // Call the method being tested
      component.addStudentToClass(newStudent);
      tick();

      // Verify the service was called
      expect(classServiceSpy.addStudentToClass).toHaveBeenCalledWith(
        mockClass.classId,
        newStudent.userId
      );

      // Verify success message
      expect(component.successMessage).toContain('Added New Student');

      // Verify studentsInClass array was updated
      expect(component.studentsInClass.length).toBe(3); // Now should be 3 with the new student
      expect(component.studentsInClass).toContain(newStudent);
    }));

    it('should handle errors when adding student to class', fakeAsync(() => {
      const newStudent: Student = {
        userId: 3,
        firstName: 'New',
        lastName: 'Student',
        email: 'new@example.com',
        role: 'STUDENT',
      };
      component.selectedClass = mockClass;
      classServiceSpy.addStudentToClass.and.returnValue(
        throwError(() => ({ error: { message: 'Student already in class' } }))
      );

      component.addStudentToClass(newStudent);
      tick();

      expect(component.errorMessage).toContain('Student already in class');
    }));
  });

  describe('Grade management', () => {
    it('should load grades for class', fakeAsync(() => {
      component.loadGradesForClass(1);
      tick();
      expect(gradeServiceSpy.getGradesByClass).toHaveBeenCalledWith(1);
      expect(component.grades.length).toBe(2);
    }));

    it('should load grades for student', fakeAsync(() => {
      component.loadGradesForStudent(1);
      tick();
      expect(gradeServiceSpy.getStudentGrades).toHaveBeenCalledWith(1);
      expect(component.grades.length).toBe(2);
    }));

    it('should open new grade modal with correct student', () => {
      component.openNewGradeModal(mockStudents[0]);
      expect(component.selectedStudent).toBe(mockStudents[0]);
      expect(component.newGrade.studentId).toBe(mockStudents[0].userId);
      expect(component.showGradeModal).toBeTrue();
    });

    it('should create grade successfully', fakeAsync(() => {
      component.selectedClass = mockClass;
      component.newGrade = {
        assignmentId: 1,
        studentId: 1,
        points: 90,
        comment: 'Great work',
      };

      gradeServiceSpy.createGrade.and.returnValue(of(mockGrades[0]));
      component.createGrade();
      tick(600); // Account for the setTimeout delay in the component

      expect(gradeServiceSpy.createGrade).toHaveBeenCalled();
      expect(component.successMessage).toContain('Grade added successfully');
      expect(component.showGradeModal).toBeFalse();
    }));

    it('should validate grade points range', () => {
      component.assignments = [
        {
          assignmentId: 1,
          title: 'Test',
          description: '',
          maxPoints: 100,
          minPoints: 10,
          dueDate: '2023-09-30',
          assignmentType: mockAssignmentType,
          class: mockClassForAssignment,
        },
      ];

      component.newGrade = {
        assignmentId: 1,
        studentId: 1,
        points: 5,
        comment: '',
      };

      component.checkPointsRange();
      expect(component.errorMessage).toContain('Points must be at least 10');

      component.newGrade.points = 110;
      component.checkPointsRange();
      expect(component.errorMessage).toContain('Points cannot exceed 100');

      component.newGrade.points = 90;
      component.checkPointsRange();
      expect(component.errorMessage).not.toContain('Points');
    });

    it('should edit grade', () => {
      component.openEditGradeModal(mockGrades[0]);
      expect(component.selectedGrade).toBeTruthy();
      expect(component.editGradeForm.points).toBe(mockGrades[0].points);
      expect(component.showEditGradeModal).toBeTrue();
    });

    it('should update grade successfully', fakeAsync(() => {
      component.selectedClass = mockClass;
      component.selectedGrade = mockGrades[0];
      component.editGradeForm = {
        assignmentId: 1,
        studentId: 1,
        points: 95,
        comment: 'Updated comment',
      };

      gradeServiceSpy.updateGrade.and.returnValue(of(true));
      component.updateGrade();
      tick();

      expect(gradeServiceSpy.updateGrade).toHaveBeenCalledWith(
        mockGrades[0].gradeId,
        jasmine.any(Object)
      );
      expect(component.successMessage).toContain('Grade updated successfully');
      expect(component.showEditGradeModal).toBeFalse();
    }));

    it('should delete grade', fakeAsync(() => {
      component.selectedClass = mockClass;
      component.gradeToDelete = 1;

      gradeServiceSpy.deleteGrade.and.returnValue(of({ success: true }));
      component.deleteGrade();
      tick(600); // Account for the setTimeout delay in the component

      expect(gradeServiceSpy.deleteGrade).toHaveBeenCalledWith(1);
      expect(component.successMessage).toContain('Grade deleted successfully');
      expect(component.showDeleteConfirmation).toBeFalse();
    }));
  });

  describe('Bulk grade management', () => {
    it('should initialize bulk grade form with students', () => {
      component.selectClass(mockClass);
      fixture.detectChanges();

      expect(component.bulkGradeForm.students.length).toBe(mockStudents.length);
      expect(component.bulkGradeForm.students[0].userId).toBe(
        mockStudents[0].userId
      );
    });

    it('should toggle all students selection', () => {
      component.selectClass(mockClass);
      fixture.detectChanges();

      component.bulkGradeForm.selectAll = true;
      component.toggleAllStudents();

      expect(
        component.bulkGradeForm.students.every((s) => s.selected)
      ).toBeTrue();

      component.bulkGradeForm.selectAll = false;
      component.toggleAllStudents();

      expect(
        component.bulkGradeForm.students.every((s) => !s.selected)
      ).toBeTrue();
    });

    it('should submit bulk grades successfully', fakeAsync(() => {
      component.selectedClass = mockClass;
      component.bulkGradeForm = {
        assignmentId: 1,
        selectAll: true,
        students: [
          { userId: 1, selected: true, points: 80, comment: 'Good' },
          { userId: 2, selected: true, points: 85, comment: 'Very good' },
        ],
      };

      gradeServiceSpy.createGradesBatch.and.returnValue(
        of([mockGrades[0], mockGrades[1]])
      );
      component.submitBulkGrades();
      tick();

      expect(gradeServiceSpy.createGradesBatch).toHaveBeenCalled();
      expect(component.successMessage).toContain('Successfully added 2 grades');
      expect(component.showBulkGradeModal).toBeFalse();
    }));

    it('should validate bulk grade form before submission', () => {
      component.bulkGradeForm = {
        assignmentId: 0,
        selectAll: false,
        students: [],
      };

      component.submitBulkGrades();
      expect(component.errorMessage).toContain('Please select an assignment');

      component.bulkGradeForm.assignmentId = 1;
      component.submitBulkGrades();
      expect(component.errorMessage).toContain(
        'Please select at least one student'
      );
    });
  });

  describe('Quick bulk grading', () => {
    it('should prepare quick bulk grade data', fakeAsync(() => {
      component.gradeUploadForm.assignmentId = 1;
      component.studentsInClass = mockStudents;

      gradeServiceSpy.getGradesByAssignment.and.returnValue(
        of([
          { student: { userId: 1 } } as Grade, // Student 1 already has a grade
        ])
      );

      component.prepareQuickBulkGradeData();
      tick();

      expect(gradeServiceSpy.getGradesByAssignment).toHaveBeenCalledWith(1);
      // Only student 2 should be in the list (no existing grade)
      expect(component.quickGradeEntries.length).toBe(1);
      expect(component.quickGradeEntries[0].studentId).toBe(2);
    }));

    it('should toggle all quick grades', () => {
      component.quickGradeEntries = [
        {
          studentId: 1,
          studentName: 'John Doe',
          selected: false,
          points: 0,
          comment: '',
        },
        {
          studentId: 2,
          studentName: 'Jane Smith',
          selected: false,
          points: 0,
          comment: '',
        },
      ];

      component.quickGradeDefaults.selectAll = true;
      component.toggleAllQuickGrades();

      expect(component.quickGradeEntries.every((e) => e.selected)).toBeTrue();
    });

    it('should apply default values to selected entries', () => {
      component.quickGradeEntries = [
        {
          studentId: 1,
          studentName: 'John Doe',
          selected: true,
          points: 0,
          comment: '',
        },
        {
          studentId: 2,
          studentName: 'Jane Smith',
          selected: false,
          points: 0,
          comment: '',
        },
      ];

      component.quickGradeDefaults = {
        selectAll: false,
        points: 85,
        comment: 'Default comment',
      };

      component.assignments = mockAssignments;
      component.gradeUploadForm.assignmentId = 1;

      component.applyDefaultValues();

      expect(component.quickGradeEntries[0].points).toBe(85);
      expect(component.quickGradeEntries[0].comment).toBe('Default comment');
      expect(component.quickGradeEntries[1].points).toBe(0); // Not selected, should not change
    });

    it('should submit quick grades successfully', fakeAsync(() => {
      component.selectedClass = mockClass;
      component.gradeUploadForm.assignmentId = 1;
      component.quickGradeEntries = [
        {
          studentId: 1,
          studentName: 'John Doe',
          selected: true,
          points: 80,
          comment: 'Good',
        },
        {
          studentId: 2,
          studentName: 'Jane Smith',
          selected: true,
          points: 85,
          comment: 'Very good',
        },
      ];

      gradeServiceSpy.createGradesBatch.and.returnValue(
        of([mockGrades[0], mockGrades[1]])
      );
      component.submitQuickGrades();
      tick();

      expect(gradeServiceSpy.createGradesBatch).toHaveBeenCalled();
      expect(component.successMessage).toContain('Successfully added 2 grades');
      expect(component.showGradeUploadModal).toBeFalse();
    }));
  });
});
