
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
import {
  ActivatedRoute,
  Router,
  RouterModule
} from '@angular/router';

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

  // Hard Copy / Soft Copy information
  copyType?: string;
  isPrinted?: boolean;
  printedByUserId?: number | null;
  printedAt?: string | null;
}

// Approval signature information returned by the backend.
interface ApprovalSignature {
  approverName: string;
  approvedAt: string;
  signatureBase64: string;
  signatureContentType: string;
}

@Component({
  selector: 'app-request-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    AdminHeader,
    FooterComponent
  ],
  templateUrl: './request-management.html',
  styleUrl: './request-management.css'
})
export class RequestManagement implements OnInit, OnDestroy {

  private readonly adminUrl =
    'https://localhost:7103/api/Admin';

  requests: AdminLetterRequest[] = [];
  selectedRequest: AdminLetterRequest | null = null;

  selectedFilter = 'All';
  searchText = '';

  isLoading = false;
  isProcessing = false;
  isUploadingSignature = false;

  isDownloadingPdf = false;
  isPrintingPdf = false;
  markingPrintedRequestId: number | null = null;

  errorMessage = '';
  successMessage = '';
  actionErrorMessage = '';

  // Rejection
  showRejectForm = false;
  rejectionReason = '';

  // Admin signature upload
  selectedSignatureFile: File | null = null;
  signaturePreviewUrl: string | null = null;
  signatureUploaded = false;

  // Approval signature displayed in approved request preview.
  approvalSignature: ApprovalSignature | null = null;
  isLoadingApprovalSignature = false;

  private isBrowser = false;

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute,
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

    // Read the filter sent from the Admin Dashboard.
    // Example: /admin/requests?filter=Pending
    this.route.queryParamMap.subscribe(params => {

      const requestedFilter = params.get('filter') || 'All';

      const validFilters = [
        'All',
        'Pending',
        'Approved',
        'Rejected'
      ];

      this.selectedFilter =
        validFilters.find(
          filter =>
            filter.toLowerCase() === requestedFilter.trim().toLowerCase()
        ) || 'All';

      this.cdr.detectChanges();

    });

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
        request.status?.trim().toLowerCase() ===
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
  // Copy type and print status
  // ---------------------------------------

  isHardCopy(request: AdminLetterRequest): boolean {
    return request.copyType?.trim().toLowerCase() === 'hard copy';
  }

  isSoftCopy(request: AdminLetterRequest): boolean {
    return request.copyType?.trim().toLowerCase() === 'soft copy';
  }

  getPrintStatus(request: AdminLetterRequest): string {

    if (request.status?.trim().toLowerCase() !== 'approved') {
      return request.status;
    }

    if (!this.isHardCopy(request)) {
      return 'Not Required';
    }

    return request.isPrinted ? 'Printed' : 'Waiting for Print';

  }

  // ---------------------------------------
  // Request details and approval signature
  // ---------------------------------------

  viewRequest(request: AdminLetterRequest): void {

    this.selectedRequest = request;

    this.showRejectForm = false;
    this.rejectionReason = '';
    this.actionErrorMessage = '';

    // Clear previous request's approval signature.
    this.approvalSignature = null;
    this.isLoadingApprovalSignature = false;

    // Retrieve signature only for approved requests.
    if (request.status?.trim().toLowerCase() === 'approved') {

      this.isLoadingApprovalSignature = true;

      this.http.get<ApprovalSignature>(
        `${this.adminUrl}/approvalsignature`,
        {
          params: {
            id: request.letterRequestId.toString()
          }
        }
      ).subscribe({

        next: (response) => {

          // Make sure the response belongs to the currently opened request.
          if (
            this.selectedRequest?.letterRequestId ===
            request.letterRequestId
          ) {
            this.approvalSignature = response;
          }

          this.isLoadingApprovalSignature = false;
          this.cdr.detectChanges();

        },

        error: (error) => {

          console.error(
            'Unable to load approval signature:',
            error
          );

          this.isLoadingApprovalSignature = false;
          this.cdr.detectChanges();

        }

      });

    }

  }

  closeDetails(): void {

    this.selectedRequest = null;
    this.showRejectForm = false;
    this.rejectionReason = '';
    this.actionErrorMessage = '';

    this.approvalSignature = null;
    this.isLoadingApprovalSignature = false;

  }

  // ---------------------------------------
  // Admin signature upload
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

