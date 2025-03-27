import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-teacher-auth',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './teacher-auth.component.html',
  styleUrl: './teacher-auth.component.scss'
})
export class TeacherAuthComponent {
  activeTab: 'login' | 'signup' = 'login';
}
