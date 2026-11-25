
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
import { Header } from '../../employee/header/header';
import { FooterComponent } from '../../shared/footer/footer';

interface LetterRequest {
  letterRequestId: number;
  letterTypeName: string;
  requestDate: string;
  status: string;
  letterContent: string;
  copyType: string;
  isPrinted: boolean;
  printedByUserId?: number | null;
  printedAt?: string | null;
}

interface LetterPreview {
  letterRequestId: number;
  letterContent: string;
  status: string;
  message: string;
}

@Component({
  selector: 'app-my-requests',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    Header,
    FooterComponent
  ],
  templateUrl: './my-request.html',
  styleUrl: './my-request.css'
})
export class MyRequests implements OnInit {

  private readonly requestUrl =
    'https://localhost:7103/api/LetterRequest/myrequest';

  private readonly previewUrl =
    'https://localhost:7103/api/LetterPdf/preview';

  private readonly downloadUrl =
    'https://localhost:7103/api/LetterPdf/download';

  private readonly cancelUrl =
    'https://localhost:7103/api/LetterRequest/cancel';

  requests: LetterRequest[] = [];

  isLoading = true;
  errorMessage = '';
  successMessage = '';

  downloadingRequestId: number | null = null;
  cancellingRequestId: number | null = null;

  showPreview = false;
  isPreviewLoading = false;
  previewLetter: LetterPreview | null = null;
  previewLetterTypeName = '';

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

  // Load employee letter requests
 loadRequests(): void {
  this.isLoading = true;
  this.errorMessage = '';

  this.http.get<LetterRequest[]>(this.requestUrl).subscribe({
    next: response => {
      console.log('API requests:', response);

      this.requests = Array.isArray(response)
        ? response.map(request => ({
            ...request,
            copyType: request.copyType?.trim() || 'Soft Copy',
            isPrinted: request.isPrinted ?? false
          }))
        : [];

      console.log(
        'Copy types received:',
        this.requests.map(request => ({
          id: request.letterRequestId,
          copyType: request.copyType,
          isPrinted: request.isPrinted
        }))
      );

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

  // View pending letter preview
  viewLetter(request: LetterRequest): void {

    if (request.status?.toLowerCase() !== 'pending') {
      return;
    }

    this.showPreview = true;
    this.isPreviewLoading = true;
    this.previewLetter = null;
    this.previewLetterTypeName = request.letterTypeName;
    this.errorMessage = '';

    this.http.get<LetterPreview>(
      `${this.previewUrl}?id=${request.letterRequestId}`
    ).subscribe({

      next: response => {

        if (response.status?.toLowerCase() !== 'pending') {
          this.closePreview();
          this.loadRequests();
          return;
        }

        this.previewLetter = response;
        this.isPreviewLoading = false;
        this.cdr.detectChanges();
      },

      error: error => {

        console.error('Preview failed:', error);

        this.isPreviewLoading = false;
        this.closePreview();

        this.errorMessage = error.status === 401
          ? 'Your session has expired. Please log in again.'
          : 'Unable to preview this letter. Please try again.';

        this.cdr.detectChanges();
      }

    });
  }

  // Close preview modal
  closePreview(): void {

    this.showPreview = false;
    this.previewLetter = null;
    this.previewLetterTypeName = '';
    this.isPreviewLoading = false;
  }

  // Download approved Soft Copy
  downloadLetter(request: LetterRequest): void {

    if (
      request.status?.toLowerCase() !== 'approved' ||
      request.copyType !== 'Soft Copy'
    ) {
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
          this.downloadingRequestId = null;
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

  // Cancel pending request
  cancelRequest(request: LetterRequest): void {

    if (request.status?.toLowerCase() !== 'pending') {
      return;
    }

    const confirmed = window.confirm(
      `Are you sure you want to cancel your ${request.letterTypeName} request?`
    );

    if (!confirmed) {
      return;
    }

    this.cancellingRequestId = request.letterRequestId;
    this.errorMessage = '';
    this.successMessage = '';

    this.http.post(
      `${this.cancelUrl}?id=${request.letterRequestId}`,
      null
    ).subscribe({

      next: () => {

        this.successMessage =
          'Your letter request has been cancelled successfully.';

        this.cancellingRequestId = null;
        this.loadRequests();
      },

      error: error => {

        console.error('Failed to cancel request:', error);

        if (error.status === 401) {

          this.errorMessage =
            'Your session has expired. Please log in again.';

        } else if (error.status === 404) {

          this.errorMessage =
            'The request was not found or is no longer pending. Refresh the page and try again.';

        } else {

          this.errorMessage =
            error.error?.message ||
            'Unable to cancel the request. Please try again.';
        }

        this.cancellingRequestId = null;
        this.cdr.detectChanges();
      }

    });
  }

  // Navigate to new letter request page
  goToRequestLetter(): void {
    this.router.navigate(['/employee/employee-letter']);
  }
}
