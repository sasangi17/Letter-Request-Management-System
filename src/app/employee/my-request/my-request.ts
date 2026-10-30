import {
  Component,
  OnInit,
  Inject,
  PLATFORM_ID,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule, isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';

import { AuthService } from '../../services/auth.service';
import {Header} from "../../employee/header/header";

interface LetterRequest {
  letterRequestId: number;
  letterTypeId: number;
  letterTypeName: string;
  requestDate: string;
  status: string;
  letterContent: string;
}

@Component({
  selector: 'app-my-requests',
  standalone: true,
  imports: [CommonModule, RouterModule, Header],
  templateUrl: './my-request.html',
  styleUrl: './my-request.css',
})
export class MyRequests implements OnInit {

  private readonly requestUrl =
    'https://localhost:7103/api/LetterRequest/myrequest';

  private readonly downloadUrl =
    'https://localhost:7103/api/LetterRequest/letterdownload';

  requests: LetterRequest[] = [];

  isLoading = true;
  errorMessage = '';
  downloadingRequestId: number | null = null;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
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

    if (!this.authService.hasRole('Employee')) {
      if (this.authService.hasRole('Admin')) {
        this.router.navigate(['/admin']);
      } else {
        this.authService.logout();
        this.router.navigate(['/login']);
      }
      return;
    }

    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.http.get<LetterRequest[]>(this.requestUrl).subscribe({
      next: response => {
        this.requests = Array.isArray(response) ? response : [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: error => {
        console.error('Failed to load requests:', error);

        this.errorMessage = error.status === 401
          ? 'Your session has expired. Please log in again.'
          : 'Unable to load your requests. Please try again.';

        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

viewLetter(request: LetterRequest): void {
  if (request.status?.toLowerCase() !== 'pending') {
    return;
  }

  const previewUrl =
    `https://localhost:7103/api/LetterRequest/preview?id=${request.letterRequestId}`;

  this.http.get(previewUrl, {
    responseType: 'blob'
  }).subscribe({
    next: file => {
      const fileUrl = window.URL.createObjectURL(file);

      window.open(fileUrl, '_blank');

      setTimeout(() => {
        window.URL.revokeObjectURL(fileUrl);
      }, 60000);
    },
    error: error => {
      console.error('Preview failed:', error);

      this.errorMessage =
        'Unable to preview this letter. Please try again.';

      this.cdr.detectChanges();
    }
  });
}

  downloadLetter(request: LetterRequest): void {
    if (request.status?.toLowerCase() !== 'approved') {
      return;
    }

    this.downloadingRequestId = request.letterRequestId;
    this.errorMessage = '';

    this.http.get(
      `${this.downloadUrl}?id=${request.letterRequestId}`,
      { responseType: 'blob' }
    ).subscribe({
      next: file => {
        if (!isPlatformBrowser(this.platformId)) {
          return;
        }

        const fileUrl = window.URL.createObjectURL(file);
        const link = document.createElement('a');

        link.href = fileUrl;
        link.download = `Letter-${request.letterRequestId}.pdf`;

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        window.URL.revokeObjectURL(fileUrl);

        this.downloadingRequestId = null;
        this.cdr.detectChanges();
      },
      error: error => {
        console.error('Failed to download letter:', error);

        this.errorMessage = error.status === 401
          ? 'Your session has expired. Please log in again.'
          : 'Unable to download the letter. Please try again.';

        this.downloadingRequestId = null;
        this.cdr.detectChanges();
      }
    });
  }

  goToRequestLetter(): void {
    this.router.navigate(['/employee/employee-letter']);
  }
}
