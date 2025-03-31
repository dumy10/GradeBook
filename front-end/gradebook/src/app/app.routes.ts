import { Routes } from '@angular/router';
import { StudentAuthComponent } from './student-auth/student-auth.component';
import { WelcomePageComponent } from './welcome-page/welcome-page.component';
import { TeacherAuthComponent } from './teacher-auth/teacher-auth.component';
import { TeacherDashboardComponent } from './teacher-dashboard/teacher-dashboard.component';
import { StudentDashboardComponent } from './student-dashboard/student-dashboard.component';
import { teacherAuthGuard, studentAuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', component: WelcomePageComponent },
  { path: 'teacher', component: TeacherAuthComponent },
  { path: 'student', component: StudentAuthComponent },
  { 
    path: 'teacher-dashboard', 
    component: TeacherDashboardComponent,
    canActivate: [teacherAuthGuard]
  },
  { 
    path: 'student-dashboard', 
    component: StudentDashboardComponent,
    canActivate: [studentAuthGuard]
  },
  { path: '**', redirectTo: '' },
];
