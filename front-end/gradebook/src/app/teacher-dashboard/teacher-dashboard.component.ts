import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, PasswordChangeRequest, ProfileUpdateRequest } from '../services/auth.service';
import { ClassService, Student, Class } from '../services/class.service';
import { GradeService, Grade, CreateGradeRequest, UpdateGradeRequest, Assignment } from '../services/grade.service';
import { catchError, tap, forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrls: ['./teacher-dashboard.component.scss']
})
export class TeacherDashboardComponent implements OnInit {
  activeTab: 'profile' | 'password' | 'classes' | 'grades' = 'profile';
  userData: ProfileUpdateRequest = {
    firstName: '',
    lastName: '',
    phone: '',
    address: ''
  };

  userInfo = {
    email: '',
    username: ''
  };

  passwordData: PasswordChangeRequest = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
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
    comment: ''
  };
  
  // Edit grade form
  editGradeForm: UpdateGradeRequest = {
    assignmentId: 0,
    studentId: 0,
    points: 0,
    comment: ''
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
    students: []
  };
  
  // File upload renamed to Quick Grade Form
  gradeUploadForm: {
    assignmentId: any;
    file: File | null;
  } = {
    assignmentId: '',
    file: null
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
    comment: ''
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
      this.router.navigate(['/teacher']);
      return;
    }

    // Check if user role is teacher
    const userRole = this.authService.getUserRole();
    if (userRole?.toUpperCase() !== 'TEACHER') {
      this.authService.logout();
      this.router.navigate(['/']);
      return;
    }

    // Get user data from localStorage
    const userData = localStorage.getItem('currentUser');
    if (userData) {
      const parsedData = JSON.parse(userData);
      this.userInfo.email = parsedData.email || '';
      this.userInfo.username = parsedData.username || '';
      
      // Initial values for first and last name
      this.userData.firstName = parsedData.firstName || '';
      this.userData.lastName = parsedData.lastName || '';
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
      this.searchClasses('');
    }
    
    // If changing to grades tab and we have a class selected, load grades
    if (tab === 'grades' && this.selectedClass) {
      // Set history view to true by default
      this.showHistoryView = true;
      
      if (this.selectedStudent) {
        this.loadGradesForStudent(this.selectedStudent.userId);
      } else {
        this.loadGradesForClass(this.selectedClass.classId);
      }
    }
  }

  updateProfile(): void {
    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.authService.updateProfile(this.userData).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Profile updated successfully!';
        } else {
          this.errorMessage = response.message || 'Failed to update profile';
        }
        this.isLoading = false;
      },
      error: (error) => {
        if (error.status === 405) {
          this.errorMessage = 'Method Not Allowed: The server doesn\'t support this request method for this endpoint.';
        } else {
          this.errorMessage = error.error?.message || 'An error occurred while updating profile';
        }
        this.isLoading = false;
      }
    });
  }

  changePassword(): void {
    // Validation
    if (!this.passwordData.currentPassword || 
        !this.passwordData.newPassword || 
        !this.passwordData.confirmPassword) {
      this.errorMessage = 'Please fill in all password fields';
      return;
    }

    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.errorMessage = 'New passwords do not match';
      return;
    }

    if (this.passwordData.newPassword.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters long';
      return;
    }

    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.authService.changePassword(this.passwordData).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Password changed successfully!';
          this.passwordData = {
            currentPassword: '',
            newPassword: '',
            confirmPassword: ''
          };
        } else {
          this.errorMessage = response.message || 'Failed to change password';
        }
        this.isLoading = false;
      },
      error: (error) => {
        if (error.status === 400) {
          this.errorMessage = 'Invalid password data. The current password may be incorrect.';
        } else if (error.status === 401) {
          this.errorMessage = 'Unauthorized. Please log in again.';
        } else {
          this.errorMessage = error.error?.message || 'An error occurred while changing password';
        }
        
        this.isLoading = false;
      }
    });
  }

  // Class management methods
  searchClasses(term: string): void {
    this.classLoading = true;
    this.classService.searchClasses(term).subscribe({
      next: (classes) => {
        this.classes = classes;
        this.classLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load classes';
        this.classLoading = false;
      }
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
        this.errorMessage = 'Failed to search students';
        this.classLoading = false;
      }
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
        console.log('Students in class:', students);
        
        // Safely process the students array with type checking
        this.studentsInClass = students.map(student => {
          // Add debugging to see what's coming from the API
          console.log('Raw student from API:', student);
          
          // Check which property contains the user ID
          const userId = typeof student.userId !== 'undefined' ? student.userId : 
                       (student.hasOwnProperty('id') ? (student as any).id : 0);
          
          console.log('Extracted userId:', userId);
          
          // Handle potential differences in API response format
          const processedStudent: Student = {
            userId: userId,
            email: student.email || '',
            firstName: student.firstName || '',
            lastName: student.lastName || '',
            role: student.role || ''
          };
          return processedStudent;
        });
        
        this.classLoading = false;
      },
      error: (error) => {
        console.error('Error loading students:', error);
        this.errorMessage = 'Failed to load students in this class';
        this.classLoading = false;
      }
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
        this.errorMessage = 'Failed to load assignments for this class';
        this.gradeLoading = false;
      }
    });
  }

  addStudentToClass(student: Student): void {
    if (!this.selectedClass) return;
    
    this.classLoading = true;
    this.classService.addStudentToClass(this.selectedClass.classId, student.userId).subscribe({
      next: () => {
        this.successMessage = `${student.firstName} ${student.lastName} added to class`;
        this.loadStudentsInClass(this.selectedClass!.classId);
        this.searchStudentTerm = '';
        this.searchResults = [];
      },
      error: (error) => {
        this.errorMessage = 'Failed to add student to class';
        this.classLoading = false;
      }
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
    
    if (studentId === undefined || studentId === null || studentId === 0 || isNaN(Number(studentId))) {
      this.errorMessage = `Invalid student ID: ${studentId}`;
      return;
    }
    
    const classId = this.selectedClass.classId;
    if (classId === undefined || classId === null || isNaN(Number(classId))) {
      this.errorMessage = `Invalid class ID: ${classId}`;
      return;
    }
    
    if (confirm(`Are you sure you want to remove ${student.firstName} ${student.lastName} from this class?`)) {
      this.classLoading = true;
      
      // Log the values being sent to the API
      console.log('Removing student with ID:', studentId, 'from class with ID:', classId);
      
      this.classService.removeStudentFromClass(classId, studentId).subscribe({
        next: () => {
          this.successMessage = `${student.firstName} ${student.lastName} removed from class`;
          this.loadStudentsInClass(classId);
        },
        error: (error) => {
          console.error('Error removing student:', error);
          this.errorMessage = 'Failed to remove student from class. Error: ' + 
                             (error.error?.message || error.message || 'Unknown error');
          this.classLoading = false;
        }
      });
    }
  }

  isStudentInClass(student: Student): boolean {
    if (!student || !this.studentsInClass) return false;
    return this.studentsInClass.some(s => s.userId === student.userId);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  // Grade management methods
  loadGradesForClass(classId: number): void {
    if (!classId) {
      this.gradesForClass = [];
      return;
    }
    
    this.gradeLoading = true;
    this.errorMessage = '';
    
    this.gradeService.getGradesByClass(classId).subscribe({
      next: (grades) => {
        this.gradesForClass = grades;
        this.grades = grades; // Keep both arrays in sync
        this.gradeLoading = false;
        console.log('Grades loaded for class:', this.grades);
      },
      error: (error) => {
        this.errorMessage = 'Failed to load grades for this class';
        this.gradesForClass = [];
        this.grades = [];
        this.gradeLoading = false;
      }
    });
  }
  
  loadGradesForStudent(studentId: number): void {
    if (!studentId) return;
    
    this.selectedStudent = this.studentsInClass.find(s => s.userId === studentId) || null;
    this.gradeLoading = true;
    this.errorMessage = '';
    this.grades = []; // Clear grades array while loading
    
    this.gradeService.getStudentGrades(studentId).subscribe({
      next: (grades) => {
        console.log('Loaded grades for student:', grades);
        this.grades = grades;
        this.gradeLoading = false;
      },
      error: (error) => {
        console.error('Error loading grades for student:', error);
        this.errorMessage = error.message || 'Failed to load grades for this student';
        this.gradeLoading = false;
      }
    });
  }
  
  selectStudentForGrades(student: Student): void {
    this.selectedStudent = student;
    this.loadGradesForStudent(student.userId);
  }
  
  openNewGradeModal(student: Student): void {
    this.selectedStudent = student;
    
    // Ensure we're using a numeric ID and not a string
    const studentId = typeof student.userId === 'string' 
      ? parseInt(student.userId) 
      : student.userId;
      
    // Reset the form with default values
    this.newGrade = {
      assignmentId: 0,
      studentId: studentId,
      points: 0,
      comment: ''
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
    
    this.gradeService.checkExistingGrade(
      this.newGrade.studentId, 
      this.newGrade.assignmentId
    ).subscribe({
      next: (exists) => {
        if (exists) {
          this.errorMessage = 'This student already has a grade for this assignment. Please edit the existing grade instead.';
        } else {
          this.errorMessage = '';
        }
      },
      error: (error) => {
        console.error('Error checking for existing grade:', error);
      }
    });
  }
  
  // Check selected assignment for min/max points validation
  getSelectedAssignment(): Assignment | undefined {
    if (!this.newGrade.assignmentId || this.assignments.length === 0) {
      return undefined;
    }
    return this.assignments.find(a => a.assignmentId === this.newGrade.assignmentId);
  }

  // Check selected assignment for edit form validation
  getSelectedAssignmentForEdit(): Assignment | undefined {
    if (!this.editGradeForm.assignmentId || this.assignments.length === 0) {
      return undefined;
    }
    return this.assignments.find(a => a.assignmentId === this.editGradeForm.assignmentId);
  }
  
  // Check if points are within assignment range
  checkPointsRange(): void {
    const assignment = this.getSelectedAssignment();
    if (!assignment) return;
    
    // Check minimum points
    if (this.newGrade.points < assignment.minPoints) {
      this.errorMessage = `Points must be at least ${assignment.minPoints} for this assignment.`;
      return;
    }
    
    // Check maximum points
    if (this.newGrade.points > assignment.maxPoints) {
      this.errorMessage = `Points cannot exceed ${assignment.maxPoints} for this assignment.`;
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
      this.errorMessage = `Points must be at least ${assignment.minPoints} for this assignment.`;
      return;
    }
    
    // Check maximum points
    if (this.editGradeForm.points > assignment.maxPoints) {
      this.errorMessage = `Points cannot exceed ${assignment.maxPoints} for this assignment.`;
      return;
    }
    
    // Clear error message if points are within range
    if (this.errorMessage && this.errorMessage.includes('Points')) {
      this.errorMessage = '';
    }
  }

  createGrade(): void {
    // Form validation
    if (!this.newGrade.assignmentId || this.newGrade.assignmentId <= 0) {
      this.errorMessage = 'Please select an assignment';
      return;
    }
    
    if (this.newGrade.points < 0) {
      this.errorMessage = 'Points cannot be negative';
      return;
    }
    
    // Check if points are within assignment's min/max range
    const assignment = this.getSelectedAssignment();
    if (assignment) {
      if (this.newGrade.points < assignment.minPoints) {
        this.errorMessage = `Points must be at least ${assignment.minPoints} for this assignment.`;
        return;
      }
      
      if (this.newGrade.points > assignment.maxPoints) {
        this.errorMessage = `Points cannot exceed ${assignment.maxPoints} for this assignment.`;
        return;
      }
    }
    
    // First check if student already has a grade for this assignment
    this.gradeLoading = true;
    this.gradeService.checkExistingGrade(
      this.newGrade.studentId, 
      this.newGrade.assignmentId
    ).subscribe({
      next: (exists) => {
        if (exists) {
          this.errorMessage = 'This student already has a grade for this assignment. Please edit the existing grade instead.';
          this.gradeLoading = false;
          return;
        }
        
        // Continue with grade creation if no existing grade
        this.submitNewGrade();
      },
      error: (error) => {
        console.error('Error checking for existing grade:', error);
        this.errorMessage = 'Error checking for existing grades. Please try again.';
        this.gradeLoading = false;
      }
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
      comment: this.newGrade.comment || ''
    };
    
    console.log('Sending create grade request:', cleanedGradeRequest);
    
    // Add a delay to avoid rapid consecutive requests that might cause server stress
    setTimeout(() => {
      this.gradeService.createGrade(cleanedGradeRequest).subscribe({
        next: (grade) => {
          console.log('Grade created successfully:', grade);
          this.successMessage = 'Grade created successfully!';
          
          // If we're viewing grades for a class, refresh the list
          if (this.selectedClass) {
            this.loadGradesForClass(this.selectedClass.classId);
          }
          
          // If we're viewing grades for a student, refresh the list
          if (this.selectedStudent) {
            this.loadGradesForStudent(this.selectedStudent.userId);
          }
          
          this.showGradeModal = false;
          this.gradeLoading = false;
        },
        error: (error) => {
          console.error('Error creating grade:', error);
          
          // Format a nice error message
          let errorMessage = 'Failed to create grade';
          
          if (error.message) {
            // Check for PostgreSQL DateTime errors
            if (error.message.includes('DateTime') || error.message.includes('timestamp')) {
              errorMessage = 'There was a server error related to date/time formatting. ' +
                'Please try again. If the problem persists, contact the administrator.';
            } else {
              errorMessage = error.message;
            }
          }
          
          // Show error in modal
          this.errorMessage = errorMessage;
          this.gradeLoading = false;
        }
      });
    }, 500); // Short delay
  }
  
  openEditGradeModal(grade: Grade): void {
    // Close other modals first to prevent layering issues
    this.showGradeModal = false;
    this.showDeleteConfirmation = false;
    
    this.selectedGrade = {
      ...grade,
      // Only keep the properties we need, excluding date properties
      gradeId: grade.gradeId,
      points: grade.points,
      comment: grade.comment || '',
      student: grade.student,
      assignment: grade.assignment
    };
    
    // Make sure assignmentId is a number
    const assignmentId = typeof grade.assignment.assignmentId === 'string' 
      ? parseInt(grade.assignment.assignmentId) 
      : grade.assignment.assignmentId || 0;
      
    // Make sure studentId is a number
    const studentId = typeof grade.student.userId === 'string'
      ? parseInt(grade.student.userId)
      : grade.student.userId || 0;
    
    this.editGradeForm = {
      assignmentId: assignmentId,
      studentId: studentId,
      points: grade.points,
      comment: grade.comment || ''
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
    if (!this.editGradeForm.assignmentId || this.editGradeForm.assignmentId <= 0) {
      this.errorMessage = 'Please select a valid assignment';
      this.gradeLoading = false;
      return;
    }
    
    if (this.editGradeForm.points < 0) {
      this.errorMessage = 'Points cannot be negative';
      this.gradeLoading = false;
      return;
    }
    
    // Check if points are within assignment's min/max range
    const assignment = this.getSelectedAssignmentForEdit();
    if (assignment) {
      if (this.editGradeForm.points < assignment.minPoints) {
        this.errorMessage = `Points must be at least ${assignment.minPoints} for this assignment.`;
        this.gradeLoading = false;
        return;
      }
      
      if (this.editGradeForm.points > assignment.maxPoints) {
        this.errorMessage = `Points cannot exceed ${assignment.maxPoints} for this assignment.`;
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
      comment: this.editGradeForm.comment || ''
    };
    
    console.log('Simplified update payload:', simplifiedPayload);
    
    this.gradeService.updateGrade(this.selectedGrade.gradeId, simplifiedPayload).subscribe({
      next: (result) => {
        console.log('Update result:', result);
        if (result) {
          this.successMessage = 'Grade updated successfully!';
          
          // If we're viewing grades for a class, refresh the list
          if (this.selectedClass) {
            this.loadGradesForClass(this.selectedClass.classId);
          }
          
          // If we're viewing grades for a student, refresh the list
          if (this.selectedStudent) {
            this.loadGradesForStudent(this.selectedStudent.userId);
          }
        } else {
          this.errorMessage = 'Failed to update grade';
        }
        
        this.showEditGradeModal = false;
        this.gradeLoading = false;
      },
      error: (error) => {
        console.error('Error updating grade:', error);
        
        // Attempt to provide a more detailed error message
        let errorMessage = 'Failed to update grade';
        
        if (error.message) {
          errorMessage = error.message;
        }
        
        // Check for nested error details
        if (error.error) {
          if (error.error.message) {
            errorMessage = error.error.message;
          } else if (error.error.title) {
            errorMessage = error.error.title;
          }
        }
        
        this.errorMessage = errorMessage;
        this.gradeLoading = false;
      }
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
        next: (result) => {
          console.log('Grade delete result:', result);
          this.successMessage = 'Grade deleted successfully!';
          
          // If we're viewing grades for a class, refresh the list
          if (this.selectedClass) {
            this.loadGradesForClass(this.selectedClass.classId);
          }
          
          // If we're viewing grades for a student, refresh the list
          if (this.selectedStudent) {
            this.loadGradesForStudent(this.selectedStudent.userId);
          }
          
          this.showDeleteConfirmation = false;
          this.gradeToDelete = null;
          this.gradeLoading = false;
        },
        error: (error) => {
          console.error('Error deleting grade:', error);
          this.errorMessage = error.message || 'Failed to delete grade';
          this.gradeLoading = false;
          
          // Even if there's an error, try to refresh the data as the grade might have been deleted
          if (this.selectedClass) {
            this.loadGradesForClass(this.selectedClass.classId);
          }
          if (this.selectedStudent) {
            this.loadGradesForStudent(this.selectedStudent.userId);
          }
          
          // Close dialog even on error, as the grade might actually be deleted
          // despite an error response from the server
          this.showDeleteConfirmation = false;
          this.gradeToDelete = null;
        }
      });
    }, 300);
  }

  // Toggle between regular grade view and detailed history view
  toggleHistoryView(): void {
    // Refresh the grades based on the view
    if (this.selectedClass) {
      if (this.selectedStudent) {
        this.loadGradesForStudent(this.selectedStudent.userId);
      } else {
        this.loadGradesForClass(this.selectedClass.classId);
      }
    }
  }

  // Helper to sort grades by date (newest first)
  sortGradesByDate(grades: Grade[]): Grade[] {
    return [...grades].sort((a, b) => {
      return new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime();
    });
  }
  
  // Format date for display
  formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString();
  }
  
  // Calculate percentage score
  calculatePercentage(points: number, maxPoints: number): number {
    if (!maxPoints) return 0;
    return Math.round((points / maxPoints) * 100);
  }
  
  // Get CSS class based on grade percentage
  getGradeClass(points: number, maxPoints: number): string {
    const percentage = this.calculatePercentage(points, maxPoints);
    if (percentage >= 90) return 'text-success fw-bold';
    if (percentage >= 80) return 'text-success';
    if (percentage >= 70) return 'text-primary';
    if (percentage >= 60) return 'text-warning';
    return 'text-danger';
  }

  // Open grade history modal for a specific grade
  openGradeHistoryModal(grade: any): void {
    this.showGradeHistoryModal = true;
    this.selectedGradeHistory = grade;
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
    if (!assignmentId) return null;
    const id = Number(assignmentId);
    return this.assignments.find(a => a.assignmentId === id);
  }

  /**
   * Open bulk grade modal
   */
  openBulkGradeModal(): void {
    if (!this.selectedClass || this.assignments.length === 0 || this.studentsInClass.length === 0) {
      this.errorMessage = 'Cannot bulk grade: Make sure class has assignments and students';
      return;
    }
    
    this.bulkGradeForm = {
      assignmentId: '',
      selectAll: false,
      students: this.studentsInClass.map(student => ({
        userId: student.userId,
        selected: false,
        points: 0,
        comment: ''
      }))
    };
    
    this.showBulkGradeModal = true;
  }
  
  /**
   * Close bulk grade modal
   */
  closeBulkGradeModal(): void {
    this.showBulkGradeModal = false;
    this.bulkGradeForm = {
      assignmentId: '',
      selectAll: false,
      students: []
    };
  }
  
  /**
   * Toggle all students in bulk grade form
   */
  toggleAllStudents(): void {
    this.bulkGradeForm.students.forEach(student => {
      student.selected = this.bulkGradeForm.selectAll;
    });
  }
  
  /**
   * Get count of selected students in bulk grade form
   */
  getSelectedStudentCount(): number {
    return this.bulkGradeForm.students.filter(s => s.selected).length;
  }
  
  /**
   * Open dialog to fill all selected students with same points
   */
  bulkFillPoints(): void {
    const selectedStudents = this.bulkGradeForm.students.filter(s => s.selected);
    if (selectedStudents.length === 0) {
      this.errorMessage = 'Please select at least one student first';
      return;
    }
    
    const pointsValue = prompt('Enter points for all selected students:');
    if (pointsValue === null) return; // User cancelled
    
    const points = Number(pointsValue);
    if (isNaN(points)) {
      this.errorMessage = 'Please enter a valid number';
      return;
    }
    
    const assignment = this.getAssignmentById(this.bulkGradeForm.assignmentId);
    if (!assignment) return;
    
    const minPoints = assignment.minPoints || 0;
    const maxPoints = assignment.maxPoints;
    
    if (points < minPoints || points > maxPoints) {
      this.errorMessage = `Points must be between ${minPoints} and ${maxPoints}`;
      return;
    }
    
    // Update points for all selected students
    this.bulkGradeForm.students.forEach(student => {
      if (student.selected) {
        student.points = points;
      }
    });
  }
  
  /**
   * Check if bulk grades can be submitted
   */
  canSubmitBulkGrades(): boolean {
    if (!this.bulkGradeForm.assignmentId) return false;
    
    const selectedStudents = this.bulkGradeForm.students.filter(s => s.selected);
    if (selectedStudents.length === 0) return false;
    
    // Check if all selected students have valid points
    const assignment = this.getAssignmentById(this.bulkGradeForm.assignmentId);
    if (!assignment) return false;
    
    const minPoints = assignment.minPoints || 0;
    const maxPoints = assignment.maxPoints;
    
    return selectedStudents.every(s => {
      return !isNaN(s.points) && s.points >= minPoints && s.points <= maxPoints;
    });
  }
  
  /**
   * Submit bulk grades
   */
  submitBulkGrades(): void {
    if (!this.canSubmitBulkGrades()) return;
    
    const selectedStudents = this.bulkGradeForm.students.filter(s => s.selected);
    const assignment = this.getAssignmentById(this.bulkGradeForm.assignmentId);
    
    this.isLoading = true;
    let successCount = 0;
    let errorCount = 0;
    
    // Create observables for each grade creation
    const gradeRequests = selectedStudents.map(student => {
      const gradeData = {
        assignmentId: Number(this.bulkGradeForm.assignmentId),
        studentId: student.userId,
        points: student.points,
        comment: student.comment
      };
      
      return this.gradeService.createGrade(gradeData).pipe(
        catchError(error => {
          console.error(`Error creating grade for student ${student.userId}:`, error);
          errorCount++;
          return of(null); // Return null on error but don't break the sequence
        }),
        tap(result => {
          if (result) successCount++;
        })
      );
    });
    
    // Execute all requests in parallel
    forkJoin(gradeRequests).subscribe({
      next: () => {
        this.isLoading = false;
        if (errorCount === 0) {
          this.successMessage = `Successfully added ${successCount} grades`;
        } else {
          this.successMessage = `Added ${successCount} grades, failed to add ${errorCount} grades`;
        }
        
        // Refresh grades list
        this.loadGradesForClass(this.selectedClass!.classId);
        this.closeBulkGradeModal();
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'An error occurred while submitting grades';
        console.error('Bulk grade submission error:', error);
      }
    });
  }
  
  /**
   * Open quick bulk grade modal (replaces file upload)
   */
  openQuickBulkGradeModal(): void {
    if (!this.selectedClass || this.assignments.length === 0 || this.studentsInClass.length === 0) {
      this.errorMessage = 'Cannot bulk grade: Make sure class has assignments and students';
      return;
    }
    
    this.gradeUploadForm = {
      assignmentId: '',
      file: null
    };
    this.quickGradeEntries = [];
    this.quickGradeDefaults = {
      selectAll: false,
      points: 0,
      comment: ''
    };
    
    this.showGradeUploadModal = true;
  }
  
  /**
   * Close quick bulk grade modal
   */
  closeGradeUploadModal(): void {
    this.showGradeUploadModal = false;
    this.gradeUploadForm = {
      assignmentId: '',
      file: null
    };
    this.quickGradeEntries = [];
  }
  
  /**
   * Prepare quick bulk grade data when assignment is selected
   */
  prepareQuickBulkGradeData(): void {
    if (!this.gradeUploadForm.assignmentId) return;
    
    const assignment = this.getAssignmentById(this.gradeUploadForm.assignmentId);
    if (!assignment) return;
    
    // Set default points to max points if not set
    if (!this.quickGradeDefaults.points) {
      this.quickGradeDefaults.points = assignment.maxPoints;
    }
    
    // Create entry for each student
    this.quickGradeEntries = this.studentsInClass.map(student => ({
      studentId: student.userId,
      studentName: `${student.firstName} ${student.lastName}`,
      selected: false,
      points: 0,
      comment: ''
    }));
    
    // Sort by name
    this.sortQuickGradesByName();
  }
  
  /**
   * Toggle all students in quick grade form
   */
  toggleAllQuickGrades(): void {
    this.quickGradeEntries.forEach(entry => {
      entry.selected = this.quickGradeDefaults.selectAll;
    });
  }
  
  /**
   * Apply default values to all selected entries
   */
  applyDefaultValues(): void {
    const selectedEntries = this.quickGradeEntries.filter(entry => entry.selected);
    if (selectedEntries.length === 0) {
      this.errorMessage = 'Please select at least one student first';
      return;
    }
    
    selectedEntries.forEach(entry => {
      entry.points = this.quickGradeDefaults.points;
      entry.comment = this.quickGradeDefaults.comment;
    });
  }
  
  /**
   * Sort quick grade entries by student name
   */
  sortQuickGradesByName(): void {
    this.quickGradeEntries.sort((a, b) => a.studentName.localeCompare(b.studentName));
  }
  
  /**
   * Get count of selected students in quick grade form
   */
  getSelectedQuickGradeCount(): number {
    return this.quickGradeEntries.filter(entry => entry.selected).length;
  }
  
  /**
   * Check if quick grades can be submitted
   */
  canSubmitQuickGrades(): boolean {
    if (!this.gradeUploadForm.assignmentId) return false;
    
    const selectedEntries = this.quickGradeEntries.filter(entry => entry.selected);
    if (selectedEntries.length === 0) return false;
    
    // Check if all selected entries have valid points
    const assignment = this.getAssignmentById(this.gradeUploadForm.assignmentId);
    if (!assignment) return false;
    
    const minPoints = assignment.minPoints || 0;
    const maxPoints = assignment.maxPoints;
    
    return selectedEntries.every(entry => {
      return !isNaN(entry.points) && entry.points >= minPoints && entry.points <= maxPoints;
    });
  }
  
  /**
   * Submit quick grades
   */
  submitQuickGrades(): void {
    if (!this.canSubmitQuickGrades()) return;
    
    const selectedEntries = this.quickGradeEntries.filter(entry => entry.selected);
    
    // Create grade requests
    const gradeRequests = selectedEntries.map(entry => ({
      assignmentId: Number(this.gradeUploadForm.assignmentId),
      studentId: entry.studentId,
      points: entry.points,
      comment: entry.comment
    }));
    
    this.isLoading = true;
    
    // Use the batch create method
    this.gradeService.createGradesBatch(gradeRequests).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.successMessage = `Successfully added ${gradeRequests.length} grades`;
        
        // Refresh grades list
        this.loadGradesForClass(this.selectedClass!.classId);
        this.closeGradeUploadModal();
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'An error occurred while submitting grades';
        console.error('Quick grade submission error:', error);
      }
    });
  }
} 