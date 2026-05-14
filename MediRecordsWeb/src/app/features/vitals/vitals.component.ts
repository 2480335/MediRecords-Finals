import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { EncounterService } from '../../core/services/encounter.service';
import { ToastService } from '../../core/services/toast.service';
import { VitalSignService } from '../../core/services/vital-sign.service';
import { SearchableSelectComponent, SearchableOption } from '../../shared/components/searchable-select/searchable-select.component';
import {
  BmiCategory,
  classifyBmi,
  VitalSignCaptureResponse
} from '../../core/models/vital-sign.models';
import { EncounterLookup } from '../../core/models/encounter.models';

interface CapturedVital {
  vitalId: number;
  encounterId: number;
  bp?: string | null;
  hr?: string | null;
  temp?: string | null;
  spO2?: string | null;
  height?: number | null;
  weight?: number | null;
  bmi?: number | null;
  capturedAt: Date;
}

@Component({
  selector: 'app-vitals',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './vitals.component.html',
  styleUrl: './vitals.component.scss'
})
export class VitalsComponent implements OnInit {
  private readonly api = inject(VitalSignService);
  private readonly encounterApi = inject(EncounterService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  readonly encounters = signal<EncounterLookup[]>([]);
  readonly lockedEncounter = signal<{ id: number; patientName: string } | null>(null);
  readonly encounterOptions = computed<SearchableOption[]>(() =>
    this.encounters().map((e) => ({ value: e.encounterId, label: `#${e.encounterId}` }))
  );

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const encId = Number(params.get('encounterId'));
    if (Number.isFinite(encId) && encId > 0) {
      this.lockedEncounter.set({
        id: encId,
        patientName: params.get('patientName') ?? ''
      });
      this.form.patchValue({ encounterId: encId });
    } else {
      this.encounterApi.getAll().subscribe({
        next: (list) => this.encounters.set(list),
        error: () => this.toast.error('Could not load encounter list')
      });
    }
  }

  readonly form = this.fb.nonNullable.group({
    encounterId: [0, [Validators.required, Validators.min(1)]],
    bp: [''],
    hr: [''],
    temp: [''],
    spO2: [''],
    height: [null as number | null],
    weight: [null as number | null]
  });

  readonly saving = signal(false);
  readonly history = signal<CapturedVital[]>([]);

  /** Live BMI preview (server returns the authoritative one). */
  readonly previewBmi = computed<number | null>(() => {
    const v = this.form.getRawValue();
    if (v.height == null || v.weight == null) return null;
    const heightM = Number(v.height) / 100;
    const weightKg = Number(v.weight);
    if (!heightM || heightM <= 0 || !weightKg || weightKg <= 0) return null;
    return Math.round((weightKg / (heightM * heightM)) * 10) / 10;
  });

  readonly previewBmiCategory = computed<BmiCategory | null>(() =>
    classifyBmi(this.previewBmi())
  );

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    if (!v.bp && !v.hr && !v.temp && !v.spO2 && v.height == null && v.weight == null) {
      this.toast.error('Capture at least one vital sign.');
      return;
    }

    this.saving.set(true);
    this.api
      .capture({
        encounterId: Number(v.encounterId),
        bp: v.bp || null,
        hr: v.hr || null,
        temp: v.temp || null,
        spO2: v.spO2 || null,
        height: v.height != null ? Number(v.height) : null,
        weight: v.weight != null ? Number(v.weight) : null
      })
      .subscribe({
        next: (res: VitalSignCaptureResponse) => {
          this.saving.set(false);
          this.toast.success('Vitals captured');
          this.history.update((list) => [
            {
              vitalId: res.vitalId,
              encounterId: Number(v.encounterId),
              bp: v.bp || null,
              hr: v.hr || null,
              temp: v.temp || null,
              spO2: v.spO2 || null,
              height: v.height != null ? Number(v.height) : null,
              weight: v.weight != null ? Number(v.weight) : null,
              bmi: res.bmi ?? this.previewBmi(),
              capturedAt: new Date()
            },
            ...list
          ]);
          // Keep encounter ID, clear vital fields
          this.form.patchValue({
            bp: '', hr: '', temp: '', spO2: '', height: null, weight: null
          });
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(this.errorText(err, 'Could not capture vitals'));
        }
      });
  }

  classify(bmi: number | null | undefined): BmiCategory | null {
    return classifyBmi(bmi);
  }

  clearHistory(): void {
    this.history.set([]);
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
