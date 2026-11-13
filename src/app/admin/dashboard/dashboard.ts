import {
  Component,
  OnInit,
  Inject,
  PLATFORM_ID,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';

import { AuthService } from '../../services/auth.service';
import { AdminHeader } from '../admin-header/admin-header';


interface AdminLetterRequest {
  letterRequestId: number;
  userId: number;
  empNo: string;
  employeeName: string;
  email: string;
  department: string;
  letterTypeName: string;
  requestDate: string;
  status: string;
  letterContent: string;
}


@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    AdminHeader
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})

export class AdminDashboard implements OnInit {

  private readonly apiUrl =
    'https://localhost:7103/api/Admin/requests';

  adminName = 'Administrator';
  adminEmail = '';

  requests: AdminLetterRequest[] = [];
  selectedRequest: AdminLetterRequest | null = null;

  selectedFilter = 'All';
  searchText = '';

  isLoading = true;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const user = this.authService.getLoggedInUser();

    if (!user || !this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    if (!this.authService.hasRole('Admin')) {
      if (this.authService.hasRole('Employee')) {
        this.router.navigate(['/employee']);
      } else {
        this.authService.logout();
        this.router.navigate(['/login']);
      }

      return;
    }

    this.adminName = user.fullName || 'Administrator';
    this.adminEmail = user.email || '';

    this.loadRequests();
  }

  // Load all employee letter requests.
  loadRequests(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.http.get<AdminLetterRequest[]>(this.apiUrl).subscribe({
      next: (response) => {
        this.requests = Array.isArray(response) ? response : [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },

      error: (error) => {
        console.error('Failed to load admin requests:', error);

        this.requests = [];
        this.isLoading = false;

        if (error.status === 401) {
          this.errorMessage =
            'Your session has expired. Please log in again.';
        } else if (error.status === 403) {
          this.errorMessage =
            'You do not have permission to access the admin dashboard.';
        } else {
          this.errorMessage =
            'Unable to load letter requests. Please check the backend connection.';
        }

        this.cdr.detectChanges();
      }
    });
  }

  // Dashboard summary counts.
  get totalRequests(): number {
    return this.requests.length;
  }

  get pendingRequests(): number {
    return this.countByStatus('Pending');
  }

  get approvedRequests(): number {
    return this.countByStatus('Approved');
  }

  get rejectedRequests(): number {
    return this.countByStatus('Rejected');
  }

  get filteredRequests(): AdminLetterRequest[] {
    const search = this.searchText.trim().toLowerCase();

    return this.requests.filter(request => {
      const matchesStatus =
        this.selectedFilter === 'All' ||
        request.status?.toLowerCase() ===
          this.selectedFilter.toLowerCase();

      const matchesSearch =
        !search ||
        request.employeeName?.toLowerCase().includes(search) ||
        request.empNo?.toLowerCase().includes(search) ||
        request.email?.toLowerCase().includes(search) ||
        request.letterTypeName?.toLowerCase().includes(search);

      return matchesStatus && matchesSearch;
    });
  }

  private countByStatus(status: string): number {
    return this.requests.filter(
      request =>
        request.status?.trim().toLowerCase() === status.toLowerCase()
    ).length;
  }

  // Open request details.
  viewRequest(request: AdminLetterRequest): void {
    this.selectedRequest = request;
  }

  // Close request details.
  closeDetails(): void {
    this.selectedRequest = null;
  }

  // Refresh dashboard.
  refreshDashboard(): void {
    this.loadRequests();
  }

  // Status CSS class.
  getStatusClass(status: string): string {
    return status?.trim().toLowerCase() || 'unknown';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
