import { Component, OnInit, Inject, PLATFORM_ID ,ChangeDetectorRef } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { finalize } from 'rxjs';
import {Header} from "../employee/header/header";

interface LetterRequest {
  letterRequestId: number;
  status: string;
}

@Component({
  selector: 'app-employee',
  standalone: true,
  imports: [CommonModule,
  RouterModule,
  Header],
  templateUrl: './employee.html',
  styleUrl: './employee.css'
})
export class Employee implements OnInit {

  private readonly apiUrl =
    'https://localhost:7103/api/LetterRequest';

  employeeName = '';
  employeeEmail = '';
  employeeDepartment = '';
  employeePosition = '';

  requests: LetterRequest[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) private platformId: object
  ) {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const user = this.authService.getLoggedInUser();

    if (!user || !this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    if (!this.authService.hasRole('Employee')) {
      if (this.authService.hasRole('Admin')) {
        this.router.navigate(['/admin']);
      } else {
        this.authService.logout();
        this.router.navigate(['/login']);
      }
      return;
    }

    this.employeeName = user.fullName || 'Employee';
    this.employeeEmail = user.email || '';
    this.employeeDepartment = user.department || '';
    this.employeePosition = user.position || '';
  }

  ngOnInit(): void {
    if (
      isPlatformBrowser(this.platformId) &&
      this.authService.isLoggedIn() &&
      this.authService.hasRole('Employee')
    ) {
      this.loadMyRequests();
    } else {
      this.isLoading = false;
    }
  }

  

loadMyRequests(): void {
  this.isLoading = true;
  this.errorMessage = '';
  this.cdr.detectChanges();

  this.http
    .get<LetterRequest[]>(
      `${this.apiUrl}/myrequest`
    )
    .subscribe({
      next: (response) => {
        console.log('Requests received:', response);

        this.requests = Array.isArray(response)
          ? response
          : [];

        this.isLoading = false;
        this.cdr.detectChanges();
      },

      error: (error) => {
        console.error('Request loading failed:', error);

        this.requests = [];
        this.isLoading = false;

        this.errorMessage =
          error.status === 401
            ? 'Please log in again.'
            : error.status === 403
              ? 'You do not have permission to view these requests.'
              : 'Unable to load requests. Please try again.';

        this.cdr.detectChanges();
      }
    });
}


  get myRequests(): number {
    return this.requests.length;
  }

  get pendingRequests(): number {
    return this.requests.filter(
      request => request.status?.trim().toLowerCase() === 'pending'
    ).length;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
