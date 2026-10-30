
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
import {Header} from "../../employee/header/header";

interface LetterType {
  letterTypeId: number;
  letterTypeName: string;
  description?: string;
}

@Component({
  selector: 'app-employee-letter',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Header],
  templateUrl: './employee-letter.html',
  styleUrl: './employee-letter.css'
})
export class EmployeeLetter implements OnInit {

  private readonly letterTypeUrl =
    'https://localhost:7103/api/LetterType';

  private readonly requestUrl =
    'https://localhost:7103/api/LetterRequest/createrequest';


employeeName = '';
employeeEmail = '';
employeeEmpNo = '';
employeeDepartment = '';
employeePosition = '';  
  letterTypes: LetterType[] = [];

  selectedLetterTypeId: number | null = null;

  isLoadingTypes = true;
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

if (user) {
  this.employeeName = user.fullName || '';
  this.employeeEmail = user.email || '';
  this.employeeEmpNo = user.empNo || '';
  this.employeeDepartment = user.department || '';
  this.employeePosition = user.position || '';
}

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

    this.loadLetterTypes();
  }

  // Load available letter types.
  loadLetterTypes(): void {
    this.isLoadingTypes = true;
    this.errorMessage = '';

    this.http.get<LetterType[]>(this.letterTypeUrl).subscribe({
      next: (response) => {
        this.letterTypes = Array.isArray(response)
          ? response
          : [];

        this.isLoadingTypes = false;
        this.cdr.detectChanges();
      },

      error: (error) => {
        console.error('Failed to load letter types:', error);

        this.errorMessage =
          error.status === 401
            ? 'Your session has expired. Please log in again.'
            : 'Unable to load letter types. Please check the backend connection.';

        this.isLoadingTypes = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Get the selected letter type.
  get selectedLetterType(): LetterType | undefined {
    return this.letterTypes.find(
      type =>
        type.letterTypeId === Number(this.selectedLetterTypeId)
    );
  }

  // Submit a letter request.
  submitRequest(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.selectedLetterTypeId) {
      this.errorMessage = 'Please select a letter type.';
      return;
    }

    this.isSubmitting = true;

    const requestData = {
      letterTypeId: Number(this.selectedLetterTypeId)
    };

    this.http.post(
      this.requestUrl,
      requestData
    ).subscribe({
      next: (response) => {
        console.log('Letter request submitted:', response);

        this.successMessage =
          'Your letter request has been submitted successfully.';

        this.selectedLetterTypeId = null;
        this.isSubmitting = false;
        this.cdr.detectChanges();
      },

      error: (error) => {
        console.error('Failed to submit letter request:', error);

        if (error.status === 400) {
          this.errorMessage =
            'The request could not be submitted. Please check the selected letter type.';
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
