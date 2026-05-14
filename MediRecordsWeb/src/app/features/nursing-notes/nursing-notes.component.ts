import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { EncounterService } from '../../core/services/encounter.service';
import { NursingNoteService } from '../../core/services/nursing-note.service';
import { ToastService } from '../../core/services/toast.service';
import { SearchableSelectComponent, SearchableOption } from '../../shared/components/searchable-select/searchable-select.component';
import { NursingNoteResponse } from '../../core/models/nursing-note.models';
import { EncounterLookup } from '../../core/models/encounter.models';

const MAX_NOTES = 50;

@Component({
  selector: 'app-nursing-notes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './nursing-notes.component.html',
  styleUrl: './nursing-notes.component.scss'
})
export class NursingNotesComponent implements OnInit {
  private readonly api = inject(NursingNoteService);
  private readonly encounterApi = inject(EncounterService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  readonly maxLength = MAX_NOTES;
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
    notes: ['', [Validators.required, Validators.maxLength(MAX_NOTES)]]
  });

  readonly saving = signal(false);
  readonly notes = signal<NursingNoteResponse[]>([]);

  readonly remainingChars = computed(() => {
    const v = this.form.controls.notes.value ?? '';
    return MAX_NOTES - v.length;
  });

  /* ── Edit modal ── */
  readonly showEdit = signal(false);
  readonly editing = signal<NursingNoteResponse | null>(null);
  readonly editForm = this.fb.nonNullable.group({
    notes: ['', [Validators.required, Validators.maxLength(MAX_NOTES)]]
  });

  readonly editRemainingChars = computed(() => {
    const v = this.editForm.controls.notes.value ?? '';
    return MAX_NOTES - v.length;
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.saving.set(true);
    this.api.create(Number(v.encounterId), { notes: v.notes }).subscribe({
      next: (res) => {
        this.saving.set(false);
        this.toast.success('Nursing note added');
        this.notes.update((list) => [res, ...list]);
        this.form.patchValue({ notes: '' });
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not add note'));
      }
    });
  }

  openEdit(note: NursingNoteResponse): void {
    this.editing.set(note);
    this.editForm.reset({ notes: note.notes });
    this.showEdit.set(true);
  }

  closeEdit(): void {
    this.showEdit.set(false);
    this.editing.set(null);
  }

  submitEdit(): void {
    const target = this.editing();
    if (!target || this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    const v = this.editForm.getRawValue();
    this.saving.set(true);
    this.api.update(target.nursingNoteId, { notes: v.notes }).subscribe({
      next: (res) => {
        this.saving.set(false);
        this.toast.success('Nursing note updated');
        this.notes.update((list) =>
          list.map((n) => (n.nursingNoteId === target.nursingNoteId ? res : n))
        );
        this.closeEdit();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not update note'));
      }
    });
  }

  clearLog(): void {
    this.notes.set([]);
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

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
