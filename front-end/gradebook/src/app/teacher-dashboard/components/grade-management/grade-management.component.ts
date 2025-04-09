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
import { Class, Student, Course } from '../../../services/class.service';
import {
  Assignment,
  CreateGradeRequest,
  Grade,
  UpdateGradeRequest,
} from '../../../services/grade.service';

@Component({
  selector: 'app-grade-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './grade-management.component.html',
  styleUrl: './grade-management.component.scss',
})
export class GradeManagementComponent implements OnInit, OnChanges {
  @Input() selectedClass: Class | null = null;
  @Input() studentsInClass: Student[] = [];
  @Input() grades: Grade[] = [];
  @Input() gradesForClass: Grade[] = [];
  @Input() selectedStudent: Student | null = null;
  @Input() selectedGrade: Grade | null = null;
  @Input() assignments: Assignment[] = [];
  @Input() successMessage = '';
  @Input() errorMessage = '';
  @Input() gradeLoading = false;
  @Input() showGradeModal = false;
  @Input() showEditGradeModal = false;
  @Input() showDeleteConfirmation = false;
  @Input() gradeToDelete: number | null = null;
  @Input() showGradeHistoryModal = false;
  @Input() selectedGradeHistory: any = null;
  @Input() showBulkGradeModal = false;
  @Input() showGradeUploadModal = false;
  @Input() showHistoryView = false;
  @Input() courses: Course[] = [];

  // Bulk grading inputs
  @Input() bulkGradeForm: any = {
    assignmentId: '',
    selectAll: false,
    students: [],
  };

  // Quick grade entries
  @Input() quickGradeEntries: Array<{
    studentId: number;
    studentName: string;
    selected: boolean;
    points: number;
    comment: string;
  }> = [];

  @Input() quickGradeDefaults: {
    selectAll: boolean;
    points: number;
    comment: string;
  } = {
    selectAll: false,
    points: 0,
    comment: '',
  };

  // Grade upload form
  @Input() gradeUploadForm: {
    assignmentId: any;
    file: File | null;
  } = {
    assignmentId: '',
    file: null,
  };

  @Input() newGrade: CreateGradeRequest = {
    assignmentId: 0,
    studentId: 0,
    points: 0,
    comment: '',
  };

  @Input() editGradeForm: UpdateGradeRequest = {
    assignmentId: 0,
    studentId: 0,
    points: 0,
    comment: '',
  };

  // Output events
  @Output() selectStudentForGradesEvent = new EventEmitter<Student | null>();
  @Output() openNewGradeModalEvent = new EventEmitter<Student>();
  @Output() closeNewGradeModalEvent = new EventEmitter();
  @Output() createGradeEvent = new EventEmitter();
  @Output() openEditGradeModalEvent = new EventEmitter<Grade>();
  @Output() closeEditGradeModalEvent = new EventEmitter();
  @Output() updateGradeEvent = new EventEmitter();
  @Output() confirmDeleteGradeEvent = new EventEmitter<number>();
  @Output() closeDeleteConfirmationEvent = new EventEmitter();
  @Output() deleteGradeEvent = new EventEmitter();
  @Output() toggleHistoryViewEvent = new EventEmitter();
  @Output() openGradeHistoryModalEvent = new EventEmitter<Grade>();
  @Output() closeGradeHistoryModalEvent = new EventEmitter();
  @Output() openBulkGradeModalEvent = new EventEmitter();
  @Output() closeBulkGradeModalEvent = new EventEmitter();
  @Output() toggleAllStudentsEvent = new EventEmitter();
  @Output() bulkFillPointsEvent = new EventEmitter();
  @Output() submitBulkGradesEvent = new EventEmitter();
  @Output() openQuickBulkGradeModalEvent = new EventEmitter();
  @Output() closeGradeUploadModalEvent = new EventEmitter();
  @Output() prepareQuickBulkGradeDataEvent = new EventEmitter();
  @Output() toggleAllQuickGradesEvent = new EventEmitter();
  @Output() applyDefaultValuesEvent = new EventEmitter();
  @Output() sortQuickGradesByNameEvent = new EventEmitter();
  @Output() submitQuickGradesEvent = new EventEmitter();
  @Output() loadGradesForClassEvent = new EventEmitter<number>();
  @Output() checkForExistingGradeEvent = new EventEmitter();
  @Output() checkPointsRangeEvent = new EventEmitter();
  @Output() checkEditPointsRangeEvent = new EventEmitter();

  // Map of course IDs to course data
  courseMap: {[id: number]: {name: string, code: string, description: string}} = {};

  constructor() {}

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {
    // When courses change, update the course map
    if (changes['courses'] && this.courses) {
      this.updateCourseMap();
    }
  }

