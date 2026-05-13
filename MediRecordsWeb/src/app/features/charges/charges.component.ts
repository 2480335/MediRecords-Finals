import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { BillingService } from '../../core/services/billing.service';
import { EncounterService } from '../../core/services/encounter.service';
import { ProcedureCodeService } from '../../core/services/procedure-code.service';
import { ToastService } from '../../core/services/toast.service';
import { VisitChargeResponse } from '../../core/models/billing.models';
import { EncounterLookup } from '../../core/models/encounter.models';
import { ProcedureCodeView } from '../../core/models/procedure-code.models';

@Component({
  selector: 'app-charges',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './charges.component.html',
  styleUrl: './charges.component.scss'
})
export class ChargesComponent implements OnInit {
  private readonly billing = inject(BillingService);
  private readonly procedures = inject(ProcedureCodeService);
  private readonly encounterApi = inject(EncounterService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  readonly canEditAmount = computed(() => this.auth.role() === 'Admin');
  readonly encounters = signal<EncounterLookup[]>([]);
  readonly lockedEncounter = signal<{ id: number; patientName: string } | null>(null);

  // String-backed control to avoid edge cases with Angular's NumberValueAccessor
  // (empty input ⇒ null on number inputs sometimes leaves the control invalid
  // even when the user typed digits). We parse manually in search().
  readonly encounterIdControl = new FormControl<string>('', {
    nonNullable: true,
    validators: [
      Validators.required,
      Validators.pattern(/^\s*\d+\s*$/)
    ]
  });

  readonly currentEncounterId = signal<number | null>(null);
  readonly charges = signal<VisitChargeResponse[]>([]);
  readonly loading = signal(false);
  readonly hasSearched = signal(false);
  readonly lastError = signal<string | null>(null);

  readonly procedureCodes = signal<ProcedureCodeView[]>([]);
  readonly codesLoading = signal(true);

  /* ── Modals ── */
  readonly showAssign = signal(false);
  readonly showEdit = signal(false);
  readonly editing = signal<VisitChargeResponse | null>(null);
  readonly saving = signal(false);

  readonly assignForm = this.fb.nonNullable.group({
    codeId: [0, [Validators.required, Validators.min(1)]],
    amount: [null as number | null]
  });

  readonly editForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]]
  });

  /* ── Computed summaries ── */
  readonly summary = computed(() => {
    const list = this.charges();
    const total = list.length;
    const billed = list.filter((c) => this.isBilled(c.status)).length;
    const unbilled = total - billed;
    const amount = list.reduce((s, c) => s + (c.amount ?? 0), 0);
    const unbilledAmount = list
      .filter((c) => !this.isBilled(c.status))
      .reduce((s, c) => s + (c.amount ?? 0), 0);
    return { total, billed, unbilled, amount, unbilledAmount };
  });

  ngOnInit(): void {
    this.procedures.getAll().subscribe({
      next: (list) => {
        this.procedureCodes.set(list);
        this.codesLoading.set(false);
      },
      error: () => this.codesLoading.set(false)
    });

    const params = this.route.snapshot.queryParamMap;
    const encId = Number(params.get('encounterId'));
    if (Number.isFinite(encId) && encId > 0) {
      this.lockedEncounter.set({
        id: encId,
        patientName: params.get('patientName') ?? ''
      });
      // Auto-load charges for this encounter
      this.encounterIdControl.setValue(String(encId));
      this.search();
    } else {
      this.encounterApi.getAll().subscribe({
        next: (list) => this.encounters.set(list),
        error: () => this.toast.error('Could not load encounter list')
      });
    }
  }

  /* ── Lookup ── */
  search(): void {
    if (this.encounterIdControl.invalid) {
      this.encounterIdControl.markAsTouched();
      return;
    }
    const raw = (this.encounterIdControl.value ?? '').trim();
    const id = Number(raw);
    if (!Number.isFinite(id) || id <= 0) {
      this.encounterIdControl.markAsTouched();
      return;
    }
    this.currentEncounterId.set(id);
    this.lastError.set(null);
    this.loadCharges(id);
  }

  reset(): void {
    this.encounterIdControl.reset('');
    this.currentEncounterId.set(null);
    this.charges.set([]);
    this.hasSearched.set(false);
    this.lastError.set(null);
  }

  loadCharges(encounterId: number): void {
    this.loading.set(true);
    this.hasSearched.set(true);
    this.lastError.set(null);

    this.billing.getChargesByEncounter(encounterId).subscribe({
      next: (list) => {
        this.charges.set(Array.isArray(list) ? list : []);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.charges.set([]);
        this.loading.set(false);

        // 404 = encounter not found OR no charges yet — treat as empty state,
        // not as an error. The page already shows an "Assign first charge" CTA.
        if (err.status === 404) {
          return;
        }

        // Surface real errors clearly so the user (and we) can debug.
        const msg = this.describeHttpError(err, 'Could not load charges');
        this.lastError.set(msg);
        this.toast.error(msg);
        // eslint-disable-next-line no-console
        console.error('[Charges] GET encounters/charges failed', err);
      }
    });
  }

  /* ── Assign ── */
  openAssign(): void {
    this.assignForm.reset({ codeId: 0, amount: null });
    this.showAssign.set(true);
  }

  closeAssign(): void {
    this.showAssign.set(false);
  }

  onCodeChange(): void {
    const code = this.procedureCodes().find(
      (c) => c.codeId === Number(this.assignForm.controls.codeId.value)
    );
    if (code) {
      this.assignForm.patchValue({ amount: code.price });
    }
  }

  submitAssign(): void {
    const id = this.currentEncounterId();
    if (!id || this.assignForm.invalid) {
      this.assignForm.markAllAsTouched();
      return;
    }
    const v = this.assignForm.getRawValue();
    this.saving.set(true);
    this.billing
      .assignVisitCharge({
        encounterId: id,
        codeId: Number(v.codeId),
        amount: v.amount != null ? Number(v.amount) : null
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Charge assigned');
          this.closeAssign();
          this.loadCharges(id);
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(this.describeHttpError(err, 'Could not assign charge'));
          // eslint-disable-next-line no-console
          console.error('[Charges] POST visit-charges failed', err);
        }
      });
  }

  /* ── Edit amount ── */
  openEdit(charge: VisitChargeResponse): void {
    if (this.isBilled(charge.status)) {
      this.toast.error('Already-billed charges cannot be edited.');
      return;
    }
    this.editing.set(charge);
    this.editForm.reset({ amount: charge.amount });
    this.showEdit.set(true);
  }

  closeEdit(): void {
    this.showEdit.set(false);
    this.editing.set(null);
  }

  submitEdit(): void {
    const target = this.editing();
    const id = this.currentEncounterId();
    if (!target || !id || this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    const amount = Number(this.editForm.controls.amount.value);
    this.saving.set(true);
    this.billing.updateVisitChargeAmount(target.chargeId, { amount }).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Amount updated');
        this.closeEdit();
        this.loadCharges(id);
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.describeHttpError(err, 'Could not update amount'));
        // eslint-disable-next-line no-console
        console.error('[Charges] PUT visit-charges failed', err);
      }
    });
  }

  /* ── Helpers ── */
  isBilled(status: string): boolean {
    const s = (status ?? '').toString().trim().toLowerCase();
    return s === 'billed' || s === 'true';
  }

  iconForCode(code: string): string {
    const upper = (code ?? '').toUpperCase();
    if (upper.includes('XRAY')) return 'bi-radioactive';
    if (upper.includes('CT')) return 'bi-bullseye';
    if (upper.includes('MRI')) return 'bi-soundwave';
    if (upper.includes('ULTRA')) return 'bi-mic';
    if (upper.includes('BLOOD')) return 'bi-droplet-half';
    if (upper.includes('CONSULT')) return 'bi-chat-square-text';
    return 'bi-receipt';
  }

  private describeHttpError(err: HttpErrorResponse, fallback: string): string {
    if (err.status === 0) {
      return 'Cannot reach the API. Check that the backend is running and CORS is configured.';
    }
    if (err.status === 401) {
      return 'Your session has expired. Please sign in again.';
    }
    if (err.status === 403) {
      return 'You do not have permission to view these charges.';
    }
    let detail = '';
    if (typeof err.error === 'string') detail = err.error;
    else detail = err.error?.message ?? err.error?.error ?? '';
    return detail ? `${fallback}: ${detail}` : `${fallback} (HTTP ${err.status}).`;
  }
}
