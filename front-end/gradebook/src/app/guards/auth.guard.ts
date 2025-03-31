import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const teacherAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn() && authService.getUserRole()?.toUpperCase() === 'TEACHER') {
    return true;
  }

  // Redirect to the login page
  router.navigate(['/teacher']);
  return false;
};

export const studentAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn() && authService.getUserRole()?.toUpperCase() === 'STUDENT') {
    return true;
  }

  // Redirect to the login page
  router.navigate(['/student']);
  return false;
}; 