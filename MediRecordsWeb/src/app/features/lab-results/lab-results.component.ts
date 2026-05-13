import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { LabResultService } from '../../core/services/lab-result.service';
import { ToastService } from '../../core/services/toast.service';
import {
  LabResultEntry,
  LabResultRequest,
  LabResultResponse,
  parseResultJson
} from '../../core/models/lab-result.models';

type Scope = 'all' | 'preliminary' | 'final';

@Component({
  selector: 'app-lab-results',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './lab-results.component.html',
  styleUrl: './lab-results.component.scss'
})
export class LabResultsComponent implements OnInit {
  private readonly api = inject(LabResultService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly canCreate = computed(() => this.auth.role() === 'LabTech');

  readonly results = signal<LabResultResponse[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly scope = signal<Scope>('all');

  readonly filterForm = this.fb.nonNullable.group({
    labOrderId: [''],
    resultDate: ['']
  });

  readonly orderLookup = this.fb.nonNullable.control<string>('');

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const s = this.scope();
    return this.results().filter((r) => {
      if (s === 'preliminary' && r.status) return false;
      if (s === 'final' && !r.status) return false;
      if (term) {
        const blob = `${r.resultId} ${r.labOrderId} ${r.resultJson ?? ''}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  readonly stats = computed(() => {
    const all = this.results();
    const final = all.filter((r) => r.status).length;
    return { total: all.length, final, preliminary: all.length - final };
  });

  /* ── Detail drawer ── */
  readonly selected = signal<LabResultResponse | null>(null);
  readonly detailLoading = signal(false);

  /* ── Create modal ── */
  readonly showCreate = signal(false);
  readonly saving = signal(false);
  readonly newEntry = signal<LabResultEntry>({
    testName: '',
    value: '',
    unit: '',
    referenceRange: '',
    flag: 'Normal'
  });
  readonly draftEntries = signal<LabResultEntry[]>([]);

  readonly createForm = this.fb.nonNullable.group({
    labOrderId: [0, [Validators.required, Validators.min(1)]],
    resultDate: ['', Validators.required],
    status: [false] // false = Preliminary, true = Final
  });

  readonly flagOptions: { value: 'Normal' | 'Low' | 'High' | 'Critical'; tone: string }[] = [
    { value: 'Normal', tone: 'success' },
    { value: 'Low', tone: 'info' },
    { value: 'High', tone: 'warn' },
    { value: 'Critical', tone: 'danger' }
  ];

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    const v = this.filterForm.getRawValue();
    const filter: LabResultRequest = {
      labOrderId: v.labOrderId ? Number(v.labOrderId) : null,
      resultDate: v.resultDate || null
    };
    this.api.list(filter).subscribe({
      next: (list) => {
        this.results.set(list ?? []);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.results.set([]);
        this.loading.set(false);
        if (err.status !== 404) {
          this.toast.error(this.errorText(err, 'Could not load lab results'));
        }
      }
    });
  }

  applyFilters(): void {
    this.refresh();
  }

  resetFilters(): void {
    this.filterForm.reset({ labOrderId: '', resultDate: '' });
    this.refresh();
  }

  setSearch(v: string): void {
    this.searchTerm.set(v);
  }

  setScope(v: Scope): void {
    this.scope.set(v);
  }

  /* ── Lookup by lab order ── */
  lookupByOrder(): void {
    const raw = (this.orderLookup.value ?? '').trim();
    const id = Number(raw);
    if (!id || id < 1) {
      this.toast.error('Enter a valid lab order ID.');
      return;
    }
    this.loading.set(true);
    this.api.getByLabOrder(id).subscribe({
      next: (list) => {
        this.results.set(list ?? []);
        this.loading.set(false);
        if (!list?.length) this.toast.info(`No results yet for lab order #${id}.`);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.results.set([]);
        if (err.status === 404) {
          this.toast.info(`No results for lab order #${id}.`);
        } else {
          this.toast.error(this.errorText(err, 'Lookup failed'));
        }
      }
    });
  }

  /* ── Detail ── */
  open(r: LabResultResponse): void {
    this.selected.set(r);
    this.detailLoading.set(true);
    this.api.getById(r.resultId).subscribe({
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

  parsedEntries(r: LabResultResponse | null): LabResultEntry[] {
    return parseResultJson(r?.resultJson);
  }

  /* ── Create flow ── */
  openCreate(): void {
    const today = new Date().toISOString().slice(0, 10);
    this.createForm.reset({ labOrderId: 0, resultDate: today, status: false });
    this.draftEntries.set([]);
    this.newEntry.set({
      testName: '',
      value: '',
      unit: '',
      referenceRange: '',
      flag: 'Normal'
    });
    this.showCreate.set(true);
  }

  closeCreate(): void {
    this.showCreate.set(false);
  }

  setEntryField<K extends keyof LabResultEntry>(
    field: K,
    value: LabResultEntry[K]
  ): void {
    this.newEntry.update((e) => ({ ...e, [field]: value }));
  }

  addEntry(): void {
    const e = this.newEntry();
    if (!e.testName.trim() || !e.value.trim()) {
      this.toast.error('Test name and value are required.');
      return;
    }
    this.draftEntries.update((list) => [
      ...list,
      {
        testName: e.testName.trim(),
        value: e.value.trim(),
        unit: e.unit?.trim() || undefined,
        referenceRange: e.referenceRange?.trim() || undefined,
        flag: e.flag
      }
    ]);
    this.newEntry.set({
      testName: '',
      value: '',
      unit: '',
      referenceRange: '',
      flag: 'Normal'
    });
  }

  removeEntry(idx: number): void {
    this.draftEntries.update((list) => list.filter((_, i) => i !== idx));
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    if (this.draftEntries().length === 0) {
      this.toast.error('Add at least one result entry.');
      return;
    }
    const v = this.createForm.getRawValue();
    const isoDate = new Date(`${v.resultDate}T09:00:00`).toISOString();
    this.saving.set(true);
    this.api
      .create({
        labOrderId: Number(v.labOrderId),
        resultJson: JSON.stringify(this.draftEntries()),
        resultDate: isoDate,
        status: !!v.status
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Lab result recorded');
          this.closeCreate();
          this.refresh();
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(this.errorText(err, 'Could not record lab result'));
        }
      });
  }

  toneForFlag(flag: string | undefined | null): string {
    return this.flagOptions.find((f) => f.value === flag)?.tone ?? 'muted';
  }

  setEntryFlag(flag: 'Normal' | 'Low' | 'High' | 'Critical'): void {
    this.newEntry.update((e) => ({ ...e, flag }));
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
