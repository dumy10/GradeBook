import { CommonModule } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { Grade, GradeService } from '../../../services/grade.service';
import { NgLetDirective } from '../../directives/nglet.directive';
import { HistoryComponent } from './history.component';

describe('HistoryComponent', () => {
  let component: HistoryComponent;
  let fixture: ComponentFixture<HistoryComponent>;
  let gradeServiceSpy: jasmine.SpyObj<GradeService>;

  // Mock data
  const mockUserData = {
    userId: 1,
    username: 'testuser',
  };

  // Create multiple grades for the same assignment to test history functionality
  const mockGradeData: Grade[] = [
    {
      gradeId: 1,
      points: 75,
      comment: 'Initial grade',
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
      points: 85,
      comment: 'Updated after review',
      createdAt: '2025-03-05T14:30:00Z',
      updatedAt: '2025-03-05T14:30:00Z',
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
      gradeId: 3,
      points: 92,
      comment: 'Good programming skills',
      createdAt: '2025-03-15T10:00:00Z',
      updatedAt: '2025-03-15T10:00:00Z',
      grader: {
        userId: 3,
        firstName: 'Professor',
        lastName: 'Jones',
        email: 'jones@example.com',
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
      imports: [CommonModule, HistoryComponent, NgLetDirective],
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
    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load grades on init', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(gradeServiceSpy.getStudentGrades).toHaveBeenCalledWith(
      mockUserData.userId
    );
    expect(component.grades.length).toBe(3);
    expect(component.gradesLoading).toBe(false);
  });

  it('should handle empty grades array', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of([]));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.grades.length).toBe(0);
    // No error expected - this is a normal state
    expect(component.gradesError).toBe('');
    expect(component.gradesLoading).toBe(false);
  });

  it('should handle errors when loading grades', () => {
    const errorResponse = { status: 500, message: 'Server error' };
    gradeServiceSpy.getStudentGrades.and.returnValue(
      throwError(() => errorResponse)
    );

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.gradesLoading).toBe(false);
    expect(component.gradesError).toBe(
      'Failed to load grade history. Please try again later.'
    );
    expect(component.lastErrorDetails).not.toBeNull();
  });

  it('should calculate percentage correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.calculatePercentage(85, 100)).toBe(85);
    expect(component.calculatePercentage(45, 50)).toBe(90);
  });

  it('should format date correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Note: The exact format might depend on the user's locale
    const formattedDate = component.formatDate('2025-03-01T12:00:00Z');
    expect(formattedDate).toContain('2025');
    expect(formattedDate).toContain('Mar');
  });

  it('should format time correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    const formattedTime = component.formatTime('2025-03-01T14:30:00Z');
    // Will be either 2:30 PM or 14:30 depending on locale
    expect(formattedTime).toMatch('04:30 PM');
  });

  it('should calculate days since correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Create a date that's 3 days ago from now
    const now = new Date();
    const threeDaysAgo = new Date(now);
    threeDaysAgo.setDate(now.getDate() - 3);

    // Test the function
    expect(component.getDaysSince(threeDaysAgo.toISOString())).toBe(3);
  });

  it('should return relative date description correctly', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Create various test dates
    const now = new Date();

    // Today
    expect(component.getRelativeDate(now.toISOString())).toBe('Today');

    // Yesterday
    const yesterday = new Date(now);
    yesterday.setDate(now.getDate() - 1);
    expect(component.getRelativeDate(yesterday.toISOString())).toBe(
      'Yesterday'
    );

    // 5 days ago
    const fiveDaysAgo = new Date(now);
    fiveDaysAgo.setDate(now.getDate() - 5);
    expect(component.getRelativeDate(fiveDaysAgo.toISOString())).toBe(
      '5 days ago'
    );

    // 10 days ago (should return formatted date)
    const tenDaysAgo = new Date(now);
    tenDaysAgo.setDate(now.getDate() - 10);
    expect(component.getRelativeDate(tenDaysAgo.toISOString())).not.toContain(
      'days ago'
    );
  });

  it('should organize grades by date', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    const gradeHistory = component.getGradeHistory();

    // Should have 3 distinct dates
    expect(gradeHistory.length).toBe(3);

    // Dates should be in descending order (newest first)
    expect(new Date(gradeHistory[0].date).getTime()).toBeGreaterThan(
      new Date(gradeHistory[1].date).getTime()
    );
    expect(new Date(gradeHistory[1].date).getTime()).toBeGreaterThan(
      new Date(gradeHistory[2].date).getTime()
    );
  });

  it('should find previous grade for the same assignment', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Find previous grade for the second Math Quiz grade
    const secondMathQuizGrade = mockGradeData[1]; // This is the updated grade for Math Quiz 1
    const previousGrade = component.findPreviousGrade(secondMathQuizGrade);

    // Should find the first grade
    expect(previousGrade).not.toBeNull();
    expect(previousGrade?.gradeId).toBe(1);
    expect(previousGrade?.points).toBe(75);

    // For the first grade, there should be no previous grade
    const firstMathQuizGrade = mockGradeData[0];
    expect(component.findPreviousGrade(firstMathQuizGrade)).toBeNull();

    // For the CS assignment, there should be no previous grade
    const csAssignment = mockGradeData[2];
    expect(component.findPreviousGrade(csAssignment)).toBeNull();
  });

  it('should return correct change color class', () => {
    gradeServiceSpy.getStudentGrades.and.returnValue(of(mockGradeData));

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Improved grade
    expect(component.getChangeColorClass(85, 75)).toBe('text-success');

    // Decreased grade
    expect(component.getChangeColorClass(70, 75)).toBe('text-danger');

    // No change
    expect(component.getChangeColorClass(75, 75)).toBe('text-muted');

    // No previous grade
    expect(component.getChangeColorClass(75, 0)).toBe('');
  });

  it('should retry loading grades when retry button is clicked', () => {
    // First return an error, then success on retry
    const errorResponse = { status: 500, message: 'Server error' };
    gradeServiceSpy.getStudentGrades.and.returnValue(
      throwError(() => errorResponse)
    );

    fixture = TestBed.createComponent(HistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Verify error state
    expect(component.gradesError).toBe(
      'Failed to load grade history. Please try again later.'
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
    expect(component.grades.length).toBe(3);
  });
});
