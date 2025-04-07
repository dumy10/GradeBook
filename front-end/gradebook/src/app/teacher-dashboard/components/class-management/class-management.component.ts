import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Class, Student } from '../../../services/class.service';

@Component({
  selector: 'app-class-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './class-management.component.html',
  styleUrl: './class-management.component.scss',
})
export class ClassManagementComponent implements OnInit {
  @Input() classes: Class[] = [];
  @Input() selectedClass: Class | null = null;
  @Input() studentsInClass: Student[] = [];
  @Input() searchStudentTerm = '';
  @Input() searchClassTerm = '';
  @Input() searchResults: Student[] = [];
  @Input() classLoading = false;
  @Input() successMessage = '';
  @Input() errorMessage = '';

  @Output() searchClassesEvent = new EventEmitter<string>();
  @Output() searchStudentsEvent = new EventEmitter();
  @Output() selectClassEvent = new EventEmitter<Class>();
  @Output() addStudentToClassEvent = new EventEmitter<Student>();
  @Output() removeStudentFromClassEvent = new EventEmitter<Student>();

  constructor() {}

  ngOnInit(): void {}

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
