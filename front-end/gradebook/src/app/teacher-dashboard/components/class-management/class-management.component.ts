import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Class, Course, Student } from '../../../services/class.service';

@Component({
  selector: 'app-class-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './class-management.component.html',
  styleUrl: './class-management.component.scss',
})
export class ClassManagementComponent implements OnInit, OnChanges {
  @Input() classes: Class[] = [];
  @Input() selectedClass: Class | null = null;
  @Input() studentsInClass: Student[] = [];
  @Input() searchStudentTerm = '';
  @Input() searchClassTerm = '';
  @Input() searchResults: Student[] = [];
  @Input() classLoading = false;
  @Input() successMessage = '';
  @Input() errorMessage = '';
  @Input() courses: Course[] = [];

  @Output() searchClassesEvent = new EventEmitter<string>();
  @Output() searchStudentsEvent = new EventEmitter();
  @Output() selectClassEvent = new EventEmitter<Class>();
  @Output() addStudentToClassEvent = new EventEmitter<Student>();
  @Output() removeStudentFromClassEvent = new EventEmitter<Student>();
  @Output() openCreateClassModal = new EventEmitter<void>();
  @Output() openCreateAssignmentModal = new EventEmitter<void>();

  // Map of course IDs to course data
  courseMap: {
    [id: number]: { name: string; code: string; description: string };
  } = {};

  constructor() {}

  ngOnInit(): void {
    // Initialize the courseMap when the component is created
    if (this.courses && this.courses.length > 0) {
      this.updateCourseMap();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    // When courses change, update the course map
    if (changes['courses'] && this.courses) {
      this.updateCourseMap();
    }
  }

  // Update courseMap when courses are loaded
  private updateCourseMap(): void {
    this.courseMap = {};
    this.courses.forEach((course) => {
      this.courseMap[course.courseId] = {
        name: course.courseName,
        code: course.courseCode,
        description: course.description,
      };
    });
    console.log('CourseMap updated:', this.courseMap);
  }

  // Get course name by ID
  getCourseName(courseId: number): string {
    return this.courseMap[courseId]?.name || `Course ${courseId}`;
  }

  // Get course code by ID
  getCourseCode(courseId: number): string {
    return this.courseMap[courseId]?.code || '';
  }

  // Get course description by ID
  getCourseDescription(courseId: number): string {
    return this.courseMap[courseId]?.description || '';
  }

  // Methods that emit events to the parent component
  searchClasses(term: string): void {
    this.searchClassesEvent.emit(term);
  }

  searchStudents(): void {
    this.searchStudentsEvent.emit();
  }

  selectClass(classObj: Class): void {
    this.selectClassEvent.emit(classObj);
  }

  addStudentToClass(student: Student): void {
    this.addStudentToClassEvent.emit(student);
  }

  removeStudentFromClass(student: Student): void {
    this.removeStudentFromClassEvent.emit(student);
  }

  isStudentInClass(student: Student): boolean {
    if (!student || !this.studentsInClass) return false;
    return this.studentsInClass.some((s) => s.userId === student.userId);
  }
}
