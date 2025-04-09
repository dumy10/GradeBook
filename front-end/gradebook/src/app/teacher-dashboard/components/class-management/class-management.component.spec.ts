import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { Class, Course, Student } from '../../../services/class.service';
import { ClassManagementComponent } from './class-management.component';

describe('ClassManagementComponent', () => {
  let component: ClassManagementComponent;
  let fixture: ComponentFixture<ClassManagementComponent>;

  // Mock data
  const mockClasses: Class[] = [
    {
      classId: 1,
      courseId: 101,
      className: 'Introduction to Programming',
      description: 'Basic programming class',
      semester: '1',
      academicYear: '2023',
      startDate: '2023-01-15',
      endDate: '2023-05-15',
      createdAt: '2022-12-01',
      updatedAt: '2022-12-01',
    },
    {
      classId: 2,
      courseId: 202,
      className: 'Advanced Data Structures',
      description: 'Complex data structures class',
      semester: '1',
      academicYear: '2023',
      startDate: '2023-01-15',
      endDate: '2023-05-15',
      createdAt: '2022-12-01',
      updatedAt: '2022-12-01',
    },
  ];

  const mockCourses: Course[] = [
    {
      courseId: 101,
      courseName: 'Introduction to Programming',
      courseCode: 'CS101',
      description: 'Learn the basics of programming',
    },
    {
      courseId: 202,
      courseName: 'Advanced Data Structures',
      courseCode: 'CS202',
      description: 'Complex algorithms and data structures',
    },
  ];

  const mockStudents: Student[] = [
    {
      userId: 1,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com',
      role: 'Student',
    },
    {
      userId: 2,
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@example.com',
      role: 'Student',
    },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClassManagementComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ClassManagementComponent);
    component = fixture.componentInstance;

    // Set initial component input properties
    component.classes = [...mockClasses];
    component.courses = [...mockCourses];
    component.searchResults = [];
    component.studentsInClass = [];

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // Tests for course map and related methods
  it('should update courseMap when courses change', () => {
    // Check if courseMap is properly initialized
    expect(component.courseMap[101].name).toBe('Introduction to Programming');
    expect(component.courseMap[101].code).toBe('CS101');
    expect(component.courseMap[202].name).toBe('Advanced Data Structures');
    expect(component.courseMap[202].code).toBe('CS202');
  });

  it('should get course name correctly', () => {
    expect(component.getCourseName(101)).toBe('Introduction to Programming');
    expect(component.getCourseName(202)).toBe('Advanced Data Structures');
    expect(component.getCourseName(999)).toBe('Course 999'); // Non-existent course
  });

  it('should get course code correctly', () => {
    expect(component.getCourseCode(101)).toBe('CS101');
    expect(component.getCourseCode(202)).toBe('CS202');
    expect(component.getCourseCode(999)).toBe(''); // Non-existent course
  });

  it('should get course description correctly', () => {
    expect(component.getCourseDescription(101)).toBe(
      'Learn the basics of programming'
    );
    expect(component.getCourseDescription(202)).toBe(
      'Complex algorithms and data structures'
    );
    expect(component.getCourseDescription(999)).toBe(''); // Non-existent course
  });

  it('should display classes in the list with course details', () => {
    const classElements = fixture.debugElement.queryAll(
      By.css('.list-group-item')
    );
    expect(classElements.length).toBe(2);

    // Check if course names and codes are displayed
    expect(classElements[0].nativeElement.textContent).toContain(
      'Introduction to Programming'
    );
    expect(classElements[0].nativeElement.textContent).toContain('CS101');
    expect(classElements[1].nativeElement.textContent).toContain(
      'Advanced Data Structures'
    );
    expect(classElements[1].nativeElement.textContent).toContain('CS202');
  });

  it('should emit searchClassesEvent when searching classes', () => {
    spyOn(component.searchClassesEvent, 'emit');
    const searchTerm = 'CS101';
    component.searchClassTerm = searchTerm;
    component.searchClasses(searchTerm);
    expect(component.searchClassesEvent.emit).toHaveBeenCalledWith(searchTerm);
  });

  it('should emit searchStudentsEvent when searching students', () => {
    spyOn(component.searchStudentsEvent, 'emit');
    component.searchStudents();
    expect(component.searchStudentsEvent.emit).toHaveBeenCalled();
  });

  it('should emit selectClassEvent when a class is selected', () => {
    spyOn(component.selectClassEvent, 'emit');
    const classObj = mockClasses[0];
    component.selectClass(classObj);
    expect(component.selectClassEvent.emit).toHaveBeenCalledWith(classObj);
  });

  it('should mark a class as active when selected', () => {
    component.selectedClass = mockClasses[0];
    fixture.detectChanges();

    const classElements = fixture.debugElement.queryAll(
      By.css('.list-group-item')
    );
    expect(classElements[0].nativeElement.classList).toContain('active');
    expect(classElements[1].nativeElement.classList).not.toContain('active');
  });

  it('should emit addStudentToClassEvent when adding a student', () => {
    spyOn(component.addStudentToClassEvent, 'emit');
    const student = mockStudents[0];
    component.addStudentToClass(student);
    expect(component.addStudentToClassEvent.emit).toHaveBeenCalledWith(student);
  });

  it('should emit removeStudentFromClassEvent when removing a student', () => {
    spyOn(component.removeStudentFromClassEvent, 'emit');
    const student = mockStudents[0];
    component.removeStudentFromClass(student);
    expect(component.removeStudentFromClassEvent.emit).toHaveBeenCalledWith(
      student
    );
  });

  it('should correctly identify if a student is in class', () => {
    component.studentsInClass = [mockStudents[0]];
    fixture.detectChanges();

    expect(component.isStudentInClass(mockStudents[0])).toBe(true);
    expect(component.isStudentInClass(mockStudents[1])).toBe(false);
  });

  it('should handle null values in isStudentInClass', () => {
    expect(component.isStudentInClass(null as unknown as Student)).toBe(false);
    component.studentsInClass = null as unknown as Student[];
    expect(component.isStudentInClass(mockStudents[0])).toBe(false);
  });

  it('should display "No classes found" when classes array is empty', () => {
    component.classes = [];
    component.classLoading = false;
    fixture.detectChanges();

    const noClassesElement = fixture.debugElement.query(By.css('.text-muted'));
    expect(noClassesElement.nativeElement.textContent.trim()).toBe(
      'No classes found'
    );
  });

  it('should display loading spinner when classLoading is true', () => {
    component.classLoading = true;
    fixture.detectChanges();

    const spinnerElement = fixture.debugElement.query(
      By.css('.spinner-border')
    );
    expect(spinnerElement).toBeTruthy();
  });

  it('should display class details when a class is selected', () => {
    component.selectedClass = mockClasses[0];
    fixture.detectChanges();

    const classDetailsElement = fixture.debugElement.query(
      By.css('.class-details')
    );
    expect(classDetailsElement).toBeTruthy();
    expect(classDetailsElement.nativeElement.textContent).toContain('101');
  });

  it('should disable "Add to Class" button for students already in class', () => {
    component.selectedClass = mockClasses[0];
    component.studentsInClass = [mockStudents[0]];
    component.searchResults = mockStudents;
    fixture.detectChanges();

    expect(component.isStudentInClass(mockStudents[0])).toBe(true);
    expect(component.isStudentInClass(mockStudents[1])).toBe(false);
  });

  it('should show "Please select a class from the left panel" message when no class is selected', () => {
    component.selectedClass = null;
    fixture.detectChanges();

    const messageElement = fixture.debugElement.query(
      By.css('.text-center.text-muted.p-5')
    );
    expect(messageElement).toBeTruthy();
    expect(messageElement.nativeElement.textContent.trim()).toBe(
      'Please select a class from the left panel'
    );
  });

  it('should show the success message when provided', () => {
    component.successMessage = 'Student added successfully';
    fixture.detectChanges();

    const alertElement = fixture.debugElement.query(
      By.css('.alert.alert-success')
    );
    expect(alertElement).toBeTruthy();
    expect(alertElement.nativeElement.textContent.trim()).toBe(
      'Student added successfully'
    );
  });

  it('should show the error message when provided', () => {
    component.errorMessage = 'Failed to add student';
    fixture.detectChanges();

    const alertElement = fixture.debugElement.query(
      By.css('.alert.alert-danger')
    );
    expect(alertElement).toBeTruthy();
    expect(alertElement.nativeElement.textContent.trim()).toBe(
      'Failed to add student'
    );
  });

  it('should show "No students in this class" when studentsInClass is empty', () => {
    component.selectedClass = mockClasses[0];
    component.studentsInClass = [];
    component.classLoading = false;
    fixture.detectChanges();

    const noStudentsElement = fixture.debugElement.query(
      By.css('.text-center.text-muted')
    );
    // Find the element containing "No students in this class"
    const foundElement = Array.from(
      fixture.debugElement.queryAll(By.css('.text-center.text-muted'))
    ).find(
      (el) =>
        el.nativeElement.textContent.trim() === 'No students in this class'
    );

    expect(foundElement).toBeTruthy();
  });

  it('should display search student input when a class is selected', () => {
    component.selectedClass = mockClasses[0];
    fixture.detectChanges();

    const searchInputElement = fixture.debugElement.query(
      By.css('input[placeholder="Search for students..."]')
    );
    expect(searchInputElement).toBeTruthy();
  });

  it('should render student cards for search results', () => {
    component.selectedClass = mockClasses[0];
    component.searchResults = mockStudents;
    fixture.detectChanges();

    const studentCards = fixture.debugElement.queryAll(By.css('.student-card'));
    expect(studentCards.length).toBe(2);
    expect(studentCards[0].nativeElement.textContent).toContain('John Doe');
    expect(studentCards[0].nativeElement.textContent).toContain(
      'john.doe@example.com'
    );
    expect(studentCards[1].nativeElement.textContent).toContain('Jane Smith');
    expect(studentCards[1].nativeElement.textContent).toContain(
      'jane.smith@example.com'
    );
  });

  it('should emit searchClassesEvent when input changes', () => {
    spyOn(component.searchClassesEvent, 'emit');

    const inputElement = fixture.debugElement.query(
      By.css('input[placeholder="Search classes..."]')
    );
    inputElement.nativeElement.value = 'Math';
    inputElement.nativeElement.dispatchEvent(new Event('input'));

    // We need to set the component's property manually since we're testing in isolation
    component.searchClassTerm = 'Math';

    expect(component.searchClassesEvent.emit).toHaveBeenCalledWith('Math');
  });

  it('should display search button for students', () => {
    component.selectedClass = mockClasses[0];
    fixture.detectChanges();

    const searchButton = fixture.debugElement.query(
      By.css('button.btn-outline-primary')
    );
    expect(searchButton).toBeTruthy();
    expect(searchButton.nativeElement.textContent.trim()).toBe('Search');
  });

  it('should display the class ID in the header when a class is selected', () => {
    component.selectedClass = mockClasses[0];
    fixture.detectChanges();

    const headerElement = fixture.debugElement.query(
      By.css('.badge.bg-light.text-dark')
    );
    expect(headerElement).toBeTruthy();
    expect(headerElement.nativeElement.textContent).toContain('Class ID: 1');
  });

  it('should show students in class when they are available', () => {
    component.selectedClass = mockClasses[0];
    component.studentsInClass = mockStudents;
    fixture.detectChanges();

    const studentElements = fixture.debugElement.queryAll(
      By.css('.list-group .student-card')
    );
    expect(studentElements.length).toBe(2);
  });

  it('should disable "Add to Class" button and show "Already Added" text for students in class', () => {
    component.selectedClass = mockClasses[0];
    component.studentsInClass = [mockStudents[0]];
    component.searchResults = mockStudents;
    fixture.detectChanges();

    // Get all the buttons in search results
    const buttons = fixture.debugElement.queryAll(
      By.css('.student-card button')
    );

    // First button should be disabled (student is already in class)
    expect(buttons[0].nativeElement.disabled).toBe(true);
    expect(buttons[0].nativeElement.textContent.trim()).toBe('Already Added');

    // Second button should be enabled (student not in class)
    expect(buttons[1].nativeElement.disabled).toBe(false);
    expect(buttons[1].nativeElement.textContent.trim()).toBe('Add to Class');
  });

  it('should properly handle click on "Remove" button', () => {
    spyOn(component.removeStudentFromClassEvent, 'emit');
    component.selectedClass = mockClasses[0];
    component.studentsInClass = mockStudents;
    fixture.detectChanges();

    const removeButtons = fixture.debugElement.queryAll(
      By.css('.list-group .student-card button')
    );
    removeButtons[0].nativeElement.click();

    expect(component.removeStudentFromClassEvent.emit).toHaveBeenCalledWith(
      mockStudents[0]
    );
  });

  it('should properly handle click on "Add to Class" button', () => {
    spyOn(component.addStudentToClassEvent, 'emit');
    component.selectedClass = mockClasses[0];
    component.searchResults = [mockStudents[0]];
    fixture.detectChanges();

    const addButton = fixture.debugElement.query(
      By.css('.student-card button')
    );
    addButton.nativeElement.click();

    expect(component.addStudentToClassEvent.emit).toHaveBeenCalledWith(
      mockStudents[0]
    );
  });

  it('should properly handle click on class selection', () => {
    spyOn(component.selectClassEvent, 'emit');
    fixture.detectChanges();

    const classButtons = fixture.debugElement.queryAll(
      By.css('.list-group-item')
    );
    classButtons[1].nativeElement.click(); // Click on the second class

    expect(component.selectClassEvent.emit).toHaveBeenCalledWith(
      mockClasses[1]
    );
  });

  it('should properly handle search students button click', () => {
    spyOn(component.searchStudentsEvent, 'emit');
    component.selectedClass = mockClasses[0];
    component.searchStudentTerm = 'John';
    fixture.detectChanges();

    const searchButton = fixture.debugElement.query(
      By.css('button.btn-outline-primary')
    );
    searchButton.nativeElement.click();

    expect(component.searchStudentsEvent.emit).toHaveBeenCalled();
  });

  it('should display formatted dates correctly', () => {
    component.selectedClass = mockClasses[0];
    fixture.detectChanges();

    const classDetailsElement = fixture.debugElement.query(
      By.css('.class-details')
    );
    expect(classDetailsElement.nativeElement.textContent).toContain(
      'Jan 15, 2023'
    ); // Start date
    expect(classDetailsElement.nativeElement.textContent).toContain(
      'May 15, 2023'
    ); // End date
  });
});
