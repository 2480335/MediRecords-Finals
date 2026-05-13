import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AppointmentService } from '../../core/services/appointment.service';
import { AuthService } from '../../core/services/auth.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import { UserService } from '../../core/services/user.service';
import {
  APPOINTMENT_STATUSES,
  AppointmentRequest,
  AppointmentResponse,
  AppointmentStatusName
} from '../../core/models/appointment.models';
import { PatientLookup } from '../../core/models/patient.models';
import { ProviderLookup } from '../../core/models/user.models';

const STATUS_BY_VALUE: Record<number, AppointmentStatusName> = {
  1: 'Booked',
  2: 'CheckedIn',
  3: 'Completed',
  4: 'Cancelled',
  5: 'NoShow'
};

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './appointments.component.html',
  styleUrl: './appointments.component.scss'
})
export class AppointmentsComponent implements OnInit {
  private readonly api = inject(AppointmentService);
  private readonly patientApi = inject(PatientService);
  private readonly userApi = inject(UserService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly statuses = APPOINTMENT_STATUSES;
  readonly patients = signal<PatientLookup[]>([]);
  readonly providers = signal<ProviderLookup[]>([]);

  // Booking appointments is FrontDesk-only on the backend. Mirror that
  // in the UI so other roles don't see a button that would 403.
  readonly canBook = computed(() => this.auth.role() === 'FrontDesk');

  readonly appointments = signal<AppointmentResponse[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);

  readonly filters = this.fb.nonNullable.group({
    date: [''],
    patientId: [''],
    providerId: ['']
  });

  readonly statusFilter = signal<AppointmentStatusName | 'all'>('all');

  /* Modal state */
  readonly showCreate = signal(false);
  readonly showStatus = signal(false);
  readonly statusTarget = signal<AppointmentResponse | null>(null);
  readonly chosenStatus = signal<AppointmentStatusName>('CheckedIn');

  readonly createForm = this.fb.nonNullable.group({
    patientId: [0, [Validators.required, Validators.min(1)]],
    providerId: [0, [Validators.required, Validators.min(1)]],
    date: ['', Validators.required],
    time: ['', Validators.required],
    reason: ['']
  });

  readonly filtered = computed(() => {
    const status = this.statusFilter();
    if (status === 'all') return this.appointments();
    return this.appointments().filter((a) => this.statusName(a) === status);
  });

  readonly stats = computed(() => {
    const all = this.appointments();
    const total = all.length;
    const booked = all.filter((a) => this.statusName(a) === 'Booked').length;
    const checkedIn = all.filter((a) => this.statusName(a) === 'CheckedIn').length;
    const completed = all.filter((a) => this.statusName(a) === 'Completed').length;
    return { total, booked, checkedIn, completed };
  });

  ngOnInit(): void {
    const today = new Date().toISOString().slice(0, 10);
    this.filters.patchValue({ date: today });
    this.load();
    this.patientApi.getAll().subscribe({
      next: (list) => this.patients.set(list),
      error: () => this.toast.error('Could not load patient list')
    });
    this.userApi.getProviders().subscribe({
      next: (list) => this.providers.set(list),
      error: () => this.toast.error('Could not load provider list')
    });
  }

  load(): void {
    this.loading.set(true);
    const v = this.filters.getRawValue();
    this.api
      .list({
        date: v.date || null,
        patientId: v.patientId ? Number(v.patientId) : null,
        providerId: v.providerId ? Number(v.providerId) : null
      })
      .subscribe({
        next: (list) => {
          this.appointments.set(list ?? []);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.toast.error('Could not load appointments');
        }
      });
  }

  applyFilters(): void {
    this.load();
  }

  resetFilters(): void {
    this.filters.reset({ date: '', patientId: '', providerId: '' });
    this.load();
  }

  setStatusFilter(value: AppointmentStatusName | 'all'): void {
    this.statusFilter.set(value);
  }

  /* ─── Book ─── */
  openCreate(): void {
    const today = new Date();
    today.setMinutes(today.getMinutes() - today.getTimezoneOffset());
    const isoDate = today.toISOString().slice(0, 10);
    this.createForm.reset({
      patientId: 0,
      providerId: 0,
      date: isoDate,
      time: '09:00',
      reason: ''
    });
    this.showCreate.set(true);
  }

  closeCreate(): void {
    this.showCreate.set(false);
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    const v = this.createForm.getRawValue();
    const dateTimeIso = `${v.date}T${v.time}:00`;
    const payload: AppointmentRequest = {
      patientId: Number(v.patientId),
      providerId: Number(v.providerId),
      dateTime: dateTimeIso,
      reason: v.reason || null
    };
    this.saving.set(true);
    this.api.book(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Appointment booked');
        this.closeCreate();
        this.filters.patchValue({ date: v.date });
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not book appointment'));
      }
    });
  }

  /* ─── Status update ─── */
  openStatus(appointment: AppointmentResponse): void {
    this.statusTarget.set(appointment);
    const current = this.statusName(appointment);
    const next: AppointmentStatusName =
      current === 'Booked'
        ? 'CheckedIn'
        : current === 'CheckedIn'
        ? 'Completed'
        : 'Booked';
    this.chosenStatus.set(next);
    this.showStatus.set(true);
  }

  closeStatus(): void {
    this.showStatus.set(false);
    this.statusTarget.set(null);
  }

  submitStatus(): void {
    const target = this.statusTarget();
    if (!target) return;
    const chosen = this.chosenStatus();
    this.saving.set(true);
    this.api.updateStatus(target.appointmentId, chosen, {}).subscribe({
      next: (resp) => {
        this.saving.set(false);
        const encounterId = resp?.encounterId ?? null;
        const me = this.auth.user();
        const isOwnPhysicianCheckIn =
          chosen === 'CheckedIn' &&
          encounterId != null &&
          me?.role === 'Physician' &&
          me.userId === target.providerId;

        if (isOwnPhysicianCheckIn) {
          this.toast.success(`Encounter #${encounterId} is now open. Opening chart…`);
          this.closeStatus();
          const apptDate = (target.dateTime ?? '').slice(0, 10);
          this.router.navigate(['/workspace'], {
            queryParams: { encounterId, date: apptDate || null }
          });
          return;
        }

        if (chosen === 'CheckedIn' && encounterId != null) {
          this.toast.success(
            `Patient checked in. Encounter #${encounterId} is now open.`
          );
        } else {
          this.toast.success(`Status changed to ${chosen}`);
        }
        this.closeStatus();
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not update status'));
      }
    });
  }

  setChosenStatus(s: AppointmentStatusName): void {
    this.chosenStatus.set(s);
  }

  /* ─── Helpers ─── */
  statusName(a: AppointmentResponse): AppointmentStatusName {
    if (typeof a.status === 'number') return STATUS_BY_VALUE[a.status] ?? 'Booked';
    return (a.status as AppointmentStatusName) ?? 'Booked';
  }

  toneFor(status: AppointmentStatusName): string {
    return APPOINTMENT_STATUSES.find((s) => s.value === status)?.tone ?? 'muted';
  }

  initials(label: string | undefined): string {
    if (!label) return '?';
    return label
      .toString()
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((s) => s.charAt(0).toUpperCase())
      .join('');
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
