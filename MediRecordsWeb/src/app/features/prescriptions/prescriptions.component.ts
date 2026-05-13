import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { EncounterService } from '../../core/services/encounter.service';
import { PrescriptionService } from '../../core/services/prescription.service';
import { ToastService } from '../../core/services/toast.service';
import { UserService } from '../../core/services/user.service';
import {
  COMMON_FREQUENCIES,
  COMMON_ROUTES,
  PRESCRIPTION_STATUSES,
  PrescriptionItem,
  PrescriptionStatusName,
  PrescriptionWithItems,
  PrescriptionWithItemsCreateRequest,
  PrescriptionWithItemsUpdateRequest
} from '../../core/models/prescription.models';
import { EncounterLookup } from '../../core/models/encounter.models';
import { ProviderLookup } from '../../core/models/user.models';

type StatusTab = 'all' | 'Draft' | 'Issued';

@Component({
  selector: 'app-prescriptions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './prescriptions.component.html',
  styleUrl: './prescriptions.component.scss'
})
export class PrescriptionsComponent implements OnInit {
  private readonly api = inject(PrescriptionService);
  private readonly encounterApi = inject(EncounterService);
  private readonly userApi = inject(UserService);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  readonly canCreate = computed(() => this.auth.role() === 'Physician');
  readonly canDelete = computed(() => this.auth.role() === 'Admin');

  readonly encounters = signal<EncounterLookup[]>([]);
  readonly providers = signal<ProviderLookup[]>([]);
  readonly lockedEncounter = signal<{ id: number; patientName: string } | null>(null);

  readonly statuses = PRESCRIPTION_STATUSES;
  readonly commonRoutes = COMMON_ROUTES;
  readonly commonFrequencies = COMMON_FREQUENCIES;

