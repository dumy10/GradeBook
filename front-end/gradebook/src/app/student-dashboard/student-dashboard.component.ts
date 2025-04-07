import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { GradesComponent } from './components/grades/grades.component';
import { HistoryComponent } from './components/history/history.component';
import { PasswordComponent } from './components/password/password.component';
import { ProfileComponent } from './components/profile/profile.component';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ProfileComponent,
    GradesComponent,
    HistoryComponent,
    PasswordComponent,
  ],
  templateUrl: './student-dashboard.component.html',
  styleUrls: ['./student-dashboard.component.scss'],
})
export class StudentDashboardComponent implements OnInit {
  activeTab: 'profile' | 'password' | 'grades' | 'history' = 'grades';

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    // Check if user is logged in
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/student']);
      return;
    }

    // Check if user role is student
    const userRole = this.authService.getUserRole();
    if (userRole?.toUpperCase() !== 'STUDENT') {
      this.authService.logout();
      this.router.navigate(['/']);
      return;
    }
  }

  changeTab(tab: 'profile' | 'password' | 'grades' | 'history'): void {
    this.activeTab = tab;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
