import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MedicationService } from '../../core/services/medication.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import {
  MedicationFilter,
  MedicationListItem
} from '../../core/models/medication.models';
import { PatientLookup } from '../../core/models/patient.models';

type StatusTab = 'all' | 'Active' | 'Inactive';

@Component({
  selector: 'app-medications',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './medications.component.html',
  styleUrl: './medications.component.scss'
})
export class MedicationsComponent implements OnInit {
  private readonly api = inject(MedicationService);
  private readonly patientApi = inject(PatientService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly meds = signal<MedicationListItem[]>([]);
  readonly patients = signal<PatientLookup[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly statusTab = signal<StatusTab>('all');

  readonly filterForm = this.fb.nonNullable.group({
    patientId: [''],
    patientName: [''],
    drugName: [''],
    route: [''],
    startDate: [''],
    endDate: ['']
  });

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const tab = this.statusTab();
    return this.meds().filter((m) => {
      if (tab === 'Active' && m.status !== 'Active') return false;
      if (tab === 'Inactive' && m.status === 'Active') return false;
      if (term) {
        const blob = `${m.patientName ?? ''} ${m.patientId} ${m.drugName} ${m.dose ?? ''} ${m.frequency ?? ''} ${m.route ?? ''}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  readonly stats = computed(() => {
    const all = this.meds();
    const active = all.filter((m) => m.status === 'Active').length;
    return { total: all.length, active, inactive: all.length - active };
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
    const filter: MedicationFilter = {
      patientId: v.patientId ? Number(v.patientId) : null,
      patientName: v.patientName || null,
      drugName: v.drugName || null,
      route: v.route || null,
      startDate: v.startDate || null,
      endDate: v.endDate || null
    };
    this.api.list(filter).subscribe({
      next: (list) => {
        this.meds.set(list ?? []);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.meds.set([]);
        this.loading.set(false);
        if (err.status !== 404) {
          this.toast.error(this.errorText(err, 'Could not load medications'));
        }
      }
    });
  }

  applyFilters(): void {
    this.refresh();
  }

  resetFilters(): void {
    this.filterForm.reset({
      patientId: '',
      patientName: '',
      drugName: '',
      route: '',
      startDate: '',
      endDate: ''
    });
    this.refresh();
  }

  setSearch(v: string): void {
    this.searchTerm.set(v);
  }

  setStatusTab(v: StatusTab): void {
    this.statusTab.set(v);
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

  drugIcon(name: string): string {
    const n = (name ?? '').toLowerCase();
    if (n.includes('insulin') || n.includes('inject')) return 'bi-eyedropper';
    if (n.includes('syrup') || n.includes('drop')) return 'bi-droplet';
    if (n.includes('inhaler') || n.includes('spray')) return 'bi-wind';
    if (n.includes('cream') || n.includes('ointment') || n.includes('topical')) return 'bi-bandaid';
    return 'bi-capsule';
  }

  routeIcon(route: string | null | undefined): string {
    const r = (route ?? '').toLowerCase();
    if (r.includes('iv') || r.includes('intravenous')) return 'bi-eyedropper';
    if (r.includes('inhal')) return 'bi-wind';
    if (r.includes('top') || r.includes('skin')) return 'bi-bandaid';
    if (r.includes('inject') || r.includes('im') || r.includes('subcut')) return 'bi-eyedropper';
    return 'bi-capsule';
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
