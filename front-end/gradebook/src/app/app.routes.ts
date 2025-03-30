import { Routes } from '@angular/router';
import { WelcomePageComponent } from './welcome-page/welcome-page.component';
import { TeacherAuthComponent } from './teacher-auth/teacher-auth.component';
import { StudentAuthComponent } from './student-auth/student-auth.component';
import { StudentDashboardComponent } from './student-dashboard/student-dashboard.component';

export const routes: Routes = [
  { path: '', component: WelcomePageComponent },
  { path: 'teacher', component: TeacherAuthComponent },
  { path: 'student', component: StudentAuthComponent },
  { 
    path: 'student-dashboard', 
    component: StudentDashboardComponent,
    children: [
      { path: 'dashboard', loadComponent: () => import('./student-dashboard/pages/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: '' },
];
