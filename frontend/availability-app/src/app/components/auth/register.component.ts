import { Component, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="register-container">
      <div class="register-card">
        <div class="logo">
          <h1>AvailabilityApp</h1>
          <p>Create Service Provider Account</p>
        </div>
        
        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="register-form">
          <div class="form-row">
            <div class="form-group">
              <label for="firstName">First Name *</label>
              <input
                type="text"
                id="firstName"
                formControlName="firstName"
                class="form-input"
                [class.error]="registerForm.get('firstName')?.invalid && registerForm.get('firstName')?.touched"
              >
              <div *ngIf="registerForm.get('firstName')?.invalid && registerForm.get('firstName')?.touched" class="error-message">
                First name is required
              </div>
            </div>

            <div class="form-group">
              <label for="lastName">Last Name *</label>
              <input
                type="text"
                id="lastName"
                formControlName="lastName"
                class="form-input"
                [class.error]="registerForm.get('lastName')?.invalid && registerForm.get('lastName')?.touched"
              >
              <div *ngIf="registerForm.get('lastName')?.invalid && registerForm.get('lastName')?.touched" class="error-message">
                Last name is required
              </div>
            </div>
          </div>

          <div class="form-group">
            <label for="email">Email *</label>
            <input
              type="email"
              id="email"
              formControlName="email"
              class="form-input"
              [class.error]="registerForm.get('email')?.invalid && registerForm.get('email')?.touched"
            >
            <div *ngIf="registerForm.get('email')?.invalid && registerForm.get('email')?.touched" class="error-message">
              Valid email is required
            </div>
          </div>

          <div class="form-group">
            <label for="password">Password *</label>
            <input
              type="password"
              id="password"
              formControlName="password"
              class="form-input"
              [class.error]="registerForm.get('password')?.invalid && registerForm.get('password')?.touched"
            >
            <div *ngIf="registerForm.get('password')?.invalid && registerForm.get('password')?.touched" class="error-message">
              Password must be at least 6 characters
            </div>
          </div>

          <div class="form-group">
            <label for="businessName">Business Name</label>
            <input
              type="text"
              id="businessName"
              formControlName="businessName"
              class="form-input"
            >
          </div>

          <div class="form-group">
            <label for="phoneNumber">Phone Number</label>
            <input
              type="tel"
              id="phoneNumber"
              formControlName="phoneNumber"
              class="form-input"
            >
          </div>

          <div *ngIf="errorMessage || (errorList?.length)" class="error-box" role="alert" aria-live="polite">
            <div class="error-box-inner">
              <span class="error-icon" aria-hidden="true">⚠️</span>
              <div class="error-content">
                <strong *ngIf="errorMessage" class="error-title">{{ errorMessage }}</strong>
                <ul *ngIf="errorList?.length" class="error-list">
                  <li *ngFor="let e of errorList">{{ e }}</li>
                </ul>
              </div>
              <button type="button" class="error-close" (click)="clearError()" aria-label="Dismiss error">&times;</button>
            </div>
          </div>

          <div *ngIf="successMessage" class="success-box" role="alert" aria-live="polite">
            <div class="success-box-inner">
              <span class="success-icon" aria-hidden="true">✓</span>
              <div class="success-content">
                <strong class="success-title">{{ successMessage }}</strong>
              </div>
            </div>
          </div>

          <button 
            type="submit" 
            class="btn-primary"
            [disabled]="registerForm.invalid || loading"
          >
            {{ loading ? 'Creating Account...' : 'Create Account' }}
          </button>
        </form>

        <div class="login-link">
          <p>Already have an account? <a routerLink="/login">Sign in here</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: #f8f9fa;
      padding: 20px;
    }

    .register-card {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
      padding: 40px;
      width: 100%;
      max-width: 500px;
    }

    .logo {
      text-align: center;
      margin-bottom: 30px;
    }

    .logo h1 {
      margin: 0 0 8px 0;
      color: #1a73e8;
      font-size: 28px;
      font-weight: 400;
    }

    .logo p {
      margin: 0;
      color: #5f6368;
      font-size: 16px;
    }

    .register-form {
      margin-bottom: 24px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .form-group {
      margin-bottom: 20px;
    }

    label {
      display: block;
      margin-bottom: 6px;
      color: #202124;
      font-size: 14px;
      font-weight: 500;
    }

    .form-input {
      width: 100%;
      padding: 12px 16px;
      border: 1px solid #dadce0;
      border-radius: 4px;
      font-size: 16px;
      transition: border-color 0.2s;
      box-sizing: border-box;
    }

    .form-input:focus {
      outline: none;
      border-color: #1a73e8;
    }

    .form-input.error {
      border-color: #d93025;
    }

    .error-message {
      color: #d93025;
      font-size: 12px;
      margin-top: 4px;
    }

    .error-box {
      background-color: #fef2f2;
      border: 1px solid #fca5a5;
      border-radius: 8px;
      padding: 0;
      margin-bottom: 20px;
      animation: slideIn 0.3s ease-out;
    }

    .success-box {
      background-color: #f0fdf4;
      border: 1px solid #86efac;
      border-radius: 8px;
      padding: 0;
      margin-bottom: 20px;
      animation: slideIn 0.3s ease-out;
    }

    @keyframes slideIn {
      from {
        opacity: 0;
        transform: translateY(-10px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .error-box-inner,
    .success-box-inner {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 16px;
    }

    .error-icon,
    .success-icon {
      font-size: 20px;
      flex-shrink: 0;
      line-height: 1;
    }

    .error-content,
    .success-content {
      flex: 1;
      min-width: 0;
    }

    .error-title {
      display: block;
      color: #991b1b;
      font-size: 14px;
      font-weight: 600;
      margin: 0 0 8px 0;
      line-height: 1.4;
    }

    .success-title {
      display: block;
      color: #166534;
      font-size: 14px;
      font-weight: 600;
      margin: 0;
      line-height: 1.4;
    }

    .error-list {
      margin: 0;
      padding-left: 20px;
      color: #dc2626;
      font-size: 13px;
      line-height: 1.6;
    }

    .error-list li {
      margin-bottom: 4px;
    }

    .error-list li:last-child {
      margin-bottom: 0;
    }

    .error-close {
      background: none;
      border: none;
      color: #991b1b;
      font-size: 24px;
      line-height: 1;
      cursor: pointer;
      padding: 0;
      width: 24px;
      height: 24px;
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 4px;
      transition: background-color 0.2s;
    }

    .error-close:hover {
      background-color: rgba(153, 27, 27, 0.1);
    }

    .error-close:focus {
      outline: 2px solid #991b1b;
      outline-offset: 2px;
    }

    .btn-primary {
      width: 100%;
      padding: 12px 16px;
      background-color: #1a73e8;
      color: white;
      border: none;
      border-radius: 4px;
      font-size: 16px;
      font-weight: 500;
      cursor: pointer;
      transition: background-color 0.2s;
    }

    .btn-primary:hover:not(:disabled) {
      background-color: #1557b0;
    }

    .btn-primary:disabled {
      background-color: #80868b;
      cursor: not-allowed;
    }

    .login-link {
      text-align: center;
      padding-top: 20px;
      border-top: 1px solid #dadce0;
    }

    .login-link p {
      margin: 0;
      color: #5f6368;
      font-size: 14px;
    }

    .login-link a {
      color: #1a73e8;
      text-decoration: none;
    }

    .login-link a:hover {
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  errorMessage = '';
  errorList: string[] = [];
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      businessName: [''],
      phoneNumber: ['']
    });
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.loading = true;
      this.errorMessage = '';
      this.errorList = [];
      this.successMessage = '';

      this.authService.register(this.registerForm.value)
        .pipe(finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        }))
        .subscribe({
          next: (response) => {
            if (response && response.success) {
              // Successful registration
              this.successMessage = 'Account created successfully! Redirecting to dashboard...';
              this.cdr.detectChanges();
              setTimeout(() => {
                this.router.navigate(['/dashboard']);
              }, 1500);
              return;
            }

            // API returned a structured ApiResponse with success=false
            if (response) {
              this.errorMessage = response.message || 'Registration failed';
              this.errorList = response.errors || [];
            } else {
              this.errorMessage = 'Registration failed. Please try again.';
              this.errorList = [];
            }
            this.cdr.detectChanges();
          },
          error: (err) => {
            // Network or unexpected server error
            if (err && err.error && typeof err.error === 'object') {
              this.errorMessage = err.error.message || 'Registration failed';
              this.errorList = err.error.errors || [];
            } else if (err && err.status === 0) {
              this.errorMessage = 'Unable to connect to server. Please check your connection';
              this.errorList = [];
            } else {
              this.errorMessage = err?.message || 'An error occurred during registration';
              this.errorList = [];
            }
            this.cdr.detectChanges();
          }
        });
    }
  }

  clearError(): void {
    this.errorMessage = '';
    this.errorList = [];
    this.cdr.detectChanges();
  }
}
