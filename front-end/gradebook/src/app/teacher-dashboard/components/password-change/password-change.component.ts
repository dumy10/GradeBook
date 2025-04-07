import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PasswordChangeRequest } from '../../../services/auth.service';

@Component({
  selector: 'app-password-change',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './password-change.component.html',
  styleUrl: './password-change.component.scss',
})
export class PasswordChangeComponent implements OnInit {
  @Input() passwordData: PasswordChangeRequest = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  };

  @Input() successMessage = '';
  @Input() errorMessage = '';
  @Input() isLoading = false;

  @Output() changePasswordEvent = new EventEmitter<PasswordChangeRequest>();

  constructor() {}

  ngOnInit(): void {}

  get passwordsMatch(): boolean {
    return this.passwordData.newPassword === this.passwordData.confirmPassword;
  }

  changePassword(): void {
    this.changePasswordEvent.emit(this.passwordData);
  }
}
