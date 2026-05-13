import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ImagingOrderService } from '../../core/services/imaging-order.service';
import { ToastService } from '../../core/services/toast.service';

interface SubmittedReport {
  imagingOrderId: number;
  findings: string;
  impression: string;
  fileName?: string | null;
  submittedAt: Date;
}

@Component({
  selector: 'app-imaging-reports',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './imaging-reports.component.html',
  styleUrl: './imaging-reports.component.scss'
})
export class ImagingReportsComponent {
  private readonly api = inject(ImagingOrderService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    imagingOrderId: [0, [Validators.required, Validators.min(1)]],
    findings: ['', [Validators.required, Validators.maxLength(5000)]],
    impression: ['', [Validators.required, Validators.maxLength(2000)]]
  });

  readonly file = signal<File | null>(null);
  readonly fileError = signal<string | null>(null);
  readonly dragActive = signal(false);
  readonly saving = signal(false);
  readonly submitted = signal<SubmittedReport[]>([]);

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
    if (f.size > 25 * 1024 * 1024) {
      this.fileError.set('File exceeds 25 MB.');
      return;
    }
    const ok = ['pdf', 'png', 'jpg', 'jpeg', 'dcm', 'dicom', 'gif', 'webp'].some((ext) =>
      f.name.toLowerCase().endsWith('.' + ext)
    );
    if (!ok) {
      this.fileError.set('Use PDF, an image, or a DICOM file.');
      return;
    }
    this.file.set(f);
  }

  removeFile(): void {
    this.file.set(null);
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  fileIcon(name: string): string {
    const ext = (name.split('.').pop() ?? '').toLowerCase();
    if (['png', 'jpg', 'jpeg', 'gif', 'webp'].includes(ext)) return 'bi-file-image';
    if (ext === 'pdf') return 'bi-file-pdf';
    if (['dcm', 'dicom'].includes(ext)) return 'bi-radioactive';
    return 'bi-file-earmark';
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.saving.set(true);
    const file = this.file();
    this.api
      .uploadReport({
        imagingOrderId: Number(v.imagingOrderId),
        findings: v.findings,
        impression: v.impression,
        reportFile: file
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Imaging report submitted');
          this.submitted.update((list) => [
            {
              imagingOrderId: Number(v.imagingOrderId),
              findings: v.findings,
              impression: v.impression,
              fileName: file?.name ?? null,
              submittedAt: new Date()
            },
            ...list
          ]);
          this.form.patchValue({ findings: '', impression: '' });
          this.file.set(null);
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(this.errorText(err, 'Upload failed'));
        }
      });
  }

  clearLog(): void {
    this.submitted.set([]);
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
