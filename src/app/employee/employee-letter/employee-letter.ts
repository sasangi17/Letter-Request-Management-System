
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
import { Header } from '../../employee/header/header';
import { FooterComponent } from '../../shared/footer/footer';

interface LetterType {
  letterTypeName: string;
  description: string;
}

@Component({
  selector: 'app-employee-letter',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Header, FooterComponent],
  templateUrl: './employee-letter.html',
  styleUrl: './employee-letter.css'
})
export class EmployeeLetter implements OnInit {

  private readonly requestUrl =
    'https://localhost:7103/api/LetterRequest/createrequest';

  employeeName = '';
  employeeEmail = '';
  employeeEmpNo = '';
  employeeDepartment = '';
  employeePosition = '';

  letterTypes: LetterType[] = [
    {
      letterTypeName: 'Service Letter',
      description: 'Request an official letter confirming your service.'
    },
    {
      letterTypeName: 'Employment Confirmation Letter',
      description: 'Request an official confirmation of your employment.'
    },
    {
      letterTypeName: 'Salary Confirmation Letter',
      description: 'Request an official letter confirming your salary.'
    }
  ];

  selectedLetterTypeName: string | null = null;

  // New: copy type selection
  selectedCopyType = 'Soft Copy';

  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

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

    if (!this.authService.hasRole('Employee')) {
      if (this.authService.hasRole('Admin')) {
        this.router.navigate(['/admin']);
      } else {
        this.authService.logout();
        this.router.navigate(['/login']);
      }
      return;
    }

    this.employeeName = user.fullName || '';
    this.employeeEmail = user.email || '';
    this.employeeEmpNo = user.empNo || '';
    this.employeeDepartment = user.department || '';
    this.employeePosition = user.position || '';
  }

  get selectedLetterType(): LetterType | undefined {
    return this.letterTypes.find(
      type => type.letterTypeName === this.selectedLetterTypeName
    );
  }

  submitRequest(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.selectedLetterTypeName) {
      this.errorMessage = 'Please select a letter type.';
      return;
    }

    if (!['Soft Copy', 'Hard Copy'].includes(this.selectedCopyType)) {
      this.errorMessage = 'Please select a valid copy type.';
      return;
    }

    this.isSubmitting = true;

    const requestData = {
      letterTypeName: this.selectedLetterTypeName,
      copyType: this.selectedCopyType
    };

    this.http.post(this.requestUrl, requestData).subscribe({
      next: (response) => {
        console.log('Letter request submitted:', response);

        this.successMessage =
          `${this.selectedCopyType} request submitted successfully.`;

        this.selectedLetterTypeName = null;
        this.selectedCopyType = 'Soft Copy';

        this.isSubmitting = false;
        this.cdr.detectChanges();
      },

      error: (error) => {
        console.error('Failed to submit letter request:', error);

        if (error.status === 400) {
          this.errorMessage =
            error.error?.message ||
            'The request could not be submitted. Please check your selections.';
        } else if (error.status === 401) {
          this.errorMessage =
            'Your session has expired. Please log in again.';
        } else if (error.status === 403) {
          this.errorMessage =
            'You do not have permission to submit a request.';
        } else {
          this.errorMessage =
            'Unable to submit your request. Please try again.';
        }

        this.isSubmitting = false;
        this.cdr.detectChanges();
      }
    });
  }

  goToDashboard(): void {
    this.router.navigate(['/employee']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
