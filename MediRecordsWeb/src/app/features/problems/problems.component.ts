import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProblemService } from '../../core/services/problem.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import {
  COMMON_DIAGNOSES,
  PROBLEM_STATUSES,
  ProblemCreateRequest,
  ProblemStatus
} from '../../core/models/problem.models';
import { PatientLookup } from '../../core/models/patient.models';

interface RecordedProblem extends ProblemCreateRequest {
  patientId: number;
  recordedAt: Date;
}

@Component({
  selector: 'app-problems',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './problems.component.html',
  styleUrl: './problems.component.scss'
})
export class ProblemsComponent implements OnInit {
  private readonly api = inject(ProblemService);
  private readonly patientApi = inject(PatientService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly statuses = PROBLEM_STATUSES;
  readonly commonDiagnoses = COMMON_DIAGNOSES;
  readonly patients = signal<PatientLookup[]>([]);

  ngOnInit(): void {
    this.patientApi.getAll().subscribe({
      next: (list) => this.patients.set(list),
      error: () => this.toast.error('Could not load patient list')
    });
  }

  readonly form = this.fb.nonNullable.group({
    patientId: [0, [Validators.required, Validators.min(1)]],
    diagnosis: ['', [Validators.required, Validators.maxLength(255)]],
    status: ['Active' as ProblemStatus, Validators.required],
    startDate: ['', Validators.required],
    endDate: ['']
  });

  readonly saving = signal(false);
  readonly recent = signal<RecordedProblem[]>([]);

  pickDiagnosis(name: string): void {
    this.form.patchValue({ diagnosis: name });
  }

  setStatus(status: ProblemStatus): void {
    this.form.patchValue({ status });
  }

  showEndDate(): boolean {
    const s = this.form.controls.status.value;
    return s === 'Resolved' || s === 'Discontinued';
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const startIso = new Date(`${v.startDate}T09:00:00`).toISOString();
    const endIso = v.endDate ? new Date(`${v.endDate}T09:00:00`).toISOString() : null;

    const payload: ProblemCreateRequest = {
      diagnosis: v.diagnosis,
      status: v.status,
      startDate: startIso,
      endDate: endIso
    };

    this.saving.set(true);
    this.api.recordForPatient(Number(v.patientId), payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Problem recorded');
        this.recent.update((list) => [
          {
            patientId: Number(v.patientId),
            diagnosis: v.diagnosis,
            status: v.status,
            startDate: startIso,
            endDate: endIso,
            recordedAt: new Date()
          },
          ...list
        ]);
        this.form.patchValue({ diagnosis: '', endDate: '' });
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not record problem'));
      }
    });
  }

  toneFor(status: string): string {
    return PROBLEM_STATUSES.find((s) => s.value === status)?.tone ?? 'muted';
  }

  iconFor(status: string): string {
    return PROBLEM_STATUSES.find((s) => s.value === status)?.icon ?? 'bi-tag';
  }

  clearRecent(): void {
    this.recent.set([]);
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
