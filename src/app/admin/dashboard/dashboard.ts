
import {
  Component,
  OnInit,
  Inject,
  PLATFORM_ID,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule, isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute,Router, RouterModule } from '@angular/router';

import { AuthService } from '../../services/auth.service';
import { AdminHeader } from '../admin-header/admin-header';
import { FooterComponent } from '../../shared/footer/footer';

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

interface DailyActivity {
  dayLabel: string;
  fullDate: string;
  count: number;
  heightPercent: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    AdminHeader,
    FooterComponent
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class AdminDashboard implements OnInit {

  private readonly apiUrl = 'https://localhost:7103/api/Admin/requests';

  adminName = 'Administrator';
  adminEmail = '';

  requests: AdminLetterRequest[] = [];

  monthlyActivity: DailyActivity[] = [];

  yAxisTicks: number[] = [4, 3, 2, 1, 0];

  chartYAxisMax = 4;

  currentMonth = new Date();

  currentMonthLabel = '';
  currentMonthRequests = 0;

  isLoading = true;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute,
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

    this.currentMonth = new Date();

    this.currentMonthLabel =
      this.currentMonth.toLocaleDateString('en-US', {
        month: 'long',
        year: 'numeric'
      });

    this.loadRequests();
  }

  // Load requests from the existing backend API.
  loadRequests(): void {

    this.isLoading = true;
    this.errorMessage = '';

    this.http.get<AdminLetterRequest[]>(this.apiUrl).subscribe({

      next: (response) => {

        this.requests = Array.isArray(response) ? response : [];

        this.calculateMonthlyActivity();

        this.isLoading = false;
        this.cdr.detectChanges();
      },

      error: (error) => {

        console.error('Failed to load admin requests:', error);

        this.requests = [];
        this.monthlyActivity = [];
        this.currentMonthRequests = 0;

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

  // Navigate to Request Management with the selected filter.
  openRequests(filter: string): void {

    this.router.navigate(['/admin/requests'], {
      queryParams: {
        filter: filter
      }
    });

  }

  // Calculate daily requests for the current month.
  private calculateMonthlyActivity(): void {

    const year = this.currentMonth.getFullYear();
    const month = this.currentMonth.getMonth();

    const daysInMonth =
      new Date(year, month + 1, 0).getDate();

    const dailyCounts: number[] =
      new Array(daysInMonth).fill(0);

    this.currentMonthRequests = 0;

    this.requests.forEach(request => {

      if (!request.requestDate) {
        return;
      }

      const requestDate = new Date(request.requestDate);

      if (isNaN(requestDate.getTime())) {
        return;
      }

      const requestYear = requestDate.getFullYear();
      const requestMonth = requestDate.getMonth();
      const requestDay = requestDate.getDate();

      if (requestYear === year && requestMonth === month) {

        dailyCounts[requestDay - 1]++;
        this.currentMonthRequests++;

      }

    });

    const maximumDailyRequests =
      Math.max(...dailyCounts, 0);

    const step = Math.max(
      1,
      Math.ceil(maximumDailyRequests / 4)
    );

    this.chartYAxisMax = step * 4;

    this.yAxisTicks = [
      this.chartYAxisMax,
      step * 3,
      step * 2,
      step,
      0
    ];

    this.monthlyActivity = dailyCounts.map((count, index) => {

      const date = new Date(year, month, index + 1);

      return {
        dayLabel: String(index + 1),

        fullDate: date.toLocaleDateString('en-US', {
          day: 'numeric',
          month: 'long',
          year: 'numeric'
        }),

        count,

        heightPercent:
          (count / this.chartYAxisMax) * 100
      };

    });

  }

  // Summary card counts.
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

  // Get latest five requests.
  get recentRequests(): AdminLetterRequest[] {

    return [...this.requests]
      .sort((a, b) => {

        const dateA = new Date(a.requestDate).getTime();
        const dateB = new Date(b.requestDate).getTime();

        return dateB - dateA;

      })
      .slice(0, 5);

  }

  private countByStatus(status: string): number {

    return this.requests.filter(request =>
      request.status?.trim().toLowerCase() ===
      status.toLowerCase()
    ).length;

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
