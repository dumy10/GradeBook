import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import {
  AuthService,
  PasswordChangeRequest,
  ProfileUpdateRequest,
} from '../services/auth.service';
import { Class, ClassService, Student } from '../services/class.service';
import {
  Assignment,
  CreateGradeRequest,
  Grade,
  GradeService,
  UpdateGradeRequest,
} from '../services/grade.service';

// Import standalone components
import { ClassManagementComponent } from './components/class-management/class-management.component';
import { GradeManagementComponent } from './components/grade-management/grade-management.component';
import { PasswordChangeComponent } from './components/password-change/password-change.component';
import { ProfileComponent } from './components/profile/profile.component';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ProfileComponent,
    PasswordChangeComponent,
    ClassManagementComponent,
    GradeManagementComponent,
  ],
  templateUrl: './teacher-dashboard.component.html',
  styleUrls: ['./teacher-dashboard.component.scss'],
})
export class TeacherDashboardComponent implements OnInit {
  activeTab: 'profile' | 'password' | 'classes' | 'grades' = 'profile';
  userData: ProfileUpdateRequest = {
    firstName: '',
    lastName: '',
    phone: '',
    address: '',
  };

  userInfo = {
    email: '',
    username: '',
  };

  passwordData: PasswordChangeRequest = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  };

  // Class management
  classes: Class[] = [];
  selectedClass: Class | null = null;
  studentsInClass: Student[] = [];
  searchStudentTerm: string = '';
  searchClassTerm: string = '';
  searchResults: Student[] = [];

  // Combined grade management
  grades: Grade[] = [];
  gradesForClass: Grade[] = [];
  showHistoryView = false;

  // Grade management
  selectedStudent: Student | null = null;
  selectedGrade: Grade | null = null;
  assignments: Assignment[] = [];

  // New grade form
  newGrade: CreateGradeRequest = {
    assignmentId: 0,
    studentId: 0,
    points: 0,
    comment: '',
  };

  // Edit grade form
  editGradeForm: UpdateGradeRequest = {
    assignmentId: 0,
    studentId: 0,
    points: 0,
    comment: '',
  };

  // UI state
  successMessage = '';
  errorMessage = '';
  isLoading = false;
  classLoading = false;
  gradeLoading = false;
  showGradeModal = false;
  showEditGradeModal = false;
  showDeleteConfirmation = false;
  gradeToDelete: number | null = null;

  // Grade History modal
  showGradeHistoryModal = false;
  selectedGradeHistory: any = null;

  // Modals state
  showBulkGradeModal = false;
  showGradeUploadModal = false;

  // Bulk grading
  bulkGradeForm: {
    assignmentId: any;
    selectAll: boolean;
    students: Array<{
      userId: number;
      selected: boolean;
      points: number;
      comment: string;
    }>;
  } = {
    assignmentId: '',
    selectAll: false,
    students: [],
  };

  // File upload renamed to Quick Grade Form
  gradeUploadForm: {
    assignmentId: any;
    file: File | null;
  } = {
    assignmentId: '',
    file: null,
  };

  // Quick bulk grading
  quickGradeEntries: Array<{
    studentId: number;
    studentName: string;
    selected: boolean;
    points: number;
    comment: string;
  }> = [];

  quickGradeDefaults: {
    selectAll: boolean;
    points: number;
    comment: string;
  } = {
    selectAll: false,
    points: 0,
    comment: '',
  };

  constructor(
    private authService: AuthService,
    private router: Router,
    private classService: ClassService,
    private gradeService: GradeService
  ) {}

  ngOnInit(): void {
    // Check if user is logged in
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/teacher/login']);
      return;
    }

    // Check if user role is teacher
    const userRole = this.authService.getUserRole();
    if (userRole?.toUpperCase() !== 'TEACHER') {
      this.router.navigate(['/']);
      return;
    }

    // Get user data from localStorage
    const userData = localStorage.getItem('currentUser');
    if (userData) {
      try {
        const parsedUser = JSON.parse(userData);
        this.userInfo = {
          email: parsedUser.email || '',
          username: parsedUser.username || '',
        };

        this.userData = {
          firstName: parsedUser.firstName || '',
          lastName: parsedUser.lastName || '',
          phone: parsedUser.phone || '',
          address: parsedUser.address || '',
        };
      } catch (error) {
        console.error('Error parsing user data:', error);
      }
    }

    // Load classes when component initializes
    this.searchClasses('');
  }

  /**
   * Change active tab
   */
  changeTab(tab: 'profile' | 'password' | 'classes' | 'grades'): void {
    this.activeTab = tab;
    this.errorMessage = '';
    this.successMessage = '';

    // Stop loading states
    this.isLoading = false;
    this.classLoading = false;
    this.gradeLoading = false;

    // If changing to classes tab, search for classes
    if (tab === 'classes') {
      this.searchClasses(this.searchClassTerm);
    }

    // If changing to grades tab and we have a class selected, load grades
    if (tab === 'grades' && this.selectedClass) {
      this.loadGradesForClass(this.selectedClass.classId);
    }
  }

  // Event handlers for ProfileComponent
  updateProfile(userData: ProfileUpdateRequest): void {
    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.authService.updateProfile(userData).subscribe({
      next: (response) => {
        this.successMessage = 'Profile updated successfully';
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error?.error?.message || 'Failed to update profile';
        this.isLoading = false;
      },
    });
  }

  // Event handlers for PasswordChangeComponent
  changePassword(passwordData: PasswordChangeRequest): void {
    // Validation
    if (
      !passwordData.currentPassword ||
      !passwordData.newPassword ||
      !passwordData.confirmPassword
    ) {
      this.errorMessage = 'All password fields are required';
      return;
    }

    if (passwordData.newPassword !== passwordData.confirmPassword) {
      this.errorMessage = 'New passwords do not match';
      return;
    }

    if (passwordData.newPassword.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters long';
      return;
    }

    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.authService.changePassword(passwordData).subscribe({
      next: (response) => {
        this.successMessage = 'Password changed successfully';
        this.isLoading = false;

        // Reset form
        this.passwordData = {
          currentPassword: '',
          newPassword: '',
          confirmPassword: '',
        };
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message || 'Failed to change password';
        this.isLoading = false;
      },
    });
  }

  // Class management event handlers
  searchClasses(term: string): void {
    this.classLoading = true;
    this.classService.searchClasses(term).subscribe({
      next: (classes) => {
        this.classes = classes;
        this.classLoading = false;
      },
      error: (error) => {
        this.errorMessage = error?.error?.message || 'Failed to load classes';
        this.classLoading = false;
      },
    });
  }

  searchStudents(): void {
    if (!this.searchStudentTerm.trim()) {
      this.searchResults = [];
      return;
    }

    this.classLoading = true;
    this.classService.searchStudents(this.searchStudentTerm).subscribe({
      next: (students) => {
        this.searchResults = students;
        this.classLoading = false;
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message || 'Failed to search students';
        this.classLoading = false;
      },
    });
  }

  selectClass(classObj: Class): void {
    // Reset state
    this.selectedStudent = null;
    this.grades = [];
    this.gradesForClass = [];
    this.errorMessage = '';

    // Set new class and load data
    this.selectedClass = classObj;
    this.loadStudentsInClass(classObj.classId);
    this.loadAssignmentsForClass(classObj.classId);
    this.loadGradesForClass(classObj.classId);
  }

  loadStudentsInClass(classId: number): void {
    this.classLoading = true;
    this.classService.getStudentsInClass(classId).subscribe({
      next: (students) => {
        this.studentsInClass = students;
        this.classLoading = false;

        // Initialize bulk grading form with these students
        this.bulkGradeForm.students = this.studentsInClass.map((student) => ({
          userId: student.userId,
          selected: false,
          points: 0,
          comment: '',
        }));
      },
      error: (error) => {
        this.errorMessage = error?.error?.message || 'Failed to load students';
        this.classLoading = false;
      },
    });
  }

  loadAssignmentsForClass(classId: number): void {
    this.gradeLoading = true;
    this.gradeService.getAssignmentsForClass(classId).subscribe({
      next: (assignments) => {
        this.assignments = assignments;
        this.gradeLoading = false;
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message || 'Failed to load assignments';
        this.gradeLoading = false;
      },
    });
  }

  addStudentToClass(student: Student): void {
    if (!this.selectedClass) return;

    this.classLoading = true;
    this.classService
      .addStudentToClass(this.selectedClass.classId, student.userId)
      .subscribe({
        next: () => {
          // Add student to local list
          this.studentsInClass = [...this.studentsInClass, student];

          // Add student to bulk grading form
          this.bulkGradeForm.students.push({
            userId: student.userId,
            selected: false,
            points: 0,
            comment: '',
          });

          this.successMessage = `Added ${student.firstName} ${student.lastName} to class`;
          this.classLoading = false;
        },
        error: (error) => {
          this.errorMessage =
            error?.error?.message || 'Failed to add student to class';
          this.classLoading = false;
        },
      });
  }

  removeStudentFromClass(student: Student): void {
    if (!this.selectedClass) {
      this.errorMessage = 'No class selected';
      return;
    }

    // Debug the student object to see its structure
    console.log('Student object to remove:', student);

    // Extract and verify the student ID
    const studentId = student.userId;
    console.log('Student ID to be removed:', studentId);

    if (
      studentId === undefined ||
      studentId === null ||
      studentId === 0 ||
      isNaN(Number(studentId))
    ) {
      this.errorMessage = 'Invalid student ID';
      return;
    }

    const classId = this.selectedClass.classId;
    if (classId === undefined || classId === null || isNaN(Number(classId))) {
      this.errorMessage = 'Invalid class ID';
      return;
    }

    if (
      confirm(
        `Are you sure you want to remove ${student.firstName} ${student.lastName} from this class?`
      )
    ) {
      this.classLoading = true;

      this.classService.removeStudentFromClass(classId, studentId).subscribe({
        next: () => {
          // Remove from local list
          this.studentsInClass = this.studentsInClass.filter(
            (s) => s.userId !== studentId
          );

          // Remove from bulk grading form
          this.bulkGradeForm.students = this.bulkGradeForm.students.filter(
            (s) => s.userId !== studentId
          );

          this.successMessage = `Removed ${student.firstName} ${student.lastName} from class`;
          this.classLoading = false;
        },
        error: (error) => {
          this.errorMessage =
            error?.error?.message || 'Failed to remove student from class';
          this.classLoading = false;
        },
      });
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  // Grade management event handlers
  loadGradesForClass(classId: number): void {
    if (!classId) return;

    this.gradeLoading = true;
    this.errorMessage = '';

    this.gradeService.getGradesByClass(classId).subscribe({
      next: (grades) => {
        this.grades = grades;
        this.gradesForClass = grades;
        this.gradeLoading = false;
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message || 'Failed to load grades for class';
        this.gradeLoading = false;
      },
    });
  }

  loadGradesForStudent(studentId: number): void {
    if (!studentId) return;

    this.selectedStudent =
      this.studentsInClass.find((s) => s.userId === studentId) || null;
    this.gradeLoading = true;
    this.errorMessage = '';
    this.grades = [];

    this.gradeService.getStudentGrades(studentId).subscribe({
      next: (grades) => {
        this.grades = grades;
        this.gradeLoading = false;
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message || 'Failed to load student grades';
        this.gradeLoading = false;
      },
    });
  }

  selectStudentForGrades(student: Student | null): void {
    this.selectedStudent = student;
    if (student) {
      this.loadGradesForStudent(student.userId);
    } else {
      // If student is null, show all grades for the class
      if (this.selectedClass) {
        this.loadGradesForClass(this.selectedClass.classId);
      }
    }
  }

  openNewGradeModal(student: Student): void {
    this.selectedStudent = student;

    // Ensure we're using a numeric ID and not a string
    const studentId =
      typeof student.userId === 'string'
        ? parseInt(student.userId)
        : student.userId;

    // Reset the form with default values
    this.newGrade = {
      assignmentId: 0,
      studentId: studentId,
      points: 0,
      comment: '',
    };

    // Clear any previous messages
    this.successMessage = '';
    this.errorMessage = '';

    this.showGradeModal = true;
  }

  closeNewGradeModal(): void {
    this.showGradeModal = false;
    this.errorMessage = '';
  }

  // Check if assignment already has grade for student
  checkForExistingGrade(): void {
    if (!this.newGrade.assignmentId || !this.newGrade.studentId) {
      return;
    }

    this.gradeService
      .checkExistingGrade(this.newGrade.studentId, this.newGrade.assignmentId)
      .subscribe({
        next: (exists) => {
          if (exists) {
            this.errorMessage =
              'Student already has a grade for this assignment';
          } else {
            this.errorMessage = '';
          }
        },
        error: (error) => {
          this.errorMessage =
            error?.error?.message || 'Error checking existing grades';
        },
      });
  }

  // Check if points are within assignment range
  checkPointsRange(): void {
    const assignment = this.getSelectedAssignment();
    if (!assignment) return;

    // Check minimum points
    if (this.newGrade.points < assignment.minPoints) {
      this.errorMessage = `Points must be at least ${assignment.minPoints}`;
      return;
    }

    // Check maximum points
    if (this.newGrade.points > assignment.maxPoints) {
      this.errorMessage = `Points cannot exceed ${assignment.maxPoints}`;
      return;
    }

    // Clear error message if points are within range
    if (this.errorMessage && this.errorMessage.includes('Points')) {
      this.errorMessage = '';
    }
  }

  // Check if edit form points are within assignment range
  checkEditPointsRange(): void {
    const assignment = this.getSelectedAssignmentForEdit();
    if (!assignment) return;

    // Check minimum points
    if (this.editGradeForm.points < assignment.minPoints) {
      this.errorMessage = `Points must be at least ${assignment.minPoints}`;
      return;
    }

    // Check maximum points
    if (this.editGradeForm.points > assignment.maxPoints) {
      this.errorMessage = `Points cannot exceed ${assignment.maxPoints}`;
      return;
    }

    // Clear error message if points are within range
    if (this.errorMessage && this.errorMessage.includes('Points')) {
      this.errorMessage = '';
    }
  }

  // Check selected assignment for min/max points validation
  getSelectedAssignment(): Assignment | undefined {
    if (!this.newGrade.assignmentId || this.assignments.length === 0)
      return undefined;
    return this.assignments.find(
      (a) => a.assignmentId === this.newGrade.assignmentId
    );
  }

  // Check selected assignment for edit form validation
  getSelectedAssignmentForEdit(): Assignment | undefined {
    if (!this.editGradeForm.assignmentId || this.assignments.length === 0)
      return undefined;
    return this.assignments.find(
      (a) => a.assignmentId === this.editGradeForm.assignmentId
    );
  }

  createGrade(): void {
    // Form validation
    if (!this.newGrade.assignmentId || this.newGrade.assignmentId <= 0) {
      this.errorMessage = 'Please select an assignment';
      return;
    }

    if (this.newGrade.points < 0) {
      this.errorMessage = 'Points must be a positive number';
      return;
    }

    // Check if points are within assignment's min/max range
    const assignment = this.getSelectedAssignment();
    if (assignment) {
      if (this.newGrade.points < assignment.minPoints) {
        this.errorMessage = `Points must be at least ${assignment.minPoints}`;
        return;
      }

      if (this.newGrade.points > assignment.maxPoints) {
        this.errorMessage = `Points cannot exceed ${assignment.maxPoints}`;
        return;
      }
    }

    // First check if student already has a grade for this assignment
    this.gradeLoading = true;
    this.gradeService
      .checkExistingGrade(this.newGrade.studentId, this.newGrade.assignmentId)
      .subscribe({
        next: (exists) => {
          if (exists) {
            this.errorMessage =
              'Student already has a grade for this assignment';
            this.gradeLoading = false;
          } else {
            // If no existing grade, proceed with submission
            this.submitNewGrade();
          }
        },
        error: (error) => {
          this.errorMessage =
            error?.error?.message || 'Error checking existing grades';
          this.gradeLoading = false;
        },
      });
  }

  // Separated the actual submission to a new method
  private submitNewGrade(): void {
    this.successMessage = '';
    this.errorMessage = '';

    // Create a cleaned payload with proper type conversions
    const cleanedGradeRequest: CreateGradeRequest = {
      assignmentId: Number(this.newGrade.assignmentId),
      studentId: Number(this.newGrade.studentId),
      points: Number(this.newGrade.points),
      comment: this.newGrade.comment || '',
    };

    console.log('Sending create grade request:', cleanedGradeRequest);

    // Add a delay to avoid rapid consecutive requests that might cause server stress
    setTimeout(() => {
      this.gradeService.createGrade(cleanedGradeRequest).subscribe({
        next: (result) => {
          this.successMessage = 'Grade added successfully';
          this.showGradeModal = false;

          // Refresh grades for selected student or class
          if (this.selectedStudent) {
            this.loadGradesForStudent(this.selectedStudent.userId);
          } else if (this.selectedClass) {
            this.loadGradesForClass(this.selectedClass.classId);
          }

          this.gradeLoading = false;
        },
        error: (error) => {
          this.errorMessage = error?.error?.message || 'Failed to add grade';
          this.gradeLoading = false;
        },
      });
    }, 500);
  }

  openEditGradeModal(grade: Grade): void {
    // Close other modals first to prevent layering issues
    this.showGradeModal = false;
    this.showDeleteConfirmation = false;

    this.selectedGrade = {
      ...grade,
      gradeId: grade.gradeId,
      points: grade.points,
      comment: grade.comment || '',
      student: grade.student,
      assignment: grade.assignment,
    };

    // Make sure assignmentId is a number
    const assignmentId =
      typeof grade.assignment.assignmentId === 'string'
        ? parseInt(grade.assignment.assignmentId)
        : grade.assignment.assignmentId || 0;

    // Make sure studentId is a number
    const studentId =
      typeof grade.student.userId === 'string'
        ? parseInt(grade.student.userId)
        : grade.student.userId || 0;

    this.editGradeForm = {
      assignmentId: assignmentId,
      studentId: studentId,
      points: grade.points,
      comment: grade.comment || '',
    };

    console.log('Edit Grade Form:', this.editGradeForm);
    this.showEditGradeModal = true;
  }

  closeEditGradeModal(): void {
    this.showEditGradeModal = false;
  }

  updateGrade(): void {
    if (!this.selectedGrade) return;

    this.gradeLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    // Additional validation
    if (
      !this.editGradeForm.assignmentId ||
      this.editGradeForm.assignmentId <= 0
    ) {
      this.errorMessage = 'Please select an assignment';
      this.gradeLoading = false;
      return;
    }

    if (this.editGradeForm.points < 0) {
      this.errorMessage = 'Points must be a positive number';
      this.gradeLoading = false;
      return;
    }

    // Check if points are within assignment's min/max range
    const assignment = this.getSelectedAssignmentForEdit();
    if (assignment) {
      if (this.editGradeForm.points < assignment.minPoints) {
        this.errorMessage = `Points must be at least ${assignment.minPoints}`;
        this.gradeLoading = false;
        return;
      }

      if (this.editGradeForm.points > assignment.maxPoints) {
        this.errorMessage = `Points cannot exceed ${assignment.maxPoints}`;
        this.gradeLoading = false;
        return;
      }
    }

    // Debug logging
    console.log('Updating grade ID:', this.selectedGrade.gradeId);

    // Simplified payload with only essential fields
    const simplifiedPayload: UpdateGradeRequest = {
      assignmentId: Number(this.editGradeForm.assignmentId),
      studentId: Number(this.editGradeForm.studentId),
      points: Number(this.editGradeForm.points),
      comment: this.editGradeForm.comment || '',
    };

    console.log('Simplified update payload:', simplifiedPayload);

    this.gradeService
      .updateGrade(this.selectedGrade.gradeId, simplifiedPayload)
      .subscribe({
        next: (result) => {
          this.successMessage = 'Grade updated successfully';
          this.showEditGradeModal = false;

          // Refresh grades for selected student or class
          if (this.selectedStudent) {
            this.loadGradesForStudent(this.selectedStudent.userId);
          } else if (this.selectedClass) {
            this.loadGradesForClass(this.selectedClass.classId);
          }

          this.gradeLoading = false;
        },
        error: (error) => {
          this.errorMessage = error?.error?.message || 'Failed to update grade';
          this.gradeLoading = false;
        },
      });
  }

  confirmDeleteGrade(gradeId: number): void {
    // Close other modals first to prevent layering issues
    this.showGradeModal = false;
    this.showEditGradeModal = false;

    this.gradeToDelete = gradeId;
    this.showDeleteConfirmation = true;
  }

  closeDeleteConfirmation(): void {
    this.showDeleteConfirmation = false;
    this.gradeToDelete = null;
  }

  deleteGrade(): void {
    if (!this.gradeToDelete) return;

    this.gradeLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    console.log('Deleting grade with ID:', this.gradeToDelete);

    // Add a small delay to avoid concurrency issues
    setTimeout(() => {
      this.gradeService.deleteGrade(this.gradeToDelete!).subscribe({
        next: () => {
          this.successMessage = 'Grade deleted successfully';
          this.showDeleteConfirmation = false;
          this.gradeToDelete = null;

          // Refresh grades for selected student or class
          if (this.selectedStudent) {
            this.loadGradesForStudent(this.selectedStudent.userId);
          } else if (this.selectedClass) {
            this.loadGradesForClass(this.selectedClass.classId);
          }

          this.gradeLoading = false;
        },
        error: (error) => {
          this.errorMessage = error?.error?.message || 'Failed to delete grade';
          this.gradeLoading = false;
        },
      });
    }, 500);
  }

  // Toggle between regular grade view and detailed history view
  toggleHistoryView(): void {
    this.showHistoryView = !this.showHistoryView;
    // Implementation for refreshing the view
  }

  // Format date for display
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

  // Open grade history modal for a specific grade
  openGradeHistoryModal(grade: any): void {
    this.selectedGradeHistory = grade;
    this.showGradeHistoryModal = true;
  }

  // Close grade history modal
  closeGradeHistoryModal(): void {
    this.showGradeHistoryModal = false;
    this.selectedGradeHistory = null;
  }

  /**
   * Get assignment by ID
   */
  getAssignmentById(assignmentId: any): any {
    return this.assignments.find((a) => a.assignmentId == assignmentId);
  }

  /**
   * Open bulk grade modal
   */
  openBulkGradeModal(): void {
    // Reset form
    this.bulkGradeForm = {
      assignmentId: '',
      selectAll: false,
      students: this.studentsInClass.map((student) => ({
        userId: student.userId,
        selected: false,
        points: 0,
        comment: '',
      })),
    };

    this.showBulkGradeModal = true;
  }

  /**
   * Close bulk grade modal
   */
  closeBulkGradeModal(): void {
    this.showBulkGradeModal = false;
  }

  /**
   * Toggle all students in bulk grade form
   */
  toggleAllStudents(): void {
    const newState = this.bulkGradeForm.selectAll;
    this.bulkGradeForm.students.forEach((student) => {
      student.selected = newState;
    });
  }

  /**
   * Get count of selected students in bulk grade form
   */
  getSelectedStudentCount(): number {
    return this.bulkGradeForm.students.filter((s) => s.selected).length;
  }

  /**
   * Open dialog to fill all selected students with same points
   */
  bulkFillPoints(): void {
    const selectedStudents = this.bulkGradeForm.students.filter(
      (s) => s.selected
    );
    if (selectedStudents.length === 0) {
      alert('Please select at least one student first');
      return;
    }

    const points = prompt('Enter points to assign to all selected students:');
    if (points === null) return; // User cancelled

    const pointsNum = parseFloat(points);
    if (isNaN(pointsNum)) {
      alert('Please enter a valid number');
      return;
    }

    // Get assignment to check min/max
    const assignment = this.getAssignmentById(this.bulkGradeForm.assignmentId);
    if (assignment) {
      if (pointsNum < assignment.minPoints) {
        alert(`Points must be at least ${assignment.minPoints}`);
        return;
      }

      if (pointsNum > assignment.maxPoints) {
        alert(`Points cannot exceed ${assignment.maxPoints}`);
        return;
      }
    }

    // Apply points to all selected students
    this.bulkGradeForm.students.forEach((student) => {
      if (student.selected) {
        student.points = pointsNum;
      }
    });
  }

  /**
   * Check if bulk grades can be submitted
   */
  canSubmitBulkGrades(): boolean {
    if (!this.bulkGradeForm.assignmentId) return false;
    const selectedStudents = this.bulkGradeForm.students.filter(
      (s) => s.selected
    );
    return selectedStudents.length > 0;
  }

  /**
   * Submit bulk grades
   */
  submitBulkGrades(): void {
    if (!this.bulkGradeForm.assignmentId) {
      this.errorMessage = 'Please select an assignment';
      return;
    }

    const selectedStudents = this.bulkGradeForm.students.filter(
      (s) => s.selected
    );
    if (selectedStudents.length === 0) {
      this.errorMessage = 'Please select at least one student';
      return;
    }

    this.gradeLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    // Create array of grade requests
    const gradeRequests = selectedStudents.map((student) => ({
      assignmentId: Number(this.bulkGradeForm.assignmentId),
      studentId: student.userId,
      points: student.points,
      comment: student.comment || '',
    }));

    this.gradeService.createGradesBatch(gradeRequests).subscribe({
      next: (result) => {
        this.successMessage = `Successfully added ${result.length} grades`;
        this.showBulkGradeModal = false;

        // Refresh grades for class
        if (this.selectedClass) {
          this.loadGradesForClass(this.selectedClass.classId);
        }

        this.gradeLoading = false;
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message || 'Failed to add bulk grades';
        this.gradeLoading = false;
      },
    });
  }

  /**
   * Open quick bulk grade modal (replaces file upload)
   */
  openQuickBulkGradeModal(): void {
    // Reset form
    this.gradeUploadForm = {
      assignmentId: '',
      file: null,
    };

    this.quickGradeDefaults = {
      selectAll: false,
      points: 0,
      comment: '',
    };

    this.quickGradeEntries = [];
    this.showGradeUploadModal = true;
  }

  /**
   * Close quick bulk grade modal
   */
  closeGradeUploadModal(): void {
    this.showGradeUploadModal = false;
  }

  /**
   * Prepare quick bulk grade data when assignment is selected
   */
  prepareQuickBulkGradeData(): void {
    if (!this.gradeUploadForm.assignmentId || !this.studentsInClass.length) {
      this.quickGradeEntries = [];
      return;
    }

    // Check who already has grades for this assignment
    this.gradeLoading = true;

    this.gradeService
      .getGradesByAssignment(this.gradeUploadForm.assignmentId)
      .subscribe({
        next: (existingGrades) => {
          // Create entries for each student who doesn't already have a grade
          const studentsWithExistingGrades = new Set(
            existingGrades.map((g) => g.student.userId)
          );

          this.quickGradeEntries = this.studentsInClass
            .filter(
              (student) => !studentsWithExistingGrades.has(student.userId)
            )
            .map((student) => ({
              studentId: student.userId,
              studentName: `${student.firstName} ${student.lastName}`,
              selected: false,
              points: 0,
              comment: '',
            }));

          this.gradeLoading = false;
        },
        error: (error) => {
          this.errorMessage =
            error?.error?.message || 'Failed to check existing grades';
          this.gradeLoading = false;
        },
      });
  }

  /**
   * Toggle all students in quick grade form
   */
  toggleAllQuickGrades(): void {
    const newState = this.quickGradeDefaults.selectAll;
    this.quickGradeEntries.forEach((entry) => {
      entry.selected = newState;
    });
  }

  /**
   * Apply default values to all selected entries
   */
  applyDefaultValues(): void {
    const selectedEntries = this.quickGradeEntries.filter((e) => e.selected);
    if (selectedEntries.length === 0) {
      alert('Please select at least one student first');
      return;
    }

    // Get assignment to check min/max
    const assignment = this.getAssignmentById(
      this.gradeUploadForm.assignmentId
    );
    if (assignment) {
      if (this.quickGradeDefaults.points < assignment.minPoints) {
        alert(`Points must be at least ${assignment.minPoints}`);
        return;
      }

      if (this.quickGradeDefaults.points > assignment.maxPoints) {
        alert(`Points cannot exceed ${assignment.maxPoints}`);
        return;
      }
    }

    // Apply values to all selected entries
    this.quickGradeEntries.forEach((entry) => {
      if (entry.selected) {
        entry.points = this.quickGradeDefaults.points;
        entry.comment = this.quickGradeDefaults.comment;
      }
    });
  }

  /**
   * Sort quick grade entries by student name
   */
  sortQuickGradesByName(): void {
    this.quickGradeEntries.sort((a, b) =>
      a.studentName.localeCompare(b.studentName)
    );
  }

  /**
   * Get count of selected students in quick grade form
   */
  getSelectedQuickGradeCount(): number {
    return this.quickGradeEntries.filter((e) => e.selected).length;
  }

  /**
   * Check if quick grades can be submitted
   */
  canSubmitQuickGrades(): boolean {
    if (!this.gradeUploadForm.assignmentId) return false;
    const selectedEntries = this.quickGradeEntries.filter((e) => e.selected);
    return selectedEntries.length > 0;
  }

  /**
   * Submit quick grades
   */
  submitQuickGrades(): void {
    if (!this.gradeUploadForm.assignmentId) {
      this.errorMessage = 'Please select an assignment';
      return;
    }

    const selectedEntries = this.quickGradeEntries.filter((e) => e.selected);
    if (selectedEntries.length === 0) {
      this.errorMessage = 'Please select at least one student';
      return;
    }

    this.gradeLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    // Create array of grade requests
    const gradeRequests = selectedEntries.map((entry) => ({
      assignmentId: Number(this.gradeUploadForm.assignmentId),
      studentId: entry.studentId,
      points: entry.points,
      comment: entry.comment || '',
    }));

    this.gradeService.createGradesBatch(gradeRequests).subscribe({
      next: (result) => {
        this.successMessage = `Successfully added ${result.length} grades`;
        this.showGradeUploadModal = false;

        // Refresh grades for class
        if (this.selectedClass) {
          this.loadGradesForClass(this.selectedClass.classId);
        }

        this.gradeLoading = false;
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message || 'Failed to add quick grades';
        this.gradeLoading = false;
      },
    });
  }
}
