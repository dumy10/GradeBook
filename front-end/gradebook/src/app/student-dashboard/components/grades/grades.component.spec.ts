import { CommonModule } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { Grade, GradeService } from '../../../services/grade.service';
import { GradesComponent } from './grades.component';
import { NgLetDirective } from '../../directives/nglet.directive';

describe('GradesComponent', () => {
  let component: GradesComponent;
  let fixture: ComponentFixture<GradesComponent>;
  let gradeServiceSpy: jasmine.SpyObj<GradeService>;

  // Mock data
  const mockUserData = {
    userId: 1,
    username: 'testuser',
  };

  const mockGradeData: Grade[] = [
    {
      gradeId: 1,
      points: 85,
      comment: 'Good work',
      createdAt: '2025-03-01T12:00:00Z',
      updatedAt: '2025-03-01T12:00:00Z',
      grader: {
        userId: 2,
        firstName: 'Professor',
        lastName: 'Smith',
        email: 'prof@example.com',
        profilePicture: '',
      },
      student: {
        userId: 1,
        firstName: 'Test',
        lastName: 'Student',
        email: 'student@example.com',
        profilePicture: '',
      },
      assignment: {
        assignmentId: 1,
        title: 'Math Quiz 1',
        description: 'First quiz of the semester',
        maxPoints: 100,
        minPoints: 0,
        dueDate: '2025-02-28T23:59:59Z',
        assignmentType: {
          name: 'Quiz',
          description: 'Short assessment',
          weight: 10,
        },
        class: {
          semester: 'Spring 2025',
          academicYear: '2024-2025',
          course: {
            name: 'Mathematics 101',
            description: 'Introduction to Mathematics',
          },
        },
      },
    },
    {
      gradeId: 2,
      points: 92,
      comment: 'Excellent',
      createdAt: '2025-03-15T14:30:00Z',
      updatedAt: '2025-03-15T14:30:00Z',
      grader: {
        userId: 2,
        firstName: 'Professor',
        lastName: 'Smith',
        email: 'prof@example.com',
        profilePicture: '',
      },
      student: {
        userId: 1,
        firstName: 'Test',
        lastName: 'Student',
        email: 'student@example.com',
        profilePicture: '',
      },
      assignment: {
        assignmentId: 2,
        title: 'Programming Assignment 1',
        description: 'Basic programming concepts',
        maxPoints: 100,
        minPoints: 0,
        dueDate: '2025-03-10T23:59:59Z',
        assignmentType: {
          name: 'Assignment',
          description: 'Practical work',
          weight: 20,
        },
        class: {
          semester: 'Spring 2025',
          academicYear: '2024-2025',
          course: {
            name: 'Computer Science 101',
            description: 'Introduction to Computer Science',
          },
        },
      },
    },
  ];

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('GradeService', ['getStudentGrades']);

    await TestBed.configureTestingModule({
      imports: [CommonModule, GradesComponent, NgLetDirective],
      providers: [{ provide: GradeService, useValue: spy }],
    }).compileComponents();

    // Mock localStorage
    spyOn(localStorage, 'getItem').and.returnValue(
      JSON.stringify(mockUserData)
    );

    gradeServiceSpy = TestBed.inject(
      GradeService
    ) as jasmine.SpyObj<GradeService>;
  });

  it('should create', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of([]));
    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load grades on init', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(gradeServiceSpy.getStudentGrades).toHaveBeenCalledWith(
      mockUserData.userId
    );
    expect(component.grades.length).toBe(2);
    expect(component.gradesLoading).toBe(false);
  });

  it('should handle empty grades array', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of([]));

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.grades.length).toBe(0);
    expect(component.gradesError).toBe('');
    expect(component.gradesLoading).toBe(false);
  });

  it('should handle errors when loading grades', () => {
    const errorResponse = { status: 500, message: 'Server error' };
    gradeServiceSpy.getStudentGrades.and.returnValue(
      throwError(() => errorResponse)
    );

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.gradesLoading).toBe(false);
    expect(component.gradesError).toBe(
      'Failed to load grades. Please try again later.'
    );
    expect(component.lastErrorDetails).not.toBeNull();
  });

  it('should calculate percentage correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.calculatePercentage(85, 100)).toBe(85);
    expect(component.calculatePercentage(45, 50)).toBe(90);
    expect(component.calculatePercentage(0, 100)).toBe(0);
  });

  it('should format date correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Note: The exact format might depend on the user's locale
    const formattedDate = component.formatDate('2025-03-01T12:00:00Z');
    expect(formattedDate).toContain('2025');
    expect(formattedDate).toContain('Mar');
  });

  it('should group grades by course correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    const groupedGrades = component.getGradesByCourse();
    expect(groupedGrades.length).toBe(2);

    // Find Mathematics course
    const mathCourse = groupedGrades.find(
      (g) => g.courseName === 'Mathematics 101'
    );
    expect(mathCourse).toBeTruthy();
    expect(mathCourse?.grades.length).toBe(1);

    // Find Computer Science course
    const csCourse = groupedGrades.find(
      (g) => g.courseName === 'Computer Science 101'
    );
    expect(csCourse).toBeTruthy();
    expect(csCourse?.grades.length).toBe(1);
  });

  it('should calculate overall average correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // (85 + 92) / (100 + 100) = 177/200 = 0.885 = 89%
    expect(component.calculateOverallAverage()).toBe(89);
  });

  it('should calculate course average correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Test with first course's grades
    const mathGrades = [mockGradeData[0]];
    expect(component.calculateCourseAverage(mathGrades)).toBe(85);
  });

  it('should display the correct number of courses and assignments', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // After the grades are loaded, check the summary section
    const summaryElements = fixture.debugElement.queryAll(
      By.css('.card-body .fs-4.fw-bold.text-success')
    );

    // First summary item should be the number of grades/assignments
    expect(summaryElements[0].nativeElement.textContent.trim()).toBe('2');

    // Second summary item should be the number of courses
    expect(summaryElements[1].nativeElement.textContent.trim()).toBe('2');

    // Third summary item should be the overall average
    expect(summaryElements[2].nativeElement.textContent.trim()).toBe('89%');
  });

  it('should retry loading grades when retry button is clicked', () => {
    // First return an error, then success on retry
    const errorResponse = { status: 500, message: 'Server error' };
    gradeServiceSpy.getStudentGrades.and.returnValue(
      throwError(() => errorResponse)
    );

    fixture = TestBed.createComponent(GradesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Verify error state
    expect(component.gradesError).toBe(
      'Failed to load grades. Please try again later.'
    );

    // Now setup the spy to return success on next call
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    // Find and click the retry button
    const retryButton = fixture.debugElement.query(
      By.css('button.btn-outline-danger')
    );
    retryButton.triggerEventHandler('click', null);

    fixture.detectChanges();

    // Verify grades loaded successfully
    expect(component.gradesLoading).toBe(false);
    expect(component.gradesError).toBe('');
    expect(component.grades.length).toBe(2);
  });
});
