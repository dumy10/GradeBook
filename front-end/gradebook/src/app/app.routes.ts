import { Routes } from '@angular/router';
import { WelcomePageComponent } from './welcome-page/welcome-page.component';
import { TeacherAuthComponent } from './teacher-auth/teacher-auth.component';
import { StudentAuthComponent } from './student-auth/student-auth.component';

export const routes: Routes = [
  { path: '', component: WelcomePageComponent },
  { path: 'teacher', component: TeacherAuthComponent },
  { path: 'student', component: StudentAuthComponent },
  { path: '**', redirectTo: '' }, 
];
