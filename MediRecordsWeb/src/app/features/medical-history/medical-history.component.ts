import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MedicalHistoryService } from '../../core/services/medical-history.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import {
  COMMON_CONDITIONS,
  MedicalHistoryCreateRequest
} from '../../core/models/medical-history.models';
import { PatientLookup } from '../../core/models/patient.models';

interface RecordedHistory extends MedicalHistoryCreateRequest {
  patientId: number;
  recordedAt: Date;
}

@Component({
  selector: 'app-medical-history',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './medical-history.component.html',
  styleUrl: './medical-history.component.scss'
})
export class MedicalHistoryComponent implements OnInit {
  private readonly api = inject(MedicalHistoryService);
  private readonly patientApi = inject(PatientService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly commonConditions = COMMON_CONDITIONS;
  readonly patients = signal<PatientLookup[]>([]);

  ngOnInit(): void {
    this.patientApi.getAll().subscribe({
      next: (list) => this.patients.set(list),
      error: () => this.toast.error('Could not load patient list')
    });
  }

  readonly form = this.fb.nonNullable.group({
    patientId: [0, [Validators.required, Validators.min(1)]],
    condition: ['', [Validators.required, Validators.maxLength(255)]],
    notes: ['']
  });

  readonly saving = signal(false);
  readonly recent = signal<RecordedHistory[]>([]);

  pickCondition(name: string): void {
    this.form.patchValue({ condition: name });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const payload: MedicalHistoryCreateRequest = {
      condition: v.condition,
      notes: v.notes || null
    };

    this.saving.set(true);
    this.api.recordForPatient(Number(v.patientId), payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Medical history recorded');
        this.recent.update((list) => [
          {
            patientId: Number(v.patientId),
            condition: v.condition,
            notes: v.notes || null,
            recordedAt: new Date()
          },
          ...list
        ]);
        this.form.patchValue({ condition: '', notes: '' });
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not record medical history'));
      }
    });
  }

  clearRecent(): void {
    this.recent.set([]);
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
