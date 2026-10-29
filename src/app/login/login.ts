import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { Notify } from 'notiflix/build/notiflix-notify-aio';

import {
  AuthService,
  LoginResponse
} from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  email = '';
  password = '';

  isLoading = signal(false);
  showRoleSelection = signal(false);
  availableRoles = signal<string[]>([]);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  login(): void {
    if (this.isLoading()) {
      return;
    }

    if (!this.email.trim() || !this.password) {
      Notify.warning('Please enter your email and password.');
      return;
    }

    this.sendLogin();
  }

  selectRole(role: string): void {
    if (this.isLoading()) {
      return;
    }

    this.sendLogin(role);
  }

  cancelRoleSelection(): void {
    this.showRoleSelection.set(false);
    this.availableRoles.set([]);
    this.password = '';
  }

  private sendLogin(selectedRole?: string): void {
    this.isLoading.set(true);

    this.authService.login({
      email: this.email.trim(),
      password: this.password,
      selectedRole
    }).subscribe({
      next: (response: LoginResponse) => {
        this.isLoading.set(false);

        // The backend requires a role choice.
        if (response.requiresRoleSelection) {
          this.availableRoles.set(response.roles ?? []);
          this.showRoleSelection.set(true);
          return;
        }

        // A successful response must contain a token and role.
        if (!response.token || !response.role) {
          Notify.failure(
            'Login response is missing the token or role.'
          );
          return;
        }

        Notify.success('Login successful.');

        // Change these paths if your routes use different names.
        if (response.role.toLowerCase() === 'admin') {
          this.router.navigate(['/admin']);
        } else if (
          response.role.toLowerCase() === 'employee'
        ) {
          this.router.navigate(['/employee']);
        } else {
          Notify.failure('Unrecognized user role.');
          this.authService.logout();
        }
      },

      error: (error: HttpErrorResponse) => {
        this.isLoading.set(false);
        console.error('Login error:', error);

        if (error.status === 400) {
          Notify.failure(
            error.error?.message ??
            'Invalid request or selected role.'
          );
        } else if (error.status === 401) {
          Notify.failure('Invalid email or password.');
        } else if (error.status === 403) {
          Notify.failure('You are not authorized.');
        } else if (error.status === 0) {
          Notify.failure(
            'Cannot connect to the backend. Check that the API is running.'
          );
        } else {
          Notify.failure(
            error.error?.message ?? 'Login failed.'
          );
        }
      }
    });
  }
}