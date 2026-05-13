import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

const STRONG_PASSWORD = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

const matchPasswordsValidator: ValidatorFn = (
  group: AbstractControl
): ValidationErrors | null => {
  const newPwd = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return newPwd && confirm && newPwd !== confirm ? { mismatch: true } : null;
};

@Component({
  selector: 'app-update-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './update-password.component.html',
  styleUrl: './update-password.component.scss'
})
export class UpdatePasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  // True when the page is opened by a logged-in user (header menu, etc.).
  // False when reached anonymously from the login page via "Forgot password?".
  readonly isAuthenticated = computed(() => !!this.auth.user());

  readonly form = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      newPassword: [
        '',
        [Validators.required, Validators.minLength(8), Validators.pattern(STRONG_PASSWORD)]
      ],
      confirmPassword: ['', [Validators.required]]
    },
    { validators: matchPasswordsValidator }
  );

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly showNew = signal(false);
  readonly showConfirm = signal(false);

  constructor() {
    // Logged-in users: pre-fill and lock email to their own account.
    // Anonymous users: leave it editable so they can type the account email.
    const currentEmail = this.auth.user()?.email;
    if (currentEmail) {
      this.form.controls.email.setValue(currentEmail);
      this.form.controls.email.disable();
    }
  }

  toggleNew(): void {
    this.showNew.update((v) => !v);
  }

  toggleConfirm(): void {
    this.showConfirm.update((v) => !v);
  }

  submit(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    // getRawValue() also returns disabled controls' values, so this works
    // for both the locked (logged-in) and editable (anonymous) flows.
    const { email, newPassword, confirmPassword } = this.form.getRawValue();

    this.auth
      .forgotPassword({ email, newPassword, confirmPassword })
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.successMessage.set('Password updated. Please sign in with your new password.');
          setTimeout(() => {
            if (this.isAuthenticated()) {
              this.auth.logout();
            } else {
              this.router.navigateByUrl('/login');
            }
          }, 1500);
        },
        error: (err: HttpErrorResponse) => {
          this.loading.set(false);
          this.errorMessage.set(this.extractError(err));
        }
      });
  }

  cancel(): void {
    if (this.isAuthenticated()) {
      this.router.navigateByUrl(this.auth.dashboardRouteForRole(this.auth.role()));
    } else {
      this.router.navigateByUrl('/login');
    }
  }

  private extractError(err: HttpErrorResponse): string {
    if (err.status === 0) {
      return 'Cannot reach the server. Please try again.';
    }
    if (err.status === 404) {
      return 'No account found with that email.';
    }
    return err.error?.message ?? 'Could not update password. Please try again.';
  }
}
