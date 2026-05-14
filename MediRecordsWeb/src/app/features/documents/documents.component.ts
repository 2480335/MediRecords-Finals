import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { DocumentService } from '../../core/services/document.service';
import { EncounterService } from '../../core/services/encounter.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import { SearchableSelectComponent, SearchableOption } from '../../shared/components/searchable-select/searchable-select.component';
import {
  DOC_TYPES,
  DocType,
  DocumentListItem,
  DocumentUploadResponse
} from '../../core/models/document.models';
import { EncounterLookup } from '../../core/models/encounter.models';
import { PatientLookup } from '../../core/models/patient.models';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './documents.component.html',
  styleUrl: './documents.component.scss'
})
export class DocumentsComponent implements OnInit {
  private readonly api = inject(DocumentService);
  private readonly patientApi = inject(PatientService);
  private readonly encounterApi = inject(EncounterService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  readonly docTypes = DOC_TYPES;
  readonly patients = signal<PatientLookup[]>([]);
  readonly encounters = signal<EncounterLookup[]>([]);
  readonly lockedEncounter = signal<{ id: number; patientId: number; patientName: string } | null>(null);

  readonly patientOptions = computed<SearchableOption[]>(() =>
    this.patients().map((p) => ({ value: p.patientId, label: p.name }))
  );
  readonly encounterOptions = computed<SearchableOption[]>(() =>
    this.encounters().map((e) => ({ value: e.encounterId, label: `#${e.encounterId}` }))
  );

  readonly form = this.fb.nonNullable.group({
    patientId: [0, [Validators.required, Validators.min(1)]],
    encounterId: [0, [Validators.required, Validators.min(1)]],
    docType: ['Report' as DocType, Validators.required],
    providerOnlyVisibility: [false]
  });

  readonly file = signal<File | null>(null);
  readonly fileError = signal<string | null>(null);
  readonly dragActive = signal(false);
  readonly saving = signal(false);
  readonly uploaded = signal<DocumentUploadResponse[]>([]);

  /* ── Library (search + download) ── */
  readonly searchForm = this.fb.nonNullable.group({
    patientId: [''],
    encounterId: [''],
    uploadedDate: ['']
  });
  readonly library = signal<DocumentListItem[]>([]);
  readonly librarySearched = signal(false);
  readonly libraryLoading = signal(false);
  readonly downloadingId = signal<number | null>(null);

  ngOnInit(): void {
    this.refreshLibrary();
    const params = this.route.snapshot.queryParamMap;
    const encId = Number(params.get('encounterId'));
    const patId = Number(params.get('patientId'));
    if (Number.isFinite(encId) && encId > 0 && Number.isFinite(patId) && patId > 0) {
      this.lockedEncounter.set({
        id: encId,
        patientId: patId,
        patientName: params.get('patientName') ?? ''
      });
      this.form.patchValue({ encounterId: encId, patientId: patId });
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

  setDocType(value: DocType): void {
    this.form.patchValue({ docType: value });
  }

  selectFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const f = input.files?.[0];
    this.handleFile(f);
    input.value = '';
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragActive.set(false);
    const f = event.dataTransfer?.files?.[0];
    this.handleFile(f);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragActive.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragActive.set(false);
  }

  private handleFile(f: File | null | undefined): void {
    this.fileError.set(null);
    if (!f) return;
    if (f.size === 0) {
      this.fileError.set('File is empty.');
      return;
    }
    if (f.size > 10 * 1024 * 1024) {
      this.fileError.set('File exceeds 10 MB.');
      return;
    }
    this.file.set(f);
  }

  removeFile(): void {
    this.file.set(null);
  }

  fileLabel(): string {
    const f = this.file();
    if (!f) return '';
    return `${f.name} · ${this.formatSize(f.size)}`;
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  fileIcon(name: string): string {
    const ext = (name.split('.').pop() ?? '').toLowerCase();
    if (['png', 'jpg', 'jpeg', 'gif', 'webp', 'bmp'].includes(ext)) return 'bi-file-image';
    if (ext === 'pdf') return 'bi-file-pdf';
    if (['doc', 'docx'].includes(ext)) return 'bi-file-word';
    if (['xls', 'xlsx', 'csv'].includes(ext)) return 'bi-file-excel';
    if (['zip', 'rar', '7z'].includes(ext)) return 'bi-file-zip';
    return 'bi-file-earmark';
  }

  toneFor(docType: string): string {
    return DOC_TYPES.find((d) => d.value === docType)?.tone ?? 'primary';
  }

  iconFor(docType: string): string {
    return DOC_TYPES.find((d) => d.value === docType)?.icon ?? 'bi-file-earmark';
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    if (!this.file()) {
      this.fileError.set('Choose a file to upload.');
      return;
    }

    const v = this.form.getRawValue();
    this.saving.set(true);

    this.api
      .upload({
        patientId: Number(v.patientId),
        encounterId: Number(v.encounterId),
        docType: v.docType,
        providerOnlyVisibility: !!v.providerOnlyVisibility,
        file: this.file()!
      })
      .subscribe({
        next: (res) => {
          this.saving.set(false);
          this.toast.success('Document uploaded');
          this.uploaded.update((list) => [res, ...list]);
          this.form.patchValue({ providerOnlyVisibility: false });
          this.file.set(null);
          this.refreshLibrary();
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(this.errorText(err, 'Upload failed'));
        }
      });
  }

  clearLog(): void {
    this.uploaded.set([]);
  }

  /* ── Library actions ── */

  refreshLibrary(): void {
    const v = this.searchForm.getRawValue();
    const patientId = v.patientId ? Number(v.patientId) : null;
    const encounterId = v.encounterId ? Number(v.encounterId) : null;
    const uploadedDate = v.uploadedDate || null;

    this.libraryLoading.set(true);
    this.librarySearched.set(true);

    this.api
      .search({
        patientId,
        encounterId,
        uploadedDate
      })
      .subscribe({
        next: (rows) => {
          this.library.set(rows ?? []);
          this.libraryLoading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.library.set([]);
          this.libraryLoading.set(false);
          this.toast.error(this.errorText(err, 'Failed to load documents'));
        }
      });
  }

  resetSearch(): void {
    this.searchForm.reset({ patientId: '', encounterId: '', uploadedDate: '' });
    this.refreshLibrary();
  }

  download(doc: DocumentListItem): void {
    this.downloadingId.set(doc.documentId);
    this.api.download(doc.documentId).subscribe({
      next: ({ blob, fileName }) => {
        triggerBrowserDownload(blob, fileName || doc.fileName);
        this.downloadingId.set(null);
        this.toast.success(`Downloaded ${fileName || doc.fileName}`);
      },
      error: (err: HttpErrorResponse) => {
        this.downloadingId.set(null);
        this.toast.error(this.errorText(err, 'Download failed'));
      }
    });
  }

  trackDoc(_: number, doc: DocumentListItem): number {
    return doc.documentId;
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}

function triggerBrowserDownload(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = fileName;
  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);
  URL.revokeObjectURL(url);
}
