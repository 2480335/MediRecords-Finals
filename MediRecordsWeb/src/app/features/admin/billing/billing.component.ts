import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { BillingService } from '../../../core/services/billing.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserService } from '../../../core/services/user.service';
import {
  ExportFormat,
  ExportStatus,
  PagedResponse,
  UnbilledEncounter,
  VisitChargeResponse
} from '../../../core/models/billing.models';
import { ProviderLookup } from '../../../core/models/user.models';

@Component({
  selector: 'app-admin-billing',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './billing.component.html',
  styleUrl: './billing.component.scss'
})
export class BillingComponent implements OnInit {
  private readonly api = inject(BillingService);
  private readonly userApi = inject(UserService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(true);
  readonly providers = signal<ProviderLookup[]>([]);
  readonly exporting = signal(false);
  readonly markingBilled = signal(false);
  readonly drawerLoading = signal(false);

  readonly page = signal<PagedResponse<UnbilledEncounter> | null>(null);

  readonly filters = this.fb.nonNullable.group({
    fromDate: [''],
    toDate: [''],
    providerId: ['']
  });

  readonly currentPage = signal(1);
  readonly pageSize = signal(10);

  readonly selectedEncounterIds = signal<Set<number>>(new Set());

  /* ── Drawer state for charge picker ── */
  readonly drawerOpen = signal(false);
  readonly drawerEncounter = signal<UnbilledEncounter | null>(null);
  readonly drawerCharges = signal<VisitChargeResponse[]>([]);
  readonly selectedChargeIds = signal<Set<number>>(new Set());

  /* ── Export modal state ── */
  readonly exportOpen = signal(false);
  readonly exportForm = this.fb.nonNullable.group({
    format: ['csv' as ExportFormat],
    status: ['All' as ExportStatus],
    fromDate: [''],
    toDate: ['']
  });

  readonly summary = computed(() => {
    const p = this.page();
    if (!p) return { count: 0, total: 0 };
    const total = p.data.reduce((s, e) => s + (e.totalChargeAmount ?? 0), 0);
    return { count: p.totalCount, total };
  });

  ngOnInit(): void {
    this.load();
    this.userApi.getProviders().subscribe({
      next: (list) => this.providers.set(list),
      error: () => this.toast.error('Could not load provider list')
    });
  }

  load(): void {
    this.loading.set(true);
    const v = this.filters.getRawValue();
    this.api
      .getUnbilledEncounters({
        fromDate: v.fromDate || null,
        toDate: v.toDate || null,
        providerId: v.providerId ? Number(v.providerId) : null,
        page: this.currentPage(),
        pageSize: this.pageSize()
      })
      .subscribe({
        next: (res) => {
          this.page.set(res);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.toast.error('Failed to load unbilled encounters');
        }
      });
  }

  applyFilters(): void {
    this.currentPage.set(1);
    this.load();
  }

  resetFilters(): void {
    this.filters.reset({ fromDate: '', toDate: '', providerId: '' });
    this.currentPage.set(1);
    this.load();
  }

  goToPage(page: number): void {
    const p = this.page();
    if (!p) return;
    if (page < 1 || page > p.totalPages) return;
    this.currentPage.set(page);
    this.load();
  }

  /* ── Drawer (charges for an encounter) ── */
  openDrawer(encounter: UnbilledEncounter): void {
    this.drawerEncounter.set(encounter);
    this.drawerCharges.set([]);
    this.selectedChargeIds.set(new Set());
    this.drawerLoading.set(true);
    this.drawerOpen.set(true);

    this.api.getChargesByEncounter(encounter.encounterId).subscribe({
      next: (charges) => {
        this.drawerCharges.set(charges);
        const unbilledIds = charges
          .filter((c) => c.status === 'Unbilled' || c.status === 'False' || c.status === 'false')
          .map((c) => c.chargeId);
        this.selectedChargeIds.set(new Set(unbilledIds));
        this.drawerLoading.set(false);
      },
      error: () => {
        this.drawerLoading.set(false);
        this.toast.error('Could not load charges for this encounter');
      }
    });
  }

  closeDrawer(): void {
    this.drawerOpen.set(false);
    this.drawerEncounter.set(null);
  }

  toggleChargeSelection(chargeId: number): void {
    this.selectedChargeIds.update((set) => {
      const next = new Set(set);
      if (next.has(chargeId)) next.delete(chargeId);
      else next.add(chargeId);
      return next;
    });
  }

  isChargeSelected(chargeId: number): boolean {
    return this.selectedChargeIds().has(chargeId);
  }

  drawerSelectedTotal(): number {
    return this.drawerCharges()
      .filter((c) => this.selectedChargeIds().has(c.chargeId))
      .reduce((s, c) => s + (c.amount ?? 0), 0);
  }

  markSelectedAsBilled(): void {
    const ids = [...this.selectedChargeIds()];
    if (!ids.length) {
      this.toast.error('Select at least one charge');
      return;
    }
    this.markingBilled.set(true);
    this.api.markBilled({ chargeIds: ids }).subscribe({
      next: (res) => {
        this.markingBilled.set(false);
        this.toast.success(
          `${res.markedAsBilled.length} charge(s) marked as billed` +
            (res.alreadyBilled.length ? ` · ${res.alreadyBilled.length} already billed` : '')
        );
        this.closeDrawer();
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        this.markingBilled.set(false);
        this.toast.error(this.errorText(err, 'Failed to mark charges'));
      }
    });
  }

  /* ── Export ── */
  openExport(): void {
    const v = this.filters.getRawValue();
    this.exportForm.reset({
      format: 'csv',
      status: 'All',
      fromDate: v.fromDate || '',
      toDate: v.toDate || ''
    });
    this.exportOpen.set(true);
  }

  closeExport(): void {
    this.exportOpen.set(false);
  }

  submitExport(): void {
    const v = this.exportForm.getRawValue();
    this.exporting.set(true);
    this.api
      .exportCharges(v.format, v.status, v.fromDate || null, v.toDate || null)
      .subscribe({
        next: (blob) => {
          this.exporting.set(false);
          const url = URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `charges-${v.status.toLowerCase()}-${Date.now()}.${v.format}`;
          document.body.appendChild(a);
          a.click();
          a.remove();
          URL.revokeObjectURL(url);
          this.toast.success('Export downloaded');
          this.closeExport();
        },
        error: (err: HttpErrorResponse) => {
          this.exporting.set(false);
          this.toast.error(this.errorText(err, 'Export failed'));
        }
      });
  }

  /* ── Helpers ── */
  pageNumbers(): number[] {
    const p = this.page();
    if (!p) return [];
    const total = p.totalPages;
    const current = p.page;
    const max = 5;
    const start = Math.max(1, current - 2);
    const end = Math.min(total, start + max - 1);
    const realStart = Math.max(1, end - max + 1);
    const arr: number[] = [];
    for (let i = realStart; i <= end; i++) arr.push(i);
    return arr;
  }

  initials(name: string): string {
    return name
      .split(' ')
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
