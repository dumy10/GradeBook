import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProfileUpdateRequest } from '../../../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  @Input() userData: ProfileUpdateRequest = {
    firstName: '',
    lastName: '',
    phone: '',
    address: '',
  };

  @Input() userInfo = {
    email: '',
    username: '',
  };

  @Input() successMessage = '';
  @Input() errorMessage = '';
  @Input() isLoading = false;

  @Output() updateProfileEvent = new EventEmitter<ProfileUpdateRequest>();

  constructor() {}

  ngOnInit(): void {}

  updateProfile(): void {
    this.updateProfileEvent.emit(this.userData);
  }
}
