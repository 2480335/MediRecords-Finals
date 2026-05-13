import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EncounterService } from '../../core/services/encounter.service';
import { ToastService } from '../../core/services/toast.service';
import {
  ENCOUNTER_STATUSES,
  EncounterDetail,
  EncounterStatusName,
  EncounterSummary
} from '../../core/models/encounter.models';

@Component({
  selector: 'app-workspace',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './workspace.component.html',
  styleUrl: './workspace.component.scss'
})
export class WorkspaceComponent implements OnInit {
  private readonly api = inject(EncounterService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly chartActions: { route: string; label: string; icon: string; passPatient?: boolean }[] = [
    { route: '/soap-notes',      label: 'SOAP note',      icon: 'bi-journal-text' },
    { route: '/vitals',          label: 'Vitals',         icon: 'bi-heart-pulse' },
    { route: '/nursing-notes',   label: 'Nursing note',   icon: 'bi-clipboard2-pulse' },
    { route: '/lab-orders',      label: 'Lab order',      icon: 'bi-flask' },
    { route: '/imaging-orders',  label: 'Imaging order',  icon: 'bi-camera' },
    { route: '/prescriptions',   label: 'Prescription',   icon: 'bi-capsule' },
    { route: '/charges',         label: 'Charges',        icon: 'bi-receipt' },
    { route: '/follow-ups',      label: 'Follow-up',      icon: 'bi-calendar2-event' },
    { route: '/documents',       label: 'Documents',      icon: 'bi-paperclip', passPatient: true }
  ];

  gotoChart(route: string, passPatient = false): void {
    const enc = this.selected();
    if (!enc) return;
    const queryParams: Record<string, string | number> = {
      encounterId: enc.encounterId,
      patientName: enc.patientName ?? ''
    };
    if (passPatient && enc.patientId) {
      queryParams['patientId'] = enc.patientId;
    }
    this.router.navigate([route], { queryParams });
  }

  readonly statuses = ENCOUNTER_STATUSES;

  readonly summaries = signal<EncounterSummary[]>([]);
  readonly loading = signal(true);
  readonly statusFilter = signal<EncounterStatusName | 'all'>('all');

  readonly selected = signal<EncounterDetail | null>(null);
  readonly detailLoading = signal(false);
  readonly statusSaving = signal(false);

  readonly dateControl = this.fb.nonNullable.control<string>(
    new Date().toISOString().slice(0, 10)
  );

  readonly filtered = computed(() => {
    const s = this.statusFilter();
    if (s === 'all') return this.summaries();
    return this.summaries().filter((e) => this.statusName(e.status) === s);
  });

  readonly stats = computed(() => {
    const all = this.summaries();
    return {
      total: all.length,
      open: all.filter((e) => this.statusName(e.status) === 'Open').length,
      closed: all.filter((e) => this.statusName(e.status) === 'Closed').length,
      locked: all.filter((e) => this.statusName(e.status) === 'Locked').length
    };
  });

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const dateParam = params.get('date');
    if (dateParam) {
      this.dateControl.setValue(dateParam);
    }
    const encounterIdParam = Number(params.get('encounterId'));
    this.load(Number.isFinite(encounterIdParam) && encounterIdParam > 0 ? encounterIdParam : null);
  }

  load(autoOpenEncounterId: number | null = null): void {
    this.loading.set(true);
    const date = this.dateControl.value || null;
    this.api.workspace(date).subscribe({
      next: (list) => {
        this.summaries.set(list ?? []);
        this.loading.set(false);
        if (autoOpenEncounterId) {
          this.autoOpenEncounter(autoOpenEncounterId);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.summaries.set([]);
        this.loading.set(false);
        if (err.status !== 404) {
          this.toast.error(this.errorText(err, 'Could not load workspace'));
        }
        if (autoOpenEncounterId) {
          this.autoOpenEncounter(autoOpenEncounterId);
        }
      }
    });
  }

  private autoOpenEncounter(encounterId: number): void {
    const match = this.summaries().find((s) => s.encounterId === encounterId);
    if (match) {
      this.open(match);
      return;
    }
    // Encounter is outside the current date filter — fetch and open it directly.
    this.detailLoading.set(true);
    this.api.getById(encounterId).subscribe({
      next: (full) => {
        this.selected.set(full);
        this.detailLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.detailLoading.set(false);
        if (err.status !== 404) {
          this.toast.error(this.errorText(err, 'Could not load encounter detail'));
        }
      }
    });
  }

  setStatusFilter(v: EncounterStatusName | 'all'): void {
    this.statusFilter.set(v);
  }

  shiftDate(days: number): void {
    const cur = new Date(this.dateControl.value || new Date());
    cur.setDate(cur.getDate() + days);
    this.dateControl.setValue(cur.toISOString().slice(0, 10));
    this.load();
  }

  setToday(): void {
    this.dateControl.setValue(new Date().toISOString().slice(0, 10));
    this.load();
  }

  /* ─── Detail drawer ─── */
  open(summary: EncounterSummary): void {
    this.detailLoading.set(true);
    this.selected.set({
      encounterId: summary.encounterId,
      patientId: 0,
      patientName: summary.patientName,
      providerId: 0,
      providerName: '',
      visitType: summary.visitType,
      status: summary.status,
      date: summary.date
    });
    this.api.getById(summary.encounterId).subscribe({
      next: (full) => {
        this.selected.set(full);
        this.detailLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.detailLoading.set(false);
        this.toast.error(this.errorText(err, 'Could not load encounter detail'));
      }
    });
  }

  close(): void {
    this.selected.set(null);
  }

  changeStatus(target: EncounterStatusName): void {
    const enc = this.selected();
    if (!enc) return;
    if (this.statusName(enc.status) === target) return;
    this.statusSaving.set(true);

    this.api.updateStatus(enc.encounterId, target).subscribe({
      next: (res) => {
        this.statusSaving.set(false);
        this.toast.success(res.message || `Status changed to ${target}`);
        // Reflect change locally
        this.selected.set({ ...enc, status: target });
        this.summaries.update((list) =>
          list.map((s) =>
            s.encounterId === enc.encounterId ? { ...s, status: target } : s
          )
        );
      },
      error: (err: HttpErrorResponse) => {
        this.statusSaving.set(false);
        this.toast.error(this.errorText(err, 'Could not update status'));
      }
    });
  }

  /* ─── Helpers ─── */
  statusName(status: string | number | undefined): EncounterStatusName {
    if (typeof status === 'number') {
      if (status === 1) return 'Open';
      if (status === 2) return 'Closed';
      if (status === 3) return 'Locked';
    }
    const s = (status ?? '').toString();
    if (s === 'Open' || s === 'Closed' || s === 'Locked') return s;
    if (s === '1') return 'Open';
    if (s === '2') return 'Closed';
    if (s === '3') return 'Locked';
    return 'Open';
  }

  toneFor(status: EncounterStatusName): string {
    return ENCOUNTER_STATUSES.find((s) => s.value === status)?.tone ?? 'muted';
  }

  iconFor(visitType: string): string {
    const v = (visitType ?? '').toLowerCase();
    if (v.includes('tele')) return 'bi-camera-video';
    if (v.includes('follow')) return 'bi-arrow-repeat';
    if (v.includes('emerg')) return 'bi-shield-exclamation';
    if (v.includes('consult')) return 'bi-chat-dots';
    return 'bi-clipboard2-pulse';
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

  isLocked(): boolean {
    const enc = this.selected();
    return enc ? this.statusName(enc.status) === 'Locked' : false;
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
