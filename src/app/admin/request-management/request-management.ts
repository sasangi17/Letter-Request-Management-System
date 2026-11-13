
import {
  Component,
  OnInit,
  OnDestroy,
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
  selector: 'app-request-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    AdminHeader
  ],
  templateUrl: './request-management.html',
  styleUrl: './request-management.css'
})

export class RequestManagement implements OnInit, OnDestroy {

  private readonly adminUrl = 'https://localhost:7103/api/Admin';

  requests: AdminLetterRequest[] = [];
  selectedRequest: AdminLetterRequest | null = null;

  selectedFilter = 'All';
  searchText = '';

  isLoading = false;
  isProcessing = false;
  isUploadingSignature = false;

  errorMessage = '';
  successMessage = '';
  actionErrorMessage = '';

  // Rejection
  showRejectForm = false;
  rejectionReason = '';

  // Signature
  selectedSignatureFile: File | null = null;
  signaturePreviewUrl: string | null = null;
  signatureUploaded = false;

  private isBrowser = false;

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    this.isBrowser = isPlatformBrowser(this.platformId);

    if (!this.isBrowser) {
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

    this.loadRequests();
    this.loadSavedSignature();
  }

  ngOnDestroy(): void {
    this.releaseSignaturePreview();
  }

  // ---------------------------------------
  // Load employee requests
  // ---------------------------------------

  loadRequests(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.http.get<AdminLetterRequest[]>(
      `${this.adminUrl}/requests`
    ).subscribe({
      next: (response) => {
        this.requests = Array.isArray(response) ? response : [];

        // Update the selected request with the latest server data.
        if (this.selectedRequest) {
          this.selectedRequest =
            this.requests.find(
              request =>
                request.letterRequestId ===
                this.selectedRequest?.letterRequestId
            ) ?? null;
        }

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Failed to load requests:', error);

        this.requests = [];
        this.isLoading = false;

        if (error.status === 401) {
          this.errorMessage =
            'Your session has expired. Please log in again.';
        } else if (error.status === 403) {
          this.errorMessage =
            'You do not have permission to view these requests.';
        } else {
          this.errorMessage =
            'Unable to load requests. Please check the backend connection.';
        }

        this.cdr.detectChanges();
      }
    });
  }

  // ---------------------------------------
  // Dashboard counts
  // ---------------------------------------

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

  private countByStatus(status: string): number {
    return this.requests.filter(
      request =>
        request.status?.trim().toLowerCase() === status.toLowerCase()
    ).length;
  }

  // ---------------------------------------
  // Search and filtering
  // ---------------------------------------

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

  changeFilter(filter: string): void {
    this.selectedFilter = filter;
  }

  getStatusClass(status: string): string {
    return status?.trim().toLowerCase() || 'unknown';
  }

  // ---------------------------------------
  // Request details
  // ---------------------------------------

  viewRequest(request: AdminLetterRequest): void {
    this.selectedRequest = request;
    this.showRejectForm = false;
    this.rejectionReason = '';
    this.actionErrorMessage = '';
  }

  closeDetails(): void {
    this.selectedRequest = null;
    this.showRejectForm = false;
    this.rejectionReason = '';
    this.actionErrorMessage = '';
  }

  // ---------------------------------------
  // Admin signature
  // ---------------------------------------

  loadSavedSignature(): void {
    this.http.get(`${this.adminUrl}/signature`, {
      responseType: 'blob'
    }).subscribe({
      next: (signatureBlob) => {
        this.releaseSignaturePreview();

        this.signaturePreviewUrl =
          URL.createObjectURL(signatureBlob);

        this.signatureUploaded = true;
        this.cdr.detectChanges();
      },
      error: () => {
        this.signatureUploaded = false;
        this.signaturePreviewUrl = null;
        this.cdr.detectChanges();
      }
    });
  }

  onSignatureSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    this.actionErrorMessage = '';
    this.successMessage = '';

    if (!file) {
      return;
    }

    const allowedTypes = ['image/png', 'image/jpeg'];

    if (!allowedTypes.includes(file.type)) {
      this.actionErrorMessage =
        'Only PNG and JPG signature images are allowed.';
      input.value = '';
      return;
    }

    if (file.size > 2 * 1024 * 1024) {
      this.actionErrorMessage =
        'Signature image must not exceed 2 MB.';
      input.value = '';
      return;
    }

