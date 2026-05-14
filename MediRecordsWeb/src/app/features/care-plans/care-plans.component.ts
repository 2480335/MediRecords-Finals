import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { CarePlanService } from '../../core/services/care-plan.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import { SearchableSelectComponent, SearchableOption } from '../../shared/components/searchable-select/searchable-select.component';
import {
  CarePlanCreateRequest,
  CarePlanDetails
} from '../../core/models/care-plan.models';
import { PatientLookup } from '../../core/models/patient.models';

type StatusFilter = 'all' | 'active' | 'completed';

@Component({
  selector: 'app-care-plans',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './care-plans.component.html',
  styleUrl: './care-plans.component.scss'
})
export class CarePlansComponent implements OnInit {
  private readonly api = inject(CarePlanService);
  private readonly patientApi = inject(PatientService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly canCreate = computed(() => this.auth.role() === 'Physician');
  readonly patients = signal<PatientLookup[]>([]);
  readonly patientOptions = computed<SearchableOption[]>(() =>
    this.patients().map((p) => ({ value: p.patientId, label: p.name }))
  );

  readonly plans = signal<CarePlanDetails[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly statusFilter = signal<StatusFilter>('all');
  readonly selected = signal<CarePlanDetails | null>(null);
  readonly detailLoading = signal(false);

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const status = this.statusFilter();
    return this.plans().filter((p) => {
      if (status === 'active' && p.status) return false;
      if (status === 'completed' && !p.status) return false;
      if (term) {
        const blob = `${p.patientName} ${p.patientId} ${p.instructions}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  readonly stats = computed(() => {
    const all = this.plans();
    const active = all.filter((p) => !p.status).length;
    const completed = all.length - active;
    return { total: all.length, active, completed };
  });

  /* ── Create modal ── */
  readonly showCreate = signal(false);
  readonly saving = signal(false);
  readonly newGoal = signal('');
  readonly goals = signal<string[]>([]);

  readonly createForm = this.fb.nonNullable.group({
    patientId: [0, [Validators.required, Validators.min(1)]],
    instructions: ['', [Validators.required, Validators.maxLength(2000)]],
    status: [false]
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
    this.api.list({}).subscribe({
      next: (list) => {
        this.plans.set(this.parseGoals(list));
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 404) {
          this.plans.set([]);
        } else {
          this.toast.error('Could not load care plans');
        }
      }
    });
  }

  setSearch(v: string): void {
    this.searchTerm.set(v);
  }

  setStatus(v: StatusFilter): void {
    this.statusFilter.set(v);
  }

  open(plan: CarePlanDetails): void {
    this.detailLoading.set(true);
    this.selected.set(plan); // optimistic
    this.api.getById(plan.carePlanId).subscribe({
      next: (full) => {
        this.selected.set(this.parseGoals([full])[0]);
        this.detailLoading.set(false);
      },
      error: () => {
        this.detailLoading.set(false);
      }
    });
  }

  close(): void {
    this.selected.set(null);
  }

  parsedGoals(plan: CarePlanDetails | null): string[] {
    if (!plan) return [];
    const raw = plan.goalsJSON;
    if (!raw) return [];
    try {
      const parsed = JSON.parse(raw);
      if (Array.isArray(parsed)) return parsed.map(String);
    } catch {
      /* fall through */
    }
    return raw
      .split(/[,;|\n]+/)
      .map((s) => s.trim())
      .filter(Boolean);
  }

  private parseGoals(list: CarePlanDetails[]): CarePlanDetails[] {
    return list;
  }

  /* ── Create flow ── */
  openCreate(): void {
    this.createForm.reset({ patientId: 0, instructions: '', status: false });
    this.goals.set([]);
    this.newGoal.set('');
    this.showCreate.set(true);
  }

  closeCreate(): void {
    this.showCreate.set(false);
  }

  setNewGoal(v: string): void {
    this.newGoal.set(v);
  }

  addGoal(): void {
    const value = this.newGoal().trim();
    if (!value) return;
    if (this.goals().includes(value)) {
      this.toast.info('That goal is already in the list.');
      return;
    }
    this.goals.update((arr) => [...arr, value]);
    this.newGoal.set('');
  }

  removeGoal(idx: number): void {
    this.goals.update((arr) => arr.filter((_, i) => i !== idx));
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    if (this.goals().length === 0) {
      this.toast.error('Add at least one goal.');
      return;
    }
    const v = this.createForm.getRawValue();
    const payload: CarePlanCreateRequest = {
      patientId: Number(v.patientId),
      goals: [...this.goals()],
      instructions: v.instructions,
      status: !!v.status
    };
    this.saving.set(true);
    this.api.create(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Care plan created');
        this.closeCreate();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not create care plan'));
      }
    });
  }

  initials(name: string | undefined | null): string {
    if (!name) return '?';
    return name
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
