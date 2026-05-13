import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { jwtDecode } from 'jwt-decode';
import { AuthService } from '../../core/services/auth.service';
import { PatientService } from '../../core/services/patient.service';
import { TokenService } from '../../core/services/token.service';
import { ToastService } from '../../core/services/toast.service';
import {
  GENDER_OPTIONS,
  PATIENT_STATUSES,
  PATIENT_STATUS_VALUE,
  PatientCreateRequest,
  PatientDetails,
  PatientLookup,
  PatientStatusName,
  PatientUpdateRequest,
  formatAddress,
  parseAddress
} from '../../core/models/patient.models';

@Component({
  selector: 'app-patients',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './patients.component.html',
  styleUrl: './patients.component.scss'
})
export class PatientsComponent implements OnInit {
  private readonly api = inject(PatientService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly tokenService = inject(TokenService);
  private readonly fb = inject(FormBuilder);

  readonly canManage = computed(() => this.auth.role() === 'FrontDesk');
  readonly genders = GENDER_OPTIONS;
  readonly statuses = PATIENT_STATUSES;

  readonly patientIdControl = new FormControl<string>('', {
    nonNullable: true,
    validators: [Validators.required, Validators.pattern(/^\s*\d+\s*$/)]
  });

  readonly patient = signal<PatientDetails | null>(null);
  readonly loading = signal(false);
  readonly hasSearched = signal(false);
  readonly lookupError = signal<string | null>(null);

  /* ── List + search ── */
  readonly patients = signal<PatientLookup[]>([]);
  readonly listLoading = signal(true);
  readonly searchTerm = signal('');
  readonly statusFilter = signal<'all' | 'Active' | 'Inactive' | 'Deceased'>('all');

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const status = this.statusFilter();
    return this.patients().filter((p) => {
      if (status !== 'all' && this.normalizeStatus(p.status) !== status) return false;
      if (term) {
        const blob = `${p.patientId} ${p.name} ${p.mrn} ${p.phoneNo}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  ngOnInit(): void {
    this.refreshList();
  }

  refreshList(): void {
    this.listLoading.set(true);
    this.api.getAll().subscribe({
      next: (list) => {
        this.patients.set(list ?? []);
        this.listLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.patients.set([]);
        this.listLoading.set(false);
        if (err.status !== 404) {
          this.toast.error(this.errorText(err, 'Could not load patient list'));
        }
      }
    });
  }

  setSearch(v: string): void {
    this.searchTerm.set(v);
  }

  setStatusFilter(v: 'all' | 'Active' | 'Inactive' | 'Deceased'): void {
    this.statusFilter.set(v);
  }

  openPatientFromRow(p: PatientLookup): void {
    this.patientIdControl.setValue(String(p.patientId));
    this.loadPatient(p.patientId);
  }

  /* ── Create modal ── */
  readonly showCreate = signal(false);
  readonly saving = signal(false);

  readonly createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(255)]],
    dob: ['', Validators.required],
    gender: ['Male', Validators.required],
    phoneNo: ['', [Validators.required, Validators.maxLength(15)]],
    contactInfo: [''],
    primaryProviderId: [''],
    line1: [''],
    line2: [''],
    city: [''],
    state: [''],
    postalCode: [''],
    country: ['']
  });

  /* ── Edit modal ── */
  readonly showEdit = signal(false);
  readonly editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(255)]],
    dob: ['', Validators.required],
    gender: [''],
    phoneNo: ['', [Validators.required, Validators.maxLength(15)]],
    primaryProviderId: [''],
    status: ['Active' as PatientStatusName, Validators.required],
    line1: [''],
    line2: [''],
    city: [''],
    state: [''],
    postalCode: [''],
    country: ['']
  });

  /* ── Lookup ── */
  search(): void {
    if (this.patientIdControl.invalid) {
      this.patientIdControl.markAsTouched();
      return;
    }
    const id = Number((this.patientIdControl.value ?? '').trim());
    if (!Number.isFinite(id) || id <= 0) {
      this.patientIdControl.markAsTouched();
      return;
    }
    this.loadPatient(id);
  }

  reset(): void {
    this.patientIdControl.reset('');
    this.patient.set(null);
    this.hasSearched.set(false);
    this.lookupError.set(null);
  }

  loadPatient(id: number): void {
    this.loading.set(true);
    this.hasSearched.set(true);
    this.lookupError.set(null);

    this.api.getById(id).subscribe({
      next: (data) => {
        // eslint-disable-next-line no-console
        console.log('[Patients] GET /Patient/' + id + ' OK', data);
        if (!data || (data as PatientDetails).patientId == null) {
          this.patient.set(null);
          this.lookupError.set(
            `The server returned a 200 OK but no patient body. ` +
            `The API may have responded with an empty payload.`
          );
        } else {
          this.patient.set(data);
        }
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.patient.set(null);
        this.loading.set(false);
        // eslint-disable-next-line no-console
        console.error('[Patients] GET /Patient/' + id + ' failed', {
          status: err.status,
          statusText: err.statusText,
          body: err.error,
          url: err.url
        });
        this.lookupError.set(this.describeLookupError(err, id));
      }
    });
  }

  private describeLookupError(err: HttpErrorResponse, id: number): string {
    if (err.status === 0) {
      return 'Cannot reach the API. Check that the backend is running.';
    }
    if (err.status === 401) {
      return 'Your session has expired. Please sign in again.';
    }
    if (err.status === 403) {
      return "You don't have permission to view patients. Sign in as Front Desk, Physician, or Nurse.";
    }
    if (err.status === 404) {
      return `No patient found with ID #${id}.`;
    }
    if (err.status === 400) {
      return this.errorText(err, 'Invalid patient ID.');
    }
    if (err.status >= 500) {
      const detail = this.errorText(err, '');
      return detail
        ? `Server error while loading patient #${id}: ${detail}`
        : `Server error while loading patient #${id} (HTTP ${err.status}). Check backend logs.`;
    }
    return this.errorText(err, `Could not load patient (HTTP ${err.status}).`);
  }

  /* ── Create ── */
  openCreate(): void {
    this.createForm.reset({
      name: '',
      dob: '',
      gender: 'Male',
      phoneNo: '',
      contactInfo: '',
      primaryProviderId: '',
      line1: '',
      line2: '',
      city: '',
      state: '',
      postalCode: '',
      country: ''
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
    const addr = this.buildAddressJson({
      line1: v.line1, line2: v.line2,
      city: v.city, state: v.state,
      postalCode: v.postalCode, country: v.country
    });
    const payload: PatientCreateRequest = {
      name: v.name,
      dob: v.dob,
      gender: v.gender,
      phoneNo: v.phoneNo,
      contactInfo: v.contactInfo || null,
      addressJSON: addr,
      primaryProviderId: v.primaryProviderId ? Number(v.primaryProviderId) : null
    };
    this.saving.set(true);
    this.api.create(payload).subscribe({
      next: (res) => {
        this.saving.set(false);
        this.toast.success(`Patient registered (ID #${res.patientId})`);
        this.closeCreate();
        this.patientIdControl.setValue(String(res.patientId));
        this.loadPatient(res.patientId);
        this.refreshList();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        const rawClaims = this.dumpJwtClaims();
        // eslint-disable-next-line no-console
        console.error('[Patients] POST /Patient failed', {
          status: err.status,
          body: err.error,
          normalizedRole: this.auth.role(),
          rawJwtClaims: rawClaims
        });
        this.toast.error(this.describeCreateError(err, rawClaims));
      }
    });
  }

  /** Decode the access token and return its claims (for debugging). */
  private dumpJwtClaims(): Record<string, unknown> | null {
    const token = this.tokenService.getAccessToken();
    if (!token) return null;
    try {
      return jwtDecode<Record<string, unknown>>(token);
    } catch {
      return null;
    }
  }

  private describeCreateError(
    err: HttpErrorResponse,
    rawClaims: Record<string, unknown> | null = null
  ): string {
    if (err.status === 0) {
      return 'Cannot reach the API. Check that the backend is running.';
    }
    if (err.status === 401) return 'Your session has expired. Please sign in again.';
    if (err.status === 403) {
      const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
      const rawRole = rawClaims
        ? (rawClaims[ROLE_CLAIM] ?? (rawClaims as { role?: unknown }).role)
        : undefined;
      const rawText = rawRole === undefined ? '<not set>' : JSON.stringify(rawRole);
      return `Forbidden by backend (only Front Desk can register patients). ` +
             `Raw JWT role claim is ${rawText}. ` +
             `The backend is case-sensitive — it requires exactly "FrontDesk". ` +
             `Open DevTools console for the full token decode, then fix UserRole.Name in the DB and sign in again.`;
    }
    if (err.status === 409) {
      return this.errorText(err, 'A patient with these details already exists.');
    }
    if (err.status === 400) {
      return this.errorText(err, 'Some fields are invalid — please review.');
    }
    if (err.status >= 500) {
      const detail = this.errorText(err, '');
      return detail
        ? `Server error: ${detail}`
        : `Server error (HTTP ${err.status}). Check backend logs.`;
    }
    return this.errorText(err, `Could not register patient (HTTP ${err.status}).`);
  }

  /* ── Edit ── */
  openEdit(): void {
    const p = this.patient();
    if (!p) return;
    const addr = parseAddress(p.addressJSON ?? null);
    this.editForm.reset({
      name: p.name,
      dob: this.toIsoDate(p.dob),
      gender: p.gender ?? '',
      phoneNo: p.phoneNo ?? '',
      primaryProviderId: '',
      status: this.normalizeStatus(p.status),
      line1: addr?.line1 ?? '',
      line2: addr?.line2 ?? '',
      city: addr?.city ?? '',
      state: addr?.state ?? '',
      postalCode: addr?.postalCode ?? '',
      country: addr?.country ?? ''
    });
    this.showEdit.set(true);
  }

  closeEdit(): void {
    this.showEdit.set(false);
  }

  submitEdit(): void {
    const p = this.patient();
    if (!p || this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    const v = this.editForm.getRawValue();
    const addr = this.buildAddressJson({
      line1: v.line1, line2: v.line2,
      city: v.city, state: v.state,
      postalCode: v.postalCode, country: v.country
    });
    const payload: PatientUpdateRequest = {
      name: v.name,
      dob: v.dob,
      gender: v.gender || null,
      phoneNo: v.phoneNo,
      addressJSON: addr,
      primaryProviderId: v.primaryProviderId ? Number(v.primaryProviderId) : null,
      status: PATIENT_STATUS_VALUE[v.status]
    };
    this.saving.set(true);
    this.api.update(p.patientId, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Patient updated');
        this.closeEdit();
        this.loadPatient(p.patientId);
        this.refreshList();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not update patient'));
      }
    });
  }

  /* ── Helpers ── */
  initials(name: string | null | undefined): string {
    if (!name) return '?';
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((s) => s.charAt(0).toUpperCase())
      .join('');
  }

  age(dob: string | null | undefined): number | null {
    if (!dob) return null;
    const d = new Date(dob);
    if (Number.isNaN(d.getTime())) return null;
    const now = new Date();
    let years = now.getFullYear() - d.getFullYear();
    const m = now.getMonth() - d.getMonth();
    if (m < 0 || (m === 0 && now.getDate() < d.getDate())) years--;
    return years;
  }

  toneFor(status: string): string {
    return PATIENT_STATUSES.find((s) => s.value === this.normalizeStatus(status))?.tone ?? 'muted';
  }

  formattedAddress(json: string | null | undefined): string {
    return formatAddress(parseAddress(json ?? null));
  }

  hasAddress(json: string | null | undefined): boolean {
    return !!this.formattedAddress(json);
  }

  private normalizeStatus(status: string | undefined | null): PatientStatusName {
    const s = (status ?? '').toString().trim().toLowerCase();
    if (s === 'active' || s === '1') return 'Active';
    if (s === 'inactive' || s === '0') return 'Inactive';
    if (s === 'deceased' || s === '2') return 'Deceased';
    return 'Active';
  }

  private toIsoDate(input: string): string {
    if (!input) return '';
    // Already ISO date YYYY-MM-DD?
    if (/^\d{4}-\d{2}-\d{2}/.test(input)) return input.slice(0, 10);
    const d = new Date(input);
    if (Number.isNaN(d.getTime())) return '';
    return d.toISOString().slice(0, 10);
  }

  private buildAddressJson(parts: {
    line1?: string; line2?: string;
    city?: string; state?: string;
    postalCode?: string; country?: string;
  }): string | null {
    const cleaned: Record<string, string> = {};
    Object.entries(parts).forEach(([k, v]) => {
      const val = (v ?? '').toString().trim();
      if (val) cleaned[k] = val;
    });
    if (Object.keys(cleaned).length === 0) return null;
    return JSON.stringify(cleaned);
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