    this.selectedSignatureFile = file;
    this.signatureUploaded = false;

    this.releaseSignaturePreview();

    this.signaturePreviewUrl = URL.createObjectURL(file);
  }

  uploadSignature(): void {
    if (!this.selectedSignatureFile) {
      this.actionErrorMessage =
        'Please select a signature image first.';
      return;
    }

    const formData = new FormData();
    formData.append('signature', this.selectedSignatureFile);

    this.isUploadingSignature = true;
    this.actionErrorMessage = '';
    this.successMessage = '';

    this.http.post(
      `${this.adminUrl}/signature`,
      formData
    ).subscribe({
      next: (response: any) => {
        this.isUploadingSignature = false;
        this.signatureUploaded = true;
        this.selectedSignatureFile = null;

        this.successMessage =
          response.message || 'Signature uploaded successfully.';

        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isUploadingSignature = false;
        this.signatureUploaded = false;

        this.actionErrorMessage =
          error.error?.message || 'Unable to upload the signature.';

        this.cdr.detectChanges();
      }
    });
  }

  private releaseSignaturePreview(): void {
    if (this.signaturePreviewUrl?.startsWith('blob:')) {
      URL.revokeObjectURL(this.signaturePreviewUrl);
    }

    this.signaturePreviewUrl = null;
  }

  // ---------------------------------------
  // Approve request
  // ---------------------------------------

  approveRequest(request: AdminLetterRequest): void {
    if (request.status.toLowerCase() !== 'pending') {
      return;
    }

    if (!this.signatureUploaded) {
      this.actionErrorMessage =
        'Please upload your admin signature before approving a request.';
      return;
    }

    const confirmed = confirm(
      `Are you sure you want to approve the request from ${request.employeeName}?`
    );

    if (!confirmed) {
      return;
    }

    this.isProcessing = true;
    this.actionErrorMessage = '';
    this.successMessage = '';

    this.http.post(
      `${this.adminUrl}/requestsapprove?id=${request.letterRequestId}`,
      {}
    ).subscribe({
      next: (response: any) => {
        this.isProcessing = false;

        this.successMessage =
          response.message ||
          'Request approved and signed PDF generated successfully.';

        this.closeDetails();
        this.loadRequests();
      },
      error: (error) => {
        this.isProcessing = false;

        this.actionErrorMessage =
          error.error?.message || 'Unable to approve this request.';

        this.cdr.detectChanges();
      }
    });
  }

  // ---------------------------------------
  // Reject request
  // ---------------------------------------

  openRejectForm(): void {
    if (!this.selectedRequest) {
      return;
    }

    if (this.selectedRequest.status.toLowerCase() !== 'pending') {
      return;
    }

    this.showRejectForm = true;
    this.rejectionReason = '';
    this.actionErrorMessage = '';
  }

  cancelRejectForm(): void {
    this.showRejectForm = false;
    this.rejectionReason = '';
    this.actionErrorMessage = '';
  }

  rejectRequest(): void {
    if (!this.selectedRequest) {
      return;
    }

    const reason = this.rejectionReason.trim();

    if (!reason) {
      this.actionErrorMessage =
        'Please enter a rejection reason.';
      return;
    }

    if (reason.length > 1000) {
      this.actionErrorMessage =
        'The rejection reason must not exceed 1000 characters.';
      return;
    }

    const confirmed = confirm(
      `Are you sure you want to reject the request from ${this.selectedRequest.employeeName}?`
    );

    if (!confirmed) {
      return;
    }

    this.isProcessing = true;
    this.actionErrorMessage = '';
    this.successMessage = '';

    this.http.post(
      `${this.adminUrl}/requestsreject?id=${this.selectedRequest.letterRequestId}`,
      { rejectionReason: reason }
    ).subscribe({
      next: (response: any) => {
        this.isProcessing = false;

        this.successMessage =
          response.message || 'Request rejected successfully.';

        this.closeDetails();
        this.loadRequests();
      },
      error: (error) => {
        this.isProcessing = false;

        this.actionErrorMessage =
          error.error?.message || 'Unable to reject this request.';

        this.cdr.detectChanges();
      }
    });
  }

  // ---------------------------------------
  // Navigation and logout
  // ---------------------------------------

  backToDashboard(): void {
    this.router.navigate(['/admin']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
