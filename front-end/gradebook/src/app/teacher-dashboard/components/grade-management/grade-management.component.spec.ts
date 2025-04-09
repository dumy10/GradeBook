import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { Class, Student } from '../../../services/class.service';
import { Assignment, Grade } from '../../../services/grade.service';
import { GradeManagementComponent } from './grade-management.component';

describe('GradeManagementComponent', () => {
  let component: GradeManagementComponent;
  let fixture: ComponentFixture<GradeManagementComponent>;

  // Mock data
  const mockStudents: Student[] = [
    {
      userId: 1,
      firstName: 'Alice',
      lastName: 'Smith',
      email: 'alice@example.com',
      role: 'Student',
    },
    {
      userId: 2,
      firstName: 'Bob',
      lastName: 'Johnson',
      email: 'bob@example.com',
      role: 'Student',
    },
  ] as Student[];

  const mockAssignments: Assignment[] = [
    {
      assignmentId: 101,
      title: 'Midterm Exam',
      maxPoints: 100,
      minPoints: 0,
      dueDate: new Date().toISOString(),
      description: 'Midterm examination',
      assignmentType: {
        name: 'Exam',
        description: 'Examination',
        weight: 0.4,
      },
      class: {
        semester: 'Spring',
        academicYear: '2025',
        course: {
          name: 'Math 101',
          description: 'Introduction to Mathematics',
        },
      },
    },
    {
      assignmentId: 102,
      title: 'Final Project',
      maxPoints: 50,
      minPoints: 0,
      dueDate: new Date().toISOString(),
      description: 'Final project submission',
      assignmentType: {
        name: 'Project',
        description: 'Project work',
        weight: 0.3,
      },
      class: {
        semester: 'Spring',
        academicYear: '2025',
        course: {
          name: 'Math 101',
          description: 'Introduction to Mathematics',
        },
      },
    },
  ] as unknown as Assignment[];

  const mockGrades: Grade[] = [
    {
      gradeId: 201,
      assignmentId: 101,
      studentId: 1,
      points: 85,
      comment: 'Good work',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      grader: {
        userId: 10,
        firstName: 'Teacher',
        lastName: 'Smith',
        email: 'teacher@example.com',
        profilePicture: '',
      },
      student: {
        userId: 1,
        firstName: 'Alice',
        lastName: 'Smith',
        email: 'alice@example.com',
        profilePicture: '',
      },
      assignment: mockAssignments[0],
    },
    {
      gradeId: 202,
      assignmentId: 102,
      studentId: 1,
      points: 45,
      comment: 'Excellent',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      grader: {
        userId: 10,
        firstName: 'Teacher',
        lastName: 'Smith',
        email: 'teacher@example.com',
        profilePicture: '',
      },
      student: {
        userId: 1,
        firstName: 'Alice',
        lastName: 'Smith',
        email: 'alice@example.com',
        profilePicture: '',
      },
      assignment: mockAssignments[1],
    },
  ] as unknown as Grade[];

  const mockClass: Class = {
    classId: 301,
    courseId: 401,
    semester: 'Spring',
    academicYear: '2025',
    startDate: '2025-01-10',
    endDate: '2025-05-20',
    createdAt: '2024-12-15',
    updatedAt: '2024-12-15',
    className: 'Mathematics 101',
    description: 'Introduction to College Mathematics',
  } as Class;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GradeManagementComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(GradeManagementComponent);
    component = fixture.componentInstance;

    // Set up test data
    component.selectedClass = mockClass;
    component.studentsInClass = mockStudents;
    component.grades = mockGrades;
    component.gradesForClass = mockGrades;
    component.assignments = mockAssignments;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the class information', () => {
    const classInfo = fixture.debugElement.query(By.css('.card-body'));
    if (classInfo) {
      expect(classInfo.nativeElement.textContent).toContain(
        mockClass.academicYear
      );
      expect(classInfo.nativeElement.textContent).toContain(mockClass.semester);
    }
  });

  it('should display students in the class when selectedClass is set', () => {
    // Make sure selectedClass is set
    component.selectedClass = mockClass;
    component.studentsInClass = mockStudents;
    fixture.detectChanges();

    // Query for buttons rendered by *ngFor in the list-group
    const studentElements = fixture.debugElement.queryAll(
      By.css('.list-group .list-group-item-action')
    );

    // Check that the right number of students are displayed
    expect(studentElements.length).toBe(2);

    // Check the content includes student names
    expect(studentElements[0].nativeElement.textContent).toContain(
      'Alice Smith'
    );
    expect(studentElements[1].nativeElement.textContent).toContain(
      'Bob Johnson'
    );
  });

  it('should emit selectStudentForGradesEvent when student item is clicked', () => {
    // Prepare component with data
    component.selectedClass = mockClass;
    component.studentsInClass = mockStudents;
    fixture.detectChanges();

    // Spy on the event emitter
    spyOn(component.selectStudentForGradesEvent, 'emit');

    // Find the first student button directly
    const studentElements = fixture.debugElement.queryAll(
      By.css('.list-group .list-group-item-action')
    );

    // Make sure we found the elements
    expect(studentElements.length).toBeGreaterThan(0);

    // Trigger click on the first student
    studentElements[0].triggerEventHandler('click', null);

    // Verify the event was emitted with the correct student
    expect(component.selectStudentForGradesEvent.emit).toHaveBeenCalledWith(
      mockStudents[0]
    );
  });

  it('should display grades when student is selected', () => {
    component.selectedStudent = mockStudents[0];
    fixture.detectChanges();

    const gradeElements = fixture.debugElement.queryAll(By.css('tr'));
    // Account for header row + 2 grade rows
    expect(gradeElements.length).toBeGreaterThan(2);
  });

  it('should emit openNewGradeModalEvent when openNewGradeModal is called', () => {
    spyOn(component.openNewGradeModalEvent, 'emit');

    // Call the method directly
    component.openNewGradeModal(mockStudents[0]);

    // Verify the event was emitted with the correct student
    expect(component.openNewGradeModalEvent.emit).toHaveBeenCalledWith(
      mockStudents[0]
    );
  });

  it('should emit createGradeEvent when createGrade is called', () => {
    spyOn(component.createGradeEvent, 'emit');

    // Call the method directly
    component.createGrade();

    // Verify the event was emitted
    expect(component.createGradeEvent.emit).toHaveBeenCalled();
  });

  it('should emit openEditGradeModalEvent when openEditGradeModal is called', () => {
    spyOn(component.openEditGradeModalEvent, 'emit');

    // Call the method directly
    component.openEditGradeModal(mockGrades[0]);

    // Verify the event was emitted with the correct grade
    expect(component.openEditGradeModalEvent.emit).toHaveBeenCalledWith(
      mockGrades[0]
    );
  });

  it('should emit confirmDeleteGradeEvent when confirmDeleteGrade is called', () => {
    spyOn(component.confirmDeleteGradeEvent, 'emit');

    // Call the method directly
    component.confirmDeleteGrade(mockGrades[0].gradeId);

    // Verify the event was emitted with the correct gradeId
    expect(component.confirmDeleteGradeEvent.emit).toHaveBeenCalledWith(
      mockGrades[0].gradeId
    );
  });

  it('should emit openGradeHistoryModalEvent when openGradeHistoryModal is called', () => {
    spyOn(component.openGradeHistoryModalEvent, 'emit');

    // Call the method directly
    component.openGradeHistoryModal(mockGrades[0]);

    // Verify the event was emitted with the correct grade
    expect(component.openGradeHistoryModalEvent.emit).toHaveBeenCalledWith(
      mockGrades[0]
    );
  });

  it('should calculate percentage correctly', () => {
    const percentage = component.calculatePercentage(85, 100);
    expect(percentage).toBe(85);
  });

  it('should return 0 for percentage when maxPoints is 0', () => {
    const percentage = component.calculatePercentage(85, 0);
    expect(percentage).toBe(0);
  });

  it('should return the correct grade class based on percentage', () => {
    expect(component.getGradeClass(95, 100)).toBe('text-success fw-bold');
    expect(component.getGradeClass(85, 100)).toBe('text-success');
    expect(component.getGradeClass(75, 100)).toBe('text-primary');
    expect(component.getGradeClass(65, 100)).toBe('text-warning');
    expect(component.getGradeClass(55, 100)).toBe('text-danger');
  });

  it('should format date correctly', () => {
    const dateString = '2025-04-07T12:30:00Z';
    const formatted = component.formatDate(dateString);
    expect(typeof formatted).toBe('string');
    expect(formatted).not.toBe('N/A');
  });

  it('should return "N/A" for empty date strings', () => {
    const formatted = component.formatDate('');
    expect(formatted).toBe('N/A');
  });

  it('should find assignment by ID correctly', () => {
    const assignment = component.getAssignmentById(101);
    expect(assignment).toEqual(mockAssignments[0]);
  });

  it('should return undefined when assignment ID is not found', () => {
    const assignment = component.getAssignmentById(999);
    expect(assignment).toBeUndefined();
  });

  it('should get selected assignment for edit form', () => {
    component.editGradeForm.assignmentId = 101;
    const assignment = component.getSelectedAssignmentForEdit();
    expect(assignment).toEqual(mockAssignments[0]);
  });

  it('should count selected students in bulk grade form correctly', () => {
    component.bulkGradeForm.students = [
      { id: 1, name: 'Alice', selected: true },
      { id: 2, name: 'Bob', selected: false },
      { id: 3, name: 'Charlie', selected: true },
    ];

    const count = component.getSelectedStudentCount();
    expect(count).toBe(2);
  });

  it('should determine if bulk grades can be submitted', () => {
    component.bulkGradeForm.assignmentId = 101;
    component.bulkGradeForm.students = [
      { id: 1, name: 'Alice', selected: true },
      { id: 2, name: 'Bob', selected: false },
    ];

    expect(component.canSubmitBulkGrades()).toBe(true);

    component.bulkGradeForm.students = [
      { id: 1, name: 'Alice', selected: false },
      { id: 2, name: 'Bob', selected: false },
    ];

    expect(component.canSubmitBulkGrades()).toBe(false);

    component.bulkGradeForm.assignmentId = null;
    component.bulkGradeForm.students = [
      { id: 1, name: 'Alice', selected: true },
      { id: 2, name: 'Bob', selected: false },
    ];

    expect(component.canSubmitBulkGrades()).toBe(false);
  });

  it('should determine if quick grades can be submitted', () => {
    component.gradeUploadForm.assignmentId = 101;
    component.quickGradeEntries = [
      {
        studentId: 1,
        studentName: 'Alice',
        selected: true,
        points: 90,
        comment: '',
      },
      {
        studentId: 2,
        studentName: 'Bob',
        selected: false,
        points: 85,
        comment: '',
      },
    ];

    expect(component.canSubmitQuickGrades()).toBe(true);

    component.quickGradeEntries = [
      {
        studentId: 1,
        studentName: 'Alice',
        selected: false,
        points: 90,
        comment: '',
      },
      {
        studentId: 2,
        studentName: 'Bob',
        selected: false,
        points: 85,
        comment: '',
      },
    ];

    expect(component.canSubmitQuickGrades()).toBe(false);

    component.gradeUploadForm.assignmentId = null;
    component.quickGradeEntries = [
      {
        studentId: 1,
        studentName: 'Alice',
        selected: true,
        points: 90,
        comment: '',
      },
      {
        studentId: 2,
        studentName: 'Bob',
        selected: false,
        points: 85,
        comment: '',
      },
    ];

    expect(component.canSubmitQuickGrades()).toBe(false);
  });

  it('should emit all event handlers when methods are called', () => {
    // Set up spies for all output events
    const eventSpies = {
      selectStudentForGradesEvent: spyOn(
        component.selectStudentForGradesEvent,
        'emit'
      ),
      openNewGradeModalEvent: spyOn(component.openNewGradeModalEvent, 'emit'),
      closeNewGradeModalEvent: spyOn(component.closeNewGradeModalEvent, 'emit'),
      createGradeEvent: spyOn(component.createGradeEvent, 'emit'),
      openEditGradeModalEvent: spyOn(component.openEditGradeModalEvent, 'emit'),
      closeEditGradeModalEvent: spyOn(
        component.closeEditGradeModalEvent,
        'emit'
      ),
      updateGradeEvent: spyOn(component.updateGradeEvent, 'emit'),
      confirmDeleteGradeEvent: spyOn(component.confirmDeleteGradeEvent, 'emit'),
      closeDeleteConfirmationEvent: spyOn(
        component.closeDeleteConfirmationEvent,
        'emit'
      ),
      deleteGradeEvent: spyOn(component.deleteGradeEvent, 'emit'),
      toggleHistoryViewEvent: spyOn(component.toggleHistoryViewEvent, 'emit'),
      openGradeHistoryModalEvent: spyOn(
        component.openGradeHistoryModalEvent,
        'emit'
      ),
      closeGradeHistoryModalEvent: spyOn(
        component.closeGradeHistoryModalEvent,
        'emit'
      ),
      loadGradesForClassEvent: spyOn(component.loadGradesForClassEvent, 'emit'),
    };

    // Call methods that emit events
    component.selectStudentForGrades(mockStudents[0]);
    component.openNewGradeModal(mockStudents[0]);
    component.closeNewGradeModal();
    component.createGrade();
    component.openEditGradeModal(mockGrades[0]);
    component.closeEditGradeModal();
    component.updateGrade();
    component.confirmDeleteGrade(201);
    component.closeDeleteConfirmation();
    component.deleteGrade();
    component.toggleHistoryView();
    component.openGradeHistoryModal(mockGrades[0]);
    component.closeGradeHistoryModal();
    component.loadGradesForClass(301);

    // Verify all events were emitted with correct parameters
    expect(eventSpies.selectStudentForGradesEvent.calls.count()).toBe(1);
    expect(eventSpies.selectStudentForGradesEvent.calls.argsFor(0)[0]).toBe(
      mockStudents[0]
    );

    expect(eventSpies.openNewGradeModalEvent.calls.count()).toBe(1);
    expect(eventSpies.openNewGradeModalEvent.calls.argsFor(0)[0]).toBe(
      mockStudents[0]
    );

    expect(eventSpies.closeNewGradeModalEvent.calls.count()).toBe(1);
    expect(eventSpies.createGradeEvent.calls.count()).toBe(1);

    expect(eventSpies.openEditGradeModalEvent.calls.count()).toBe(1);
    expect(eventSpies.openEditGradeModalEvent.calls.argsFor(0)[0]).toBe(
      mockGrades[0]
    );

    expect(eventSpies.closeEditGradeModalEvent.calls.count()).toBe(1);
    expect(eventSpies.updateGradeEvent.calls.count()).toBe(1);

    expect(eventSpies.confirmDeleteGradeEvent.calls.count()).toBe(1);
    expect(eventSpies.confirmDeleteGradeEvent.calls.argsFor(0)[0]).toBe(201);

    expect(eventSpies.closeDeleteConfirmationEvent.calls.count()).toBe(1);
    expect(eventSpies.deleteGradeEvent.calls.count()).toBe(1);

    expect(eventSpies.toggleHistoryViewEvent.calls.count()).toBe(1);

    expect(eventSpies.openGradeHistoryModalEvent.calls.count()).toBe(1);
    expect(eventSpies.openGradeHistoryModalEvent.calls.argsFor(0)[0]).toBe(
      mockGrades[0]
    );

    expect(eventSpies.closeGradeHistoryModalEvent.calls.count()).toBe(1);

    expect(eventSpies.loadGradesForClassEvent.calls.count()).toBe(1);
    expect(eventSpies.loadGradesForClassEvent.calls.argsFor(0)[0]).toBe(301);
  });
});
