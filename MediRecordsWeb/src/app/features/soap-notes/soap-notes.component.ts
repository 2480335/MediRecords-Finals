import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { EncounterService } from '../../core/services/encounter.service';
import { SOAPNoteService } from '../../core/services/soap-note.service';
import { ToastService } from '../../core/services/toast.service';
import { SearchableSelectComponent, SearchableOption } from '../../shared/components/searchable-select/searchable-select.component';
import { SOAPNoteResponse } from '../../core/models/soap-note.models';
import { EncounterLookup } from '../../core/models/encounter.models';

type Section = 'S' | 'O' | 'A' | 'P';

interface SectionMeta {
  key: Section;
  letter: string;
  title: string;
  subtitle: string;
  icon: string;
  tone: string;
}

@Component({
  selector: 'app-soap-notes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './soap-notes.component.html',
  styleUrl: './soap-notes.component.scss'
})
export class SOAPNotesComponent implements OnInit {
  private readonly api = inject(SOAPNoteService);
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
    hpi: ['', [Validators.required, Validators.maxLength(5000)]],
    ros: [''],
    examFindings: [''],
    observations: [''],
    assessment: [''],
    plan: [''],
    isDraft: [true]
  });

  readonly saving = signal(false);
  readonly lastSaved = signal<SOAPNoteResponse | null>(null);
  readonly activeSection = signal<Section>('S');

  readonly sections: SectionMeta[] = [
    {
      key: 'S',
      letter: 'S',
      title: 'Subjective',
      subtitle: 'What the patient says',
      icon: 'bi-chat-quote',
      tone: 'primary'
    },
    {
      key: 'O',
      letter: 'O',
      title: 'Objective',
      subtitle: 'What you observe',
      icon: 'bi-clipboard2-data',
      tone: 'violet'
    },
    {
      key: 'A',
      letter: 'A',
      title: 'Assessment',
      subtitle: 'Your clinical judgment',
      icon: 'bi-lightbulb',
      tone: 'amber'
    },
    {
      key: 'P',
      letter: 'P',
      title: 'Plan',
      subtitle: 'What happens next',
      icon: 'bi-flag',
      tone: 'success'
    }
  ];

  readonly progress = computed(() => {
    const v = this.form.getRawValue();
    const total = 5;
    let filled = 0;
    if (v.hpi?.trim()) filled++;
    if (v.examFindings?.trim() || v.observations?.trim()) filled++;
    if (v.ros?.trim()) filled++;
    if (v.assessment?.trim()) filled++;
    if (v.plan?.trim()) filled++;
    return Math.round((filled / total) * 100);
  });

  readonly subjectiveFilled = computed(() => {
    const v = this.form.getRawValue();
    return !!v.hpi?.trim() || !!v.ros?.trim();
  });

  readonly objectiveFilled = computed(() => {
    const v = this.form.getRawValue();
    return !!v.examFindings?.trim() || !!v.observations?.trim();
  });

  readonly assessmentFilled = computed(() => {
    const v = this.form.getRawValue();
    return !!v.assessment?.trim();
  });

  readonly planFilled = computed(() => {
    const v = this.form.getRawValue();
    return !!v.plan?.trim();
  });

  setSection(section: Section): void {
    this.activeSection.set(section);
  }

  isSectionFilled(key: Section): boolean {
    if (key === 'S') return this.subjectiveFilled();
    if (key === 'O') return this.objectiveFilled();
    if (key === 'A') return this.assessmentFilled();
    return this.planFilled();
  }

  setIsDraft(value: boolean): void {
    this.form.patchValue({ isDraft: value });
  }

  saveAsDraft(): void {
    this.form.patchValue({ isDraft: true });
    this.submit();
  }

  signAndSave(): void {
    this.form.patchValue({ isDraft: false });
    this.submit();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.activeSection.set('S');
      return;
    }
    const v = this.form.getRawValue();
    this.saving.set(true);
    this.api
      .save({
        encounterId: Number(v.encounterId),
        hpi: v.hpi,
        ros: v.ros || null,
        examFindings: v.examFindings || null,
        observations: v.observations || null,
        assessment: v.assessment || null,
        plan: v.plan || null,
        isDraft: !!v.isDraft
      })
      .subscribe({
        next: (res) => {
          this.saving.set(false);
          this.lastSaved.set(res);
          this.toast.success(v.isDraft ? 'Draft saved' : 'SOAP note signed & saved');
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(this.errorText(err, 'Could not save SOAP note'));
        }
      });
  }

  reset(): void {
    this.form.reset({
      encounterId: 0,
      hpi: '',
      ros: '',
      examFindings: '',
      observations: '',
      assessment: '',
      plan: '',
      isDraft: true
    });
    this.lastSaved.set(null);
    this.activeSection.set('S');
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