  readonly prescriptions = signal<PrescriptionWithItems[]>([]);
  readonly loading = signal(true);
  readonly lastError = signal<string | null>(null);
  readonly searchTerm = signal('');
  readonly statusTab = signal<StatusTab>('all');

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const tab = this.statusTab();
    return this.prescriptions().filter((p) => {
      if (tab !== 'all' && this.statusName(p.status) !== tab) return false;
      if (term) {
        const itemBlob = p.prescriptionItems
          .map((i) => `${i.drugName} ${i.dose ?? ''} ${i.frequency ?? ''} ${i.route ?? ''}`)
          .join(' ');
        const blob = `${p.prescriptionId} ${p.encounterId} ${p.providerId} ${itemBlob}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  readonly stats = computed(() => {
    const all = this.prescriptions();
    const issued = all.filter((p) => this.statusName(p.status) === 'Issued').length;
    return { total: all.length, issued, draft: all.length - issued };
  });

  /* ── Drawer ── */
  readonly selected = signal<PrescriptionWithItems | null>(null);
  readonly detailLoading = signal(false);

  /* ── Create modal ── */
  readonly showCreate = signal(false);
  readonly saving = signal(false);
  readonly draftItems = signal<PrescriptionItem[]>([]);
  readonly newItem = signal<PrescriptionItem>(this.emptyItem());

  readonly createForm = this.fb.nonNullable.group({
    encounterId: [0, [Validators.required, Validators.min(1)]],
    providerId: [0, [Validators.required, Validators.min(1)]],
    status: ['Draft' as PrescriptionStatusName, Validators.required]
  });

  /* ── Edit modal ── */
  readonly showEdit = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly editItems = signal<PrescriptionItem[]>([]);
  readonly editNewItem = signal<PrescriptionItem>(this.emptyItem());

  readonly editForm = this.fb.nonNullable.group({
    providerId: [0, [Validators.required, Validators.min(1)]],
    status: ['Draft' as PrescriptionStatusName, Validators.required]
  });

  /* ── Delete confirm ── */
  readonly showDelete = signal(false);
  readonly deleting = signal<PrescriptionWithItems | null>(null);

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
    } else {
      this.encounterApi.getAll().subscribe({
        next: (list) => this.encounters.set(list),
        error: () => this.toast.error('Could not load encounter list')
      });
      this.userApi.getProviders().subscribe({
        next: (list) => this.providers.set(list),
        error: () => this.toast.error('Could not load provider list')
      });
    }
  }

  refresh(): void {
    this.loading.set(true);
    this.lastError.set(null);
    this.api.list().subscribe({
      next: (list) => {
        this.prescriptions.set(list ?? []);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.prescriptions.set([]);
        this.loading.set(false);
        if (err.status === 404) return; // empty list — handled by empty state
        // eslint-disable-next-line no-console
        console.error('[Prescriptions] GET failed', err);
        const msg = this.describeListError(err, 'Could not load prescriptions');
        this.lastError.set(msg);
      }
    });
  }

  private describeListError(err: HttpErrorResponse, fallback: string): string {
    if (err.status === 0) {
      return 'Cannot reach the API. Check that the backend is running.';
    }
    if (err.status === 401) return 'Your session has expired. Please sign in again.';
    if (err.status === 403) return "You don't have permission to view this list.";
    if (err.status >= 500) {
      const detail = this.errorText(err, '');
      return detail
        ? `${fallback}: ${detail}`
        : `${fallback} (HTTP ${err.status}). Check backend logs.`;
    }
    return this.errorText(err, `${fallback} (HTTP ${err.status}).`);
  }

  setSearch(v: string): void {
    this.searchTerm.set(v);
  }

  setStatusTab(v: StatusTab): void {
    this.statusTab.set(v);
  }

  /* ── Detail drawer ── */
  open(p: PrescriptionWithItems): void {
    this.selected.set(p);
    this.detailLoading.set(true);
    this.api.getById(p.prescriptionId).subscribe({
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

  /* ── Create flow ── */
  openCreate(): void {
    const lockedId = this.lockedEncounter()?.id ?? 0;
    const providerId = this.lockedEncounter() ? (this.auth.user()?.userId ?? 0) : 0;
    this.createForm.reset({
      encounterId: lockedId,
      providerId,
      status: 'Draft'
    });
    this.draftItems.set([]);
    this.newItem.set(this.emptyItem());
    this.showCreate.set(true);
  }

  closeCreate(): void {
    this.showCreate.set(false);
  }

  setNewItemField<K extends keyof PrescriptionItem>(
    field: K,
    value: PrescriptionItem[K]
  ): void {
    this.newItem.update((i) => ({ ...i, [field]: value }));
  }

  addDraftItem(): void {
    const item = this.newItem();
    if (!item.drugName.trim()) {
      this.toast.error('Drug name is required.');
      return;
    }
    if (!item.durationDays || item.durationDays <= 0) {
      this.toast.error('Duration (days) must be greater than zero.');
      return;
    }
    this.draftItems.update((list) => [...list, this.normalize(item)]);
    this.newItem.set(this.emptyItem());
  }

  removeDraftItem(idx: number): void {
    this.draftItems.update((list) => list.filter((_, i) => i !== idx));
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    if (this.draftItems().length === 0) {
      this.toast.error('Add at least one prescription item.');
      return;
    }
    const v = this.createForm.getRawValue();
    const payload: PrescriptionWithItemsCreateRequest = {
      encounterId: Number(v.encounterId),
      providerId: Number(v.providerId),
      status: v.status,
      prescriptionItems: [...this.draftItems()]
    };
    this.saving.set(true);
    this.api.create(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Prescription created');
        this.closeCreate();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not create prescription'));
      }
    });
  }

  /* ── Edit flow ── */
  openEdit(p: PrescriptionWithItems): void {
    this.editingId.set(p.prescriptionId);
    this.editForm.reset({
      providerId: p.providerId,
      status: this.statusName(p.status)
    });
    this.editItems.set(
      p.prescriptionItems.map((i) => ({
        drugName: i.drugName,
        dose: i.dose ?? '',
        frequency: i.frequency ?? '',
        route: i.route ?? '',
        durationDays: i.durationDays,
        instructions: i.instructions ?? ''
      }))
    );
    this.editNewItem.set(this.emptyItem());
    this.selected.set(null);
    this.showEdit.set(true);
  }

  closeEdit(): void {
    this.showEdit.set(false);
    this.editingId.set(null);
  }

  setEditItemField<K extends keyof PrescriptionItem>(
    field: K,
    value: PrescriptionItem[K]
  ): void {
    this.editNewItem.update((i) => ({ ...i, [field]: value }));
  }

  addEditItem(): void {
    const item = this.editNewItem();
    if (!item.drugName.trim()) {
      this.toast.error('Drug name is required.');
      return;
    }
    if (!item.durationDays || item.durationDays <= 0) {
      this.toast.error('Duration (days) must be greater than zero.');
      return;
    }
    this.editItems.update((list) => [...list, this.normalize(item)]);
    this.editNewItem.set(this.emptyItem());
  }

  removeEditItem(idx: number): void {
    this.editItems.update((list) => list.filter((_, i) => i !== idx));
  }

  submitEdit(): void {
    const id = this.editingId();
    if (!id || this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    if (this.editItems().length === 0) {
      this.toast.error('Add at least one prescription item.');
      return;
    }
    const v = this.editForm.getRawValue();
    const payload: PrescriptionWithItemsUpdateRequest = {
      providerId: Number(v.providerId),
      status: v.status,
      prescriptionItems: [...this.editItems()]
    };
    this.saving.set(true);
    this.api.update(id, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Prescription updated');
        this.closeEdit();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not update prescription'));
      }
    });
  }

  /* ── Delete flow ── */
  openDelete(p: PrescriptionWithItems): void {
    this.deleting.set(p);
    this.selected.set(null);
    this.showDelete.set(true);
  }

  closeDelete(): void {
    this.showDelete.set(false);
    this.deleting.set(null);
  }

  confirmDelete(): void {
    const p = this.deleting();
    if (!p) return;
    this.saving.set(true);
    this.api.remove(p.prescriptionId).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Prescription deleted');
        this.closeDelete();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not delete prescription'));
      }
    });
  }

  /* ── Helpers ── */
  statusName(status: string | null | undefined): PrescriptionStatusName {
    const s = (status ?? '').toString().trim().toLowerCase();
    if (s === 'issued' || s === '1') return 'Issued';
    return 'Draft';
  }

  toneFor(status: string | null | undefined): string {
    return PRESCRIPTION_STATUSES.find((s) => s.value === this.statusName(status))?.tone ?? 'muted';
  }

  iconForStatus(status: string | null | undefined): string {
    return PRESCRIPTION_STATUSES.find((s) => s.value === this.statusName(status))?.icon ?? 'bi-pencil';
  }

  itemSummary(item: PrescriptionItem): string {
    const parts = [item.dose, item.frequency, item.route].filter(Boolean);
    return parts.join(' · ');
  }

  private emptyItem(): PrescriptionItem {
    return {
      drugName: '',
      dose: '',
      frequency: '',
      route: '',
      durationDays: 7,
      instructions: ''
    };
  }

  private normalize(item: PrescriptionItem): PrescriptionItem {
    return {
      drugName: item.drugName.trim(),
      dose: item.dose?.trim() || null,
      frequency: item.frequency?.trim() || null,
      route: item.route?.trim() || null,
      durationDays: Number(item.durationDays),
      instructions: item.instructions?.trim() || null
    };
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
