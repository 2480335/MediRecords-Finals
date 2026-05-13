import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);
  readonly year = new Date().getFullYear();

  private returnUrl: string | null = null;

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');

    if (this.auth.isAuthenticated()) {
      this.redirectAfterLogin();
    }
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  submit(): void {
    this.errorMessage.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.loading.set(false);
        this.redirectAfterLogin();
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.errorMessage.set(this.extractError(err));
      }
    });
  }

  private redirectAfterLogin(): void {
    if (this.returnUrl && !this.returnUrl.startsWith('/login')) {
      this.router.navigateByUrl(this.returnUrl);
      return;
    }

    const role = this.auth.role();
    if (!role) {
      // Logged in but the JWT did not contain a recognized role — bail out
      // with a clear message instead of silently bouncing back to /login.
      this.auth.logout(false);
      this.errorMessage.set(
        "Your account doesn't have a recognized role. Please contact an administrator."
      );
      return;
    }

    this.router.navigateByUrl(this.auth.dashboardRouteForRole(role));
  }

  private extractError(err: HttpErrorResponse): string {
    if (err.status === 0) {
      return 'Cannot reach the server. Please check your connection.';
    }
    if (err.status === 400) {
      return err.error?.message ?? 'Please enter valid credentials.';
    }
    if (err.status === 401) {
      return err.error?.message ?? 'Invalid email or password.';
    }
    return err.error?.message ?? 'Login failed. Please try again.';
  }
}
