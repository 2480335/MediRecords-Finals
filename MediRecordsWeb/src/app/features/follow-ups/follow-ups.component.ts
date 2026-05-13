import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { EncounterService } from '../../core/services/encounter.service';
import { FollowUpService } from '../../core/services/follow-up.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import {
  FollowUpDetails,
  FollowUpFilter
} from '../../core/models/follow-up.models';
import { EncounterLookup } from '../../core/models/encounter.models';
import { PatientLookup } from '../../core/models/patient.models';

@Component({
  selector: 'app-follow-ups',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './follow-ups.component.html',
  styleUrl: './follow-ups.component.scss'
})
export class FollowUpsComponent implements OnInit {
  private readonly api = inject(FollowUpService);
  private readonly patientApi = inject(PatientService);
  private readonly encounterApi = inject(EncounterService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  readonly followUps = signal<FollowUpDetails[]>([]);
  readonly patients = signal<PatientLookup[]>([]);
  readonly encounters = signal<EncounterLookup[]>([]);
  readonly lockedEncounter = signal<{ id: number; patientName: string } | null>(null);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly windowFilter = signal<'all' | 'upcoming' | 'past'>('all');

  readonly selected = signal<FollowUpDetails | null>(null);
  readonly detailLoading = signal(false);

  readonly filterForm = this.fb.nonNullable.group({
    patientId: [''],
    encounterId: ['']
  });

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const w = this.windowFilter();
    const now = Date.now();
    return this.followUps().filter((f) => {
      if (w !== 'all') {
        const due = new Date(f.recommendedDate).getTime();
        if (w === 'upcoming' && due < now) return false;
        if (w === 'past' && due >= now) return false;
      }
      if (term) {
        const blob = `${f.patientName} ${f.patientId} ${f.encounterId} ${f.notes}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  readonly stats = computed(() => {
    const list = this.followUps();
    const now = Date.now();
    const upcoming = list.filter(
      (f) => new Date(f.recommendedDate).getTime() >= now
    ).length;
    return { total: list.length, upcoming, past: list.length - upcoming };
  });

  /* ─── Create modal ─── */
  readonly showCreate = signal(false);
  readonly saving = signal(false);

  readonly createForm = this.fb.nonNullable.group({
    encounterId: [0, [Validators.required, Validators.min(1)]],
    recommendedDate: ['', Validators.required],
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
    } else {
      this.patientApi.getAll().subscribe({
        next: (list) => this.patients.set(list),
        error: () => this.toast.error('Could not load patient list')
      });
      this.encounterApi.getAll().subscribe({
        next: (list) => this.encounters.set(list),
        error: () => this.toast.error('Could not load encounter list')
      });
    }
  }

  refresh(): void {
    this.loading.set(true);
    const v = this.filterForm.getRawValue();
    const filter: FollowUpFilter = {
      patientId: v.patientId ? Number(v.patientId) : null,
      encounterId: v.encounterId ? Number(v.encounterId) : null
    };
    this.api.list(filter).subscribe({
      next: (list) => {
        this.followUps.set(list ?? []);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.followUps.set([]);
        this.loading.set(false);
        if (err.status !== 404) {
          this.toast.error(this.errorText(err, 'Could not load follow-ups'));
        }
      }
    });
  }

  applyFilters(): void {
    this.refresh();
  }

  resetFilters(): void {
    this.filterForm.reset({ patientId: '', encounterId: '' });
    this.refresh();
  }

  setSearch(v: string): void {
    this.searchTerm.set(v);
  }

  setWindow(v: 'all' | 'upcoming' | 'past'): void {
    this.windowFilter.set(v);
  }

  /* ─── Detail ─── */
  open(f: FollowUpDetails): void {
    this.selected.set(f);
    this.detailLoading.set(true);
    this.api.getById(f.followupId).subscribe({
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

  /* ─── Create ─── */
  openCreate(): void {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 7);
    const lockedId = this.lockedEncounter()?.id ?? 0;
    this.createForm.reset({
      encounterId: lockedId,
      recommendedDate: tomorrow.toISOString().slice(0, 10),
      notes: ''
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
    const isoDate = new Date(`${v.recommendedDate}T09:00:00`).toISOString();
    this.saving.set(true);
    this.api
      .create(Number(v.encounterId), {
        recommendedDate: isoDate,
        notes: v.notes
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Follow-up scheduled');
          this.closeCreate();
          this.refresh();
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(this.errorText(err, 'Could not schedule follow-up'));
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

  daysUntil(date: string): number {
    const ms = new Date(date).getTime() - Date.now();
    return Math.ceil(ms / (1000 * 60 * 60 * 24));
  }

  isUpcoming(date: string): boolean {
    return new Date(date).getTime() >= Date.now();
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
