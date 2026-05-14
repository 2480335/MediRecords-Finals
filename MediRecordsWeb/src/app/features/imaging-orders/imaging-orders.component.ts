import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { EncounterService } from '../../core/services/encounter.service';
import { ImagingOrderService } from '../../core/services/imaging-order.service';
import { ToastService } from '../../core/services/toast.service';
import { SearchableSelectComponent, SearchableOption } from '../../shared/components/searchable-select/searchable-select.component';
import {
  IMAGING_STUDY_TYPES,
  ImagingOrderResponse,
  ImagingReportFull,
  ImagingStudyType
} from '../../core/models/imaging.models';
import { EncounterLookup } from '../../core/models/encounter.models';

@Component({
  selector: 'app-imaging-orders',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './imaging-orders.component.html',
  styleUrl: './imaging-orders.component.scss'
})
export class ImagingOrdersComponent implements OnInit {
  private readonly api = inject(ImagingOrderService);
  private readonly encounterApi = inject(EncounterService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  readonly studyTypes = IMAGING_STUDY_TYPES;
  readonly encounters = signal<EncounterLookup[]>([]);
  readonly lockedEncounter = signal<{ id: number; patientName: string } | null>(null);
  readonly encounterOptions = computed<SearchableOption[]>(() =>
    this.encounters().map((e) => ({ value: e.encounterId, label: `#${e.encounterId}` }))
  );

  readonly orders = signal<ImagingOrderResponse[]>([]);
  readonly loading = signal(true);
  readonly lastError = signal<string | null>(null);
  readonly searchTerm = signal('');
  readonly studyFilter = signal<ImagingStudyType | 'all'>('all');
  readonly statusFilter = signal<'all' | 'open' | 'reported'>('all');

  readonly filterForm = this.fb.nonNullable.group({
    encounterId: [''],
    orderedDate: ['']
  });

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const study = this.studyFilter();
    const status = this.statusFilter();
    return this.orders().filter((o) => {
      if (study !== 'all' && o.studyType !== study) return false;
      if (status === 'open' && o.status) return false;
      if (status === 'reported' && !o.status) return false;
      if (term) {
        const blob = `${o.encounterId} ${o.studyType ?? ''} ${o.notes ?? ''}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  readonly stats = computed(() => {
    const all = this.orders();
    const open = all.filter((o) => !o.status).length;
    return { total: all.length, open, reported: all.length - open };
  });

  /* ── Drawer (reports) ── */
  readonly drawerOrder = signal<ImagingOrderResponse | null>(null);
  readonly drawerReports = signal<ImagingReportFull[]>([]);
  readonly drawerLoading = signal(false);

  /* ── Create modal ── */
  readonly showCreate = signal(false);
  readonly saving = signal(false);

  readonly createForm = this.fb.nonNullable.group({
    encounterId: [0, [Validators.required, Validators.min(1)]],
    studyType: ['Xray' as ImagingStudyType, Validators.required],
    notes: ['', [Validators.required, Validators.maxLength(2000)]]
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
    this.lastError.set(null);
    const v = this.filterForm.getRawValue();
    this.api
      .list({
        encounterID: v.encounterId ? Number(v.encounterId) : null,
        orderedDate: v.orderedDate || null
      })
      .subscribe({
        next: (list) => {
          this.orders.set(list ?? []);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.orders.set([]);
          this.loading.set(false);
          if (err.status === 404) return;
          // eslint-disable-next-line no-console
          console.error('[ImagingOrders] GET failed', err);
          this.lastError.set(this.describeListError(err, 'Could not load imaging orders'));
        }
      });
  }

  private describeListError(err: HttpErrorResponse, fallback: string): string {
    if (err.status === 0) return 'Cannot reach the API. Check that the backend is running.';
    if (err.status === 401) return 'Your session has expired. Please sign in again.';
    if (err.status === 403) return "You don't have permission to view imaging orders.";
    if (err.status >= 500) {
      const detail = this.errorText(err, '');
      return detail
        ? `${fallback}: ${detail}`
        : `${fallback} (HTTP ${err.status}). Check backend logs.`;
    }
    return this.errorText(err, `${fallback} (HTTP ${err.status}).`);
  }

  applyFilters(): void {
    this.refresh();
  }

  resetFilters(): void {
    this.filterForm.reset({ encounterId: '', orderedDate: '' });
    this.refresh();
  }

  setSearch(v: string): void {
    this.searchTerm.set(v);
  }

  setStudy(v: ImagingStudyType | 'all'): void {
    this.studyFilter.set(v);
  }

  setStatus(v: 'all' | 'open' | 'reported'): void {
    this.statusFilter.set(v);
  }

  /* ── Drawer ── */
  openDrawer(order: ImagingOrderResponse): void {
    this.drawerOrder.set(order);
    this.drawerReports.set([]);
    this.drawerLoading.set(true);
    this.api.getReportsByOrderId(order.imagingOrderId).subscribe({
      next: (reports) => {
        this.drawerReports.set(reports ?? []);
        this.drawerLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.drawerLoading.set(false);
        if (err.status !== 404) {
          this.toast.error(this.errorText(err, 'Could not load reports'));
        }
      }
    });
  }

  closeDrawer(): void {
    this.drawerOrder.set(null);
    this.drawerReports.set([]);
  }

  /* ── Create ── */
  openCreate(): void {
    const lockedId = this.lockedEncounter()?.id ?? 0;
    this.createForm.reset({
      encounterId: lockedId,
      studyType: 'Xray',
      notes: ''
    });
    this.showCreate.set(true);
  }

  closeCreate(): void {
    this.showCreate.set(false);
  }

  setStudyType(v: ImagingStudyType): void {
    this.createForm.patchValue({ studyType: v });
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    const v = this.createForm.getRawValue();
    this.saving.set(true);
    this.api
      .create(Number(v.encounterId), {
        studyType: v.studyType,
        notes: v.notes
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Imaging order created');
          this.closeCreate();
          this.refresh();
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(this.errorText(err, 'Could not create imaging order'));
        }
      });
  }

  /* ── Helpers ── */
  toneFor(study: ImagingStudyType | null): string {
    if (!study) return 'muted';
    return IMAGING_STUDY_TYPES.find((s) => s.value === study)?.tone ?? 'muted';
  }

  iconFor(study: ImagingStudyType | null): string {
    if (!study) return 'bi-camera';
    return IMAGING_STUDY_TYPES.find((s) => s.value === study)?.icon ?? 'bi-camera';
  }

  labelFor(study: ImagingStudyType | null): string {
    if (!study) return '—';
    return IMAGING_STUDY_TYPES.find((s) => s.value === study)?.label ?? study;
  }

  isReportFinal(status: string): boolean {
    const s = (status ?? '').toString().toLowerCase();
    return s === 'true' || s === 'final' || s === 'finalized' || s === 'completed';
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    // The backend's 500 handler puts the real cause in `error` and a generic
    // "internal server error" in `message`. Prefer `error` so we surface the
    // actual SQL/EF detail to the user instead of a useless platitude.
    return err.error?.error ?? err.error?.message ?? fallback;
  }
}
