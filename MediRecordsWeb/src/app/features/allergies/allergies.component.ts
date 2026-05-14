import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AllergyService } from '../../core/services/allergy.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import { SearchableSelectComponent, SearchableOption } from '../../shared/components/searchable-select/searchable-select.component';
import {
  AllergyCreateRequest,
  AllergySeverity,
  SEVERITY_OPTIONS
} from '../../core/models/allergy.models';
import { PatientLookup } from '../../core/models/patient.models';

interface RecordedAllergy extends AllergyCreateRequest {
  patientId: number;
  recordedAt: Date;
}

@Component({
  selector: 'app-allergies',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './allergies.component.html',
  styleUrl: './allergies.component.scss'
})
export class AllergiesComponent implements OnInit {
  private readonly api = inject(AllergyService);
  private readonly patientApi = inject(PatientService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly severities = SEVERITY_OPTIONS;
  readonly patients = signal<PatientLookup[]>([]);
  readonly patientOptions = computed<SearchableOption[]>(() =>
    this.patients().map((p) => ({ value: p.patientId, label: p.name }))
  );

  ngOnInit(): void {
    this.patientApi.getAll().subscribe({
      next: (list) => this.patients.set(list),
      error: () => this.toast.error('Could not load patient list')
    });
  }

  readonly form = this.fb.nonNullable.group({
    patientId: [0, [Validators.required, Validators.min(1)]],
    allergen: ['', [Validators.required, Validators.maxLength(255)]],
    reaction: [''],
    severity: ['Moderate' as AllergySeverity, Validators.required]
  });

  readonly saving = signal(false);
  readonly recent = signal<RecordedAllergy[]>([]);

  setSeverity(value: AllergySeverity): void {
    this.form.patchValue({ severity: value });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const payload: AllergyCreateRequest = {
      allergen: v.allergen,
      reaction: v.reaction || null,
      severity: v.severity
    };

    this.saving.set(true);
    this.api.recordForPatient(Number(v.patientId), payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Allergy recorded');
        this.recent.update((list) => [
          {
            patientId: Number(v.patientId),
            allergen: v.allergen,
            reaction: v.reaction || null,
            severity: v.severity,
            recordedAt: new Date()
          },
          ...list
        ]);
        this.form.patchValue({ allergen: '', reaction: '' });
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not record allergy'));
      }
    });
  }

  toneFor(severity: string | null | undefined): string {
    return SEVERITY_OPTIONS.find((s) => s.value === severity)?.tone ?? 'muted';
  }

  clearRecent(): void {
    this.recent.set([]);
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
