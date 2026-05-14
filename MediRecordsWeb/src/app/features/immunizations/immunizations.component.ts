import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { ImmunizationService } from '../../core/services/immunization.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import { SearchableSelectComponent, SearchableOption } from '../../shared/components/searchable-select/searchable-select.component';
import {
  COMMON_VACCINES,
  ImmunizationCreateRequest,
  ImmunizationDetails,
  ImmunizationFilter
} from '../../core/models/immunization.models';
import { PatientLookup } from '../../core/models/patient.models';

type StatusFilter = 'all' | 'pending' | 'administered';

@Component({
  selector: 'app-immunizations',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './immunizations.component.html',
  styleUrl: './immunizations.component.scss'
})
export class ImmunizationsComponent implements OnInit {
  private readonly api = inject(ImmunizationService);
  private readonly patientApi = inject(PatientService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly canCreate = computed(() => this.auth.role() === 'Physician');
  readonly commonVaccines = COMMON_VACCINES;
  readonly patients = signal<PatientLookup[]>([]);
  readonly patientOptions = computed<SearchableOption[]>(() =>
    this.patients().map((p) => ({ value: p.patientId, label: p.name }))
  );

  readonly records = signal<ImmunizationDetails[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly statusFilter = signal<StatusFilter>('all');

  readonly selected = signal<ImmunizationDetails | null>(null);
  readonly detailLoading = signal(false);

  readonly filterForm = this.fb.nonNullable.group({
    patientId: [''],
    patientName: [''],
    vaccine: ['']
  });

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const status = this.statusFilter();
    return this.records().filter((r) => {
      if (status === 'pending' && r.status) return false;
      if (status === 'administered' && !r.status) return false;
      if (term) {
        const blob = `${r.patientName} ${r.patientId} ${r.vaccine} ${r.dose}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  readonly stats = computed(() => {
    const all = this.records();
    const administered = all.filter((r) => r.status).length;
    return {
      total: all.length,
      administered,
      pending: all.length - administered
    };
  });

  /* ── Create modal ── */
  readonly showCreate = signal(false);
  readonly saving = signal(false);

  readonly createForm = this.fb.nonNullable.group({
    patientId: [0, [Validators.required, Validators.min(1)]],
    vaccine: ['', [Validators.required, Validators.maxLength(255)]],
    dose: ['', [Validators.required, Validators.maxLength(50)]],
    givenDate: ['', Validators.required],
    status: [true]
  });

  ngOnInit(): void {
    this.refresh();
    this.patientApi.getAll().subscribe({
      next: (list) => this.patients.set(list),
      error: () => this.toast.error('Could not load patient list')
    });
  }

  refresh(): void {
    this.loading.set(true);
    const v = this.filterForm.getRawValue();
    const filter: ImmunizationFilter = {
      patientId: v.patientId ? Number(v.patientId) : null,
      patientName: v.patientName || null,
      vaccine: v.vaccine || null
    };
    this.api.list(filter).subscribe({
      next: (list) => {
        this.records.set(list ?? []);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.records.set([]);
        this.loading.set(false);
        if (err.status !== 404) {
          this.toast.error(this.errorText(err, 'Could not load immunizations'));
        }
      }
    });
  }

  applyFilters(): void {
    this.refresh();
  }

  resetFilters(): void {
    this.filterForm.reset({ patientId: '', patientName: '', vaccine: '' });
    this.refresh();
  }

  setSearch(v: string): void {
    this.searchTerm.set(v);
  }

  setStatus(v: StatusFilter): void {
    this.statusFilter.set(v);
  }

  /* ── Detail ── */
  open(rec: ImmunizationDetails): void {
    this.selected.set(rec);
    this.detailLoading.set(true);
    this.api.getById(rec.immunizationId).subscribe({
      next: (full) => {
        this.selected.set(full);
        this.detailLoading.set(false);
      },
      error: () => this.detailLoading.set(false)
    });
  }

  close(): void {
    this.selected.set(null);
  }

  /* ── Create ── */
  openCreate(): void {
    const today = new Date().toISOString().slice(0, 10);
    this.createForm.reset({
      patientId: 0,
      vaccine: '',
      dose: '',
      givenDate: today,
      status: true
    });
    this.showCreate.set(true);
  }

  closeCreate(): void {
    this.showCreate.set(false);
  }

  pickVaccine(name: string): void {
    this.createForm.patchValue({ vaccine: name });
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    const v = this.createForm.getRawValue();
    const isoDate = new Date(`${v.givenDate}T09:00:00`).toISOString();
    const payload: ImmunizationCreateRequest = {
      vaccine: v.vaccine,
      dose: v.dose,
      givenDate: isoDate,
      status: !!v.status
    };
    this.saving.set(true);
    this.api.create(Number(v.patientId), payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Immunization recorded');
        this.closeCreate();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not record immunization'));
      }
    });
  }

  initials(name: string | null | undefined): string {
    if (!name) return '?';
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((s) => s.charAt(0).toUpperCase())
      .join('');
  }

  vaccineIcon(vaccine: string): string {
    const v = (vaccine ?? '').toLowerCase();
    if (v.includes('covid')) return 'bi-virus';
    if (v.includes('flu') || v.includes('influenza')) return 'bi-wind';
    if (v.includes('hep')) return 'bi-droplet';
    if (v.includes('mmr') || v.includes('measle') || v.includes('mump')) return 'bi-bandaid';
    if (v.includes('hpv')) return 'bi-shield-shaded';
    if (v.includes('polio')) return 'bi-shield-plus';
    return 'bi-shield-check';
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