    if (request.status?.toLowerCase() !== 'pending') {
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

    if (this.selectedRequest.status?.toLowerCase() !== 'pending') {
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
  // Download approved signed PDF
  // ---------------------------------------

  downloadApprovedPdf(request: AdminLetterRequest): void {

    if (request.status?.toLowerCase() !== 'approved') {
      return;
    }

    this.isDownloadingPdf = true;
    this.actionErrorMessage = '';

    this.http.get(
      `${this.adminUrl}/approvedpdf?id=${request.letterRequestId}`,
      { responseType: 'blob' }
    ).subscribe({

      next: (pdfBlob) => {

        this.isDownloadingPdf = false;

        const url = URL.createObjectURL(pdfBlob);
        const link = document.createElement('a');

        link.href = url;
        link.download =
          `${this.getSafeFileName(request.employeeName)}_` +
          `${this.getSafeFileName(request.letterTypeName)}.pdf`;

        document.body.appendChild(link);
        link.click();
        link.remove();

        window.setTimeout(() => URL.revokeObjectURL(url), 60000);

        this.cdr.detectChanges();

      },

      error: (error) => {

        console.error('PDF download failed:', error);

        this.isDownloadingPdf = false;

        this.actionErrorMessage =
          'Unable to download the approved signed PDF. Please try again.';

        this.cdr.detectChanges();

      }

    });

  }

  // ---------------------------------------
  // Print approved signed PDF
  // ---------------------------------------

  printApprovedPdf(request: AdminLetterRequest): void {

    if (request.status?.toLowerCase() !== 'approved') {
      return;
    }

    if (this.isHardCopy(request) && request.isPrinted) {

      this.actionErrorMessage =
        'This Hard Copy request has already been marked as printed.';

      return;

    }

    const printWindow = window.open('', '_blank');

    if (!printWindow) {

      this.actionErrorMessage =
        'Your browser blocked the print window. Please allow pop-ups and try again.';

      return;

    }

    this.isPrintingPdf = true;
    this.actionErrorMessage = '';

    this.http.get(
      `${this.adminUrl}/approvedpdf?id=${request.letterRequestId}`,
      { responseType: 'blob' }
    ).subscribe({

      next: (pdfBlob) => {

        this.isPrintingPdf = false;

        const pdfUrl = URL.createObjectURL(pdfBlob);

        printWindow.location.href = pdfUrl;

        printWindow.onload = () => {

          printWindow.focus();
          printWindow.print();

          if (this.isHardCopy(request)) {
            this.markRequestAsPrinted(request.letterRequestId);
          }

        };

        window.setTimeout(() => {

          if (!printWindow.closed) {

            try {

              printWindow.focus();
              printWindow.print();

              if (
                this.isHardCopy(request) &&
                this.markingPrintedRequestId === null &&
                !request.isPrinted
              ) {
                this.markRequestAsPrinted(request.letterRequestId);
              }

            } catch (error) {

              console.error('Unable to open print dialog:', error);

            }

          }

        }, 1500);

        window.setTimeout(() => URL.revokeObjectURL(pdfUrl), 60000);

        this.cdr.detectChanges();

      },

      error: (error) => {

        console.error('PDF printing failed:', error);

        this.isPrintingPdf = false;
        printWindow.close();

        this.actionErrorMessage =
          error.error?.message ||
          'Unable to open the approved signed PDF for printing. Please try again.';

        this.cdr.detectChanges();

      }

    });

  }

  // ---------------------------------------
  // Mark Hard Copy as printed
  // ---------------------------------------

  markRequestAsPrinted(letterRequestId: number): void {

    if (this.markingPrintedRequestId !== null) {
      return;
    }

    const request = this.requests.find(
      item => item.letterRequestId === letterRequestId
    );

    if (!request) {
      return;
    }

    if (request.status?.toLowerCase() !== 'approved') {
      return;
    }

    if (!this.isHardCopy(request)) {
      return;
    }

    if (request.isPrinted) {
      return;
    }

    this.markingPrintedRequestId = letterRequestId;
    this.actionErrorMessage = '';

    this.http.post(
      `${this.adminUrl}/markprinted?id=${letterRequestId}`,
      {}
    ).subscribe({

      next: (response: any) => {

        this.markingPrintedRequestId = null;

        request.isPrinted = true;

        if (
          this.selectedRequest?.letterRequestId === letterRequestId
        ) {
          this.selectedRequest.isPrinted = true;
        }

        this.successMessage =
          response.message || 'Hard Copy marked as printed successfully.';

        this.cdr.detectChanges();

        this.loadRequests();

      },

      error: (error) => {

        console.error('Mark printed failed:', error);

        this.markingPrintedRequestId = null;

        if (error.status === 409) {

          this.actionErrorMessage =
            'This request has already been marked as printed.';

        } else if (error.status === 400) {

          this.actionErrorMessage =
            error.error?.message ||
            'Only approved Hard Copy requests can be marked as printed.';

        } else {

          this.actionErrorMessage =
            error.error?.message ||
            'Unable to update the print status. Please try again.';

        }

        this.cdr.detectChanges();

        this.loadRequests();

      }

    });

  }

  // ---------------------------------------
  // Safe PDF filename
  // ---------------------------------------

  private getSafeFileName(value: string): string {

    return (value || 'Approved_Letter')
      .replace(/[<>:"/\\|?*\x00-\x1F]/g, '')
      .trim()
      .replace(/\s+/g, '_');

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