  // Update courseMap when courses are loaded
  private updateCourseMap(): void {
    this.courseMap = {};
    this.courses.forEach(course => {
      this.courseMap[course.courseId] = {
        name: course.courseName,
        code: course.courseCode,
        description: course.description
      };
    });
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

  // Methods that emit events to parent component
  selectStudentForGrades(student: Student | null): void {
    this.selectStudentForGradesEvent.emit(student);
  }

  openNewGradeModal(student: Student): void {
    this.openNewGradeModalEvent.emit(student);
  }

  closeNewGradeModal(): void {
    this.closeNewGradeModalEvent.emit();
  }

  createGrade(): void {
    this.createGradeEvent.emit();
  }

  openEditGradeModal(grade: Grade): void {
    this.openEditGradeModalEvent.emit(grade);
  }

  closeEditGradeModal(): void {
    this.closeEditGradeModalEvent.emit();
  }

  updateGrade(): void {
    this.updateGradeEvent.emit();
  }

  confirmDeleteGrade(gradeId: number): void {
    this.confirmDeleteGradeEvent.emit(gradeId);
  }

  closeDeleteConfirmation(): void {
    this.closeDeleteConfirmationEvent.emit();
  }

  deleteGrade(): void {
    this.deleteGradeEvent.emit();
  }

  toggleHistoryView(): void {
    this.toggleHistoryViewEvent.emit();
  }

  openGradeHistoryModal(grade: any): void {
    this.openGradeHistoryModalEvent.emit(grade);
  }

  closeGradeHistoryModal(): void {
    this.closeGradeHistoryModalEvent.emit();
  }

  openBulkGradeModal(): void {
    this.openBulkGradeModalEvent.emit();
  }

  closeBulkGradeModal(): void {
    this.closeBulkGradeModalEvent.emit();
  }

  toggleAllStudents(): void {
    this.toggleAllStudentsEvent.emit();
  }

  bulkFillPoints(): void {
    this.bulkFillPointsEvent.emit();
  }

  submitBulkGrades(): void {
    this.submitBulkGradesEvent.emit();
  }

  openQuickBulkGradeModal(): void {
    this.openQuickBulkGradeModalEvent.emit();
  }

  closeGradeUploadModal(): void {
    this.closeGradeUploadModalEvent.emit();
  }

  prepareQuickBulkGradeData(): void {
    this.prepareQuickBulkGradeDataEvent.emit();
  }

  toggleAllQuickGrades(): void {
    this.toggleAllQuickGradesEvent.emit();
  }

  applyDefaultValues(): void {
    this.applyDefaultValuesEvent.emit();
  }

  sortQuickGradesByName(): void {
    this.sortQuickGradesByNameEvent.emit();
  }

  submitQuickGrades(): void {
    this.submitQuickGradesEvent.emit();
  }

  loadGradesForClass(classId: number): void {
    this.loadGradesForClassEvent.emit(classId);
  }

  checkForExistingGrade(): void {
    this.checkForExistingGradeEvent.emit();
  }

  checkPointsRange(): void {
    this.checkPointsRangeEvent.emit();
  }

  checkEditPointsRange(): void {
    this.checkEditPointsRangeEvent.emit();
  }

  // Helper methods that can be implemented directly in this component
  getSelectedAssignment(): Assignment | undefined {
    if (!this.newGrade.assignmentId || this.assignments.length === 0)
      return undefined;
    return this.assignments.find(
      (a) => a.assignmentId === this.newGrade.assignmentId
    );
  }

  getSelectedAssignmentForEdit(): Assignment | undefined {
    if (!this.editGradeForm.assignmentId || this.assignments.length === 0)
      return undefined;
    return this.assignments.find(
      (a) => a.assignmentId === this.editGradeForm.assignmentId
    );
  }

  getAssignmentById(assignmentId: any): any {
    return this.assignments.find((a) => a.assignmentId == assignmentId);
  }

  calculatePercentage(points: number, maxPoints: number): number {
    if (!maxPoints || maxPoints === 0) return 0;
    return Math.round((points / maxPoints) * 100);
  }

  getGradeClass(points: number, maxPoints: number): string {
    const percentage = this.calculatePercentage(points, maxPoints);
    if (percentage >= 90) return 'text-success fw-bold';
    if (percentage >= 80) return 'text-success';
    if (percentage >= 70) return 'text-primary';
    if (percentage >= 60) return 'text-warning';
    return 'text-danger';
  }

  formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  getSelectedStudentCount(): number {
    if (!this.bulkGradeForm.students) return 0;
    return this.bulkGradeForm.students.filter((s: any) => s.selected).length;
  }

  getSelectedQuickGradeCount(): number {
    if (!this.quickGradeEntries) return 0;
    return this.quickGradeEntries.filter((e) => e.selected).length;
  }

  canSubmitBulkGrades(): boolean {
    if (!this.bulkGradeForm.assignmentId) return false;
    const selectedStudents = this.bulkGradeForm.students.filter(
      (s: any) => s.selected
    );
    return selectedStudents.length > 0;
  }

  canSubmitQuickGrades(): boolean {
    if (!this.gradeUploadForm.assignmentId) return false;
    const selectedEntries = this.quickGradeEntries.filter((e) => e.selected);
    return selectedEntries.length > 0;
  }
}
