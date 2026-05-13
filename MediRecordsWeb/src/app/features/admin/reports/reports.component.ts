import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ReportsService } from '../../../core/services/reports.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserService } from '../../../core/services/user.service';
import {
  ClinicKpiReport,
  DocumentationCompletenessReport,
  IncompleteEncounter,
  NoShowCancellationReport,
  ProviderUtilizationReport
} from '../../../core/models/reports.models';
import { ProviderLookup } from '../../../core/models/user.models';

type Tab = 'kpis' | 'doc' | 'noshow' | 'util';

const TAB_META: Record<Tab, { label: string; icon: string; description: string }> = {
  kpis: { label: 'Clinic KPIs', icon: 'bi-speedometer2', description: 'Headline counts across the practice' },
  doc: { label: 'Documentation', icon: 'bi-journal-check', description: 'Encounters with missing or unsigned SOAP sections' },
  noshow: { label: 'No-show & Cancellation', icon: 'bi-calendar-x', description: 'Appointment attendance for a date range' },
  util: { label: 'Provider Utilization', icon: 'bi-person-workspace', description: 'Scheduled vs completed for one provider' }
};

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  private readonly api = inject(ReportsService);
  private readonly userApi = inject(UserService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly activeTab = signal<Tab>('kpis');
  readonly providers = signal<ProviderLookup[]>([]);

  ngOnInit(): void {
    this.userApi.getProviders().subscribe({
      next: (list) => this.providers.set(list),
      error: () => this.toast.error('Could not load provider list')
    });
  }
  readonly tabMeta = TAB_META;
  readonly tabs: Tab[] = ['kpis', 'doc', 'noshow', 'util'];
  readonly loading = signal(false);

  readonly todayIso = new Date().toISOString().slice(0, 10);
  readonly monthStartIso = (() => {
    const d = new Date();
    d.setDate(1);
    return d.toISOString().slice(0, 10);
  })();

  readonly kpiResult = signal<ClinicKpiReport | null>(null);
  readonly docResult = signal<DocumentationCompletenessReport | null>(null);
  readonly noShowResult = signal<NoShowCancellationReport | null>(null);
  readonly utilResult = signal<ProviderUtilizationReport | null>(null);

  readonly kpiForm = this.fb.nonNullable.group({
    fromDate: [this.monthStartIso, Validators.required],
    toDate: [this.todayIso, Validators.required]
  });

  readonly docForm = this.fb.nonNullable.group({
    startDate: [this.monthStartIso, Validators.required],
    endDate: [this.todayIso, Validators.required],
    providerId: ['']
  });

  readonly noShowForm = this.fb.nonNullable.group({
    startDate: [this.monthStartIso, Validators.required],
    endDate: [this.todayIso, Validators.required],
    providerId: ['']
  });

  readonly utilForm = this.fb.nonNullable.group({
    providerId: ['', [Validators.required, Validators.min(1)]],
    startDate: [this.monthStartIso, Validators.required],
    endDate: [this.todayIso, Validators.required]
  });

  readonly currentMeta = computed(() => TAB_META[this.activeTab()]);
  readonly attendanceRate = computed(() => {
    const r = this.noShowResult();
    if (!r) return null;
    const remainder = 100 - (Number(r.noShowPercentage ?? 0) + Number(r.cancellationPercentage ?? 0));
    return Math.max(0, Math.round(remainder * 100) / 100);
  });

  setTab(t: Tab): void {
    this.activeTab.set(t);
  }

  runKpis(): void {
    if (this.kpiForm.invalid) {
      this.kpiForm.markAllAsTouched();
      return;
    }
    const v = this.kpiForm.getRawValue();
    if (v.fromDate > v.toDate) {
      this.toast.error('From date must be on or before To date');
      return;
    }
    this.loading.set(true);
    this.api.clinicKpis(v.fromDate, v.toDate).subscribe({
      next: (r) => {
        this.kpiResult.set(r);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.kpiResult.set(null);
        this.loading.set(false);
        this.toast.error(this.errorText(err, 'Failed to load Clinic KPIs'));
      }
    });
  }

  runDoc(): void {
    if (this.docForm.invalid) {
      this.docForm.markAllAsTouched();
      return;
    }
    const v = this.docForm.getRawValue();
    if (v.startDate > v.endDate) {
      this.toast.error('Start date must be on or before End date');
      return;
    }
    const pid = v.providerId ? Number(v.providerId) : null;
    this.loading.set(true);
    this.api.documentationCompleteness(v.startDate, v.endDate, pid).subscribe({
      next: (r) => {
        this.docResult.set(r);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.docResult.set(null);
        this.loading.set(false);
        this.toast.error(this.errorText(err, 'Failed to load Documentation report'));
      }
    });
  }

  runNoShow(): void {
    if (this.noShowForm.invalid) {
      this.noShowForm.markAllAsTouched();
      return;
    }
    const v = this.noShowForm.getRawValue();
    if (v.startDate > v.endDate) {
      this.toast.error('Start date must be on or before End date');
      return;
    }
    const pid = v.providerId ? Number(v.providerId) : null;
    this.loading.set(true);
    this.api.noShowCancellation(v.startDate, v.endDate, pid).subscribe({
      next: (r) => {
        this.noShowResult.set(r);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.noShowResult.set(null);
        this.loading.set(false);
        this.toast.error(this.errorText(err, 'Failed to load No-show report'));
      }
    });
  }

  runUtil(): void {
    if (this.utilForm.invalid) {
      this.utilForm.markAllAsTouched();
      return;
    }
    const v = this.utilForm.getRawValue();
    if (v.startDate > v.endDate) {
      this.toast.error('Start date must be on or before End date');
      return;
    }
    this.loading.set(true);
    this.api.providerUtilization(Number(v.providerId), v.startDate, v.endDate).subscribe({
      next: (r) => {
        this.utilResult.set(r);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.utilResult.set(null);
        this.loading.set(false);
        this.toast.error(this.errorText(err, 'Failed to load Provider Utilization'));
      }
    });
  }

  trackEnc(_: number, e: IncompleteEncounter): number {
    return e.encounterId;
  }

  trackSection(_: number, s: string): string {
    return s;
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
