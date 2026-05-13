import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProcedureCodeService } from '../../../core/services/procedure-code.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  PROCEDURE_KEYS,
  ProcedureCodeCreateRequest,
  ProcedureCodeKey,
  ProcedureCodeUpdateRequest,
  ProcedureCodeView
} from '../../../core/models/procedure-code.models';

@Component({
  selector: 'app-admin-procedure-codes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './procedure-codes.component.html',
  styleUrl: './procedure-codes.component.scss'
})
export class ProcedureCodesComponent implements OnInit {
  private readonly api = inject(ProcedureCodeService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly codes = signal<ProcedureCodeView[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly procedureKeys = PROCEDURE_KEYS;

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) return this.codes();
    return this.codes().filter((c) => {
      const blob = `${c.code} ${c.description}`.toLowerCase();
      return blob.includes(term);
    });
  });

  readonly totalValue = computed(() =>
    this.codes().reduce((sum, c) => sum + (c.price ?? 0), 0)
  );

  readonly avgPrice = computed(() => {
    const arr = this.codes();
    if (!arr.length) return 0;
    return arr.reduce((s, c) => s + (c.price ?? 0), 0) / arr.length;
  });

  readonly showCreate = signal(false);
  readonly showEdit = signal(false);
  readonly editing = signal<ProcedureCodeView | null>(null);
  readonly saving = signal(false);

  readonly createForm = this.fb.nonNullable.group({
    code: ['Consult' as ProcedureCodeKey, Validators.required],
    description: ['', [Validators.required, Validators.maxLength(255)]],
    price: [0, [Validators.required, Validators.min(0.01)]]
  });

  readonly editForm = this.fb.nonNullable.group({
    description: ['', [Validators.required, Validators.maxLength(255)]],
    price: [0, [Validators.required, Validators.min(0.01)]]
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.api.getAll().subscribe({
      next: (list) => {
        this.codes.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Failed to load procedure codes');
      }
    });
  }

  setSearch(value: string): void {
    this.searchTerm.set(value);
  }

  /* ─── Create ─── */
  openCreate(): void {
    this.createForm.reset({ code: 'Consult', description: '', price: 0 });
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
    const payload: ProcedureCodeCreateRequest = {
      code: v.code,
      description: v.description,
      price: Number(v.price)
    };
    this.saving.set(true);
    this.api.create(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Procedure code created');
        this.closeCreate();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not create procedure code'));
      }
    });
  }

  /* ─── Edit ─── */
  openEdit(code: ProcedureCodeView): void {
    this.editing.set(code);
    this.editForm.reset({
      description: code.description,
      price: code.price
    });
    this.showEdit.set(true);
  }

  closeEdit(): void {
    this.showEdit.set(false);
    this.editing.set(null);
  }

  submitEdit(): void {
    const code = this.editing();
    if (!code || this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    const v = this.editForm.getRawValue();
    const payload: ProcedureCodeUpdateRequest = {
      description: v.description,
      price: Number(v.price)
    };
    this.saving.set(true);
    this.api.update(code.codeId, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Procedure code updated');
        this.closeEdit();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not update procedure code'));
      }
    });
  }

  iconFor(code: string): string {
    const upper = (code ?? '').toUpperCase();
    if (upper.includes('XRAY')) return 'bi-radioactive';
    if (upper.includes('CT')) return 'bi-bullseye';
    if (upper.includes('MRI')) return 'bi-soundwave';
    if (upper.includes('ULTRA')) return 'bi-mic';
    if (upper.includes('BLOOD')) return 'bi-droplet-half';
    if (upper.includes('CONSULT')) return 'bi-chat-square-text';
    return 'bi-receipt';
  }

  private errorText(err: HttpErrorResponse, fallback: string): string {
    if (typeof err.error === 'string') return err.error;
    return err.error?.message ?? err.error?.error ?? fallback;
  }
}
