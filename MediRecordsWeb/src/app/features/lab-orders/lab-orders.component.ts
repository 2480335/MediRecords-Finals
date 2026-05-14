import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { EncounterService } from '../../core/services/encounter.service';
import { LabOrderService } from '../../core/services/lab-order.service';
import { ToastService } from '../../core/services/toast.service';
import { SearchableSelectComponent, SearchableOption } from '../../shared/components/searchable-select/searchable-select.component';
import {
  COMMON_LAB_TESTS,
  LabOrderResponse,
  LabTestItem,
  parseTestJson
} from '../../core/models/lab-order.models';
import { EncounterLookup } from '../../core/models/encounter.models';

type ScopeFilter = 'all' | 'open' | 'completed';

@Component({
  selector: 'app-lab-orders',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './lab-orders.component.html',
  styleUrl: './lab-orders.component.scss'
})
export class LabOrdersComponent implements OnInit {
  private readonly api = inject(LabOrderService);
  private readonly encounterApi = inject(EncounterService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  readonly canCreate = computed(() => this.auth.role() === 'Physician');
  readonly canMark = computed(() => this.auth.role() === 'LabTech');

  readonly commonTests = COMMON_LAB_TESTS;
  readonly encounters = signal<EncounterLookup[]>([]);
  readonly lockedEncounter = signal<{ id: number; patientName: string } | null>(null);
  readonly encounterOptions = computed<SearchableOption[]>(() =>
    this.encounters().map((e) => ({ value: e.encounterId, label: `#${e.encounterId}` }))
  );

  readonly orders = signal<LabOrderResponse[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly scope = signal<ScopeFilter>('all');

  readonly filterForm = this.fb.nonNullable.group({
    encounterId: [''],
    orderedBy: [''],
    orderDate: ['']
  });

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const s = this.scope();
    return this.orders().filter((o) => {
      if (s === 'open' && o.status) return false;
      if (s === 'completed' && !o.status) return false;
      if (term) {
        const blob = `${o.labOrderId} ${o.encounterId} ${o.orderedBy} ${o.testJson}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  readonly stats = computed(() => {
    const all = this.orders();
    const completed = all.filter((o) => o.status).length;
    return { total: all.length, completed, open: all.length - completed };
  });

  /* ── Detail drawer ── */
  readonly selected = signal<LabOrderResponse | null>(null);
  readonly detailLoading = signal(false);
  readonly statusSaving = signal(false);

  /* ── Create modal ── */
  readonly showCreate = signal(false);
  readonly saving = signal(false);
  readonly newTestName = signal('');
  readonly newTestNotes = signal('');
  readonly draftTests = signal<LabTestItem[]>([]);

  readonly createForm = this.fb.nonNullable.group({
    encounterId: [0, [Validators.required, Validators.min(1)]],
    orderDate: ['']
  });

  ngOnInit(): void {
    this.refresh();
    const params = this.route.snapshot.queryParamMap;
    const encId = Number(params.get('encounterId'));
    if (Number.isFinite(encId) && encId > 0) {
      this.lockedEncounter.set({
        id: encId,
        patientName: params.get('patientName') ?? ''
      });
      this.openCreate();
      this.createForm.patchValue({ encounterId: encId });
    } else {
      this.encounterApi.getAll().subscribe({
        next: (list) => this.encounters.set(list),
        error: () => this.toast.error('Could not load encounter list')
      });
    }
  }

  refresh(): void {
    this.loading.set(true);
    const v = this.filterForm.getRawValue();
    this.api
      .list({
        encounterId: v.encounterId ? Number(v.encounterId) : null,
        orderedBy: v.orderedBy ? Number(v.orderedBy) : null,
        orderDate: v.orderDate || null
      })
      .subscribe({
        next: (list) => {
          this.orders.set(list ?? []);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.orders.set([]);
          this.loading.set(false);
          if (err.status !== 404) {
            this.toast.error(this.errorText(err, 'Could not load lab orders'));
          }
        }
      });
  }

  applyFilters(): void {
    this.refresh();
  }

  resetFilters(): void {
    this.filterForm.reset({ encounterId: '', orderedBy: '', orderDate: '' });
    this.refresh();
  }

  setSearch(v: string): void {
    this.searchTerm.set(v);
  }

  setScope(v: ScopeFilter): void {
    this.scope.set(v);
  }

  /* ── Detail ── */
  open(order: LabOrderResponse): void {
    this.selected.set(order);
    this.detailLoading.set(true);
    this.api.getById(order.labOrderId).subscribe({
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

  parsedTests(order: LabOrderResponse | null): LabTestItem[] {
    return parseTestJson(order?.testJson);
  }

  markStatus(order: LabOrderResponse, status: boolean): void {
    if (order.status === status) return;
    this.statusSaving.set(true);
    this.api.updateStatus(order.labOrderId, status).subscribe({
      next: () => {
        this.statusSaving.set(false);
        this.toast.success(status ? 'Marked as completed' : 'Reopened');
        const updated: LabOrderResponse = { ...order, status };
        this.selected.set(updated);
        this.orders.update((list) =>
          list.map((o) => (o.labOrderId === order.labOrderId ? updated : o))
        );
      },
      error: (err: HttpErrorResponse) => {
        this.statusSaving.set(false);
        this.toast.error(this.errorText(err, 'Could not update status'));
      }
    });
  }

  /* ── Create ── */
  openCreate(): void {
    const today = new Date().toISOString().slice(0, 10);
    const lockedId = this.lockedEncounter()?.id ?? 0;
    this.createForm.reset({ encounterId: lockedId, orderDate: today });
    this.draftTests.set([]);
    this.newTestName.set('');
    this.newTestNotes.set('');
    this.showCreate.set(true);
  }

  closeCreate(): void {
    this.showCreate.set(false);
  }

  setNewTestName(v: string): void {
    this.newTestName.set(v);
  }

  setNewTestNotes(v: string): void {
    this.newTestNotes.set(v);
  }

  pickCommon(name: string): void {
    if (this.draftTests().some((t) => t.testName === name)) {
      this.toast.info('That test is already in the list.');
      return;
    }
    this.draftTests.update((list) => [...list, { testName: name }]);
  }

  addCustomTest(): void {
    const name = this.newTestName().trim();
    if (!name) return;
    if (this.draftTests().some((t) => t.testName === name)) {
      this.toast.info('That test is already in the list.');
      return;
    }
    const notes = this.newTestNotes().trim();
    this.draftTests.update((list) => [
      ...list,
      { testName: name, notes: notes || undefined }
    ]);
    this.newTestName.set('');
    this.newTestNotes.set('');
  }

  removeTest(idx: number): void {
    this.draftTests.update((list) => list.filter((_, i) => i !== idx));
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    if (this.draftTests().length === 0) {
      this.toast.error('Add at least one test.');
      return;
    }
    const v = this.createForm.getRawValue();
    const isoDate = v.orderDate
      ? new Date(`${v.orderDate}T09:00:00`).toISOString()
      : undefined;
    this.saving.set(true);
    this.api
      .create({
        encounterId: Number(v.encounterId),
        testJson: JSON.stringify(this.draftTests()),
        orderDate: isoDate
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Lab order created');
          this.closeCreate();
          this.refresh();
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(this.errorText(err, 'Could not create lab order'));
        }
      });
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
