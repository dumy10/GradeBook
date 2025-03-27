import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-student-auth',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './student-auth.component.html',
  styleUrl: './student-auth.component.scss'
})
export class StudentAuthComponent {
  activeTab: 'login' | 'signup' = 'login';

}
