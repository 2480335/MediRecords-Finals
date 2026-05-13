import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-host',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-host" aria-live="polite" aria-atomic="true">
      @for (toast of toasts.toasts(); track toast.id) {
        <div class="toast-item" [ngClass]="'toast-' + toast.kind">
          <i class="bi" [ngClass]="iconFor(toast.kind)"></i>
          <span>{{ toast.message }}</span>
          <button type="button" class="toast-close" (click)="toasts.dismiss(toast.id)" aria-label="Close">
            <i class="bi bi-x"></i>
          </button>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .toast-host {
        position: fixed;
        right: 1.25rem;
        bottom: 1.25rem;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        z-index: 1080;
        pointer-events: none;
      }
      .toast-item {
        display: flex;
        align-items: center;
        gap: 0.625rem;
        padding: 0.75rem 0.875rem 0.75rem 1rem;
        background: #fff;
        border-radius: 12px;
        box-shadow: 0 12px 28px -8px rgba(15, 23, 42, 0.18);
        min-width: 280px;
        max-width: 380px;
        font-size: 0.875rem;
        font-weight: 500;
        pointer-events: auto;
        animation: slideUp 0.2s ease both;
        border: 1px solid #e2e8f0;
      }
      .toast-item i {
        font-size: 1.05rem;
      }
      .toast-success { color: #047857; border-color: #a7f3d0; background: #ecfdf5; }
      .toast-error { color: #b91c1c; border-color: #fecaca; background: #fef2f2; }
      .toast-info { color: #1d4ed8; border-color: #bfdbfe; background: #eff6ff; }
      .toast-close {
        margin-left: auto;
        border: none;
        background: transparent;
        color: inherit;
        font-size: 1.05rem;
        cursor: pointer;
        opacity: 0.7;
      }
      .toast-close:hover { opacity: 1; }
      @keyframes slideUp {
        from { opacity: 0; transform: translateY(8px); }
        to { opacity: 1; transform: translateY(0); }
      }
    `
  ]
})
export class ToastHostComponent {
  protected readonly toasts = inject(ToastService);

  iconFor(kind: string): string {
    switch (kind) {
      case 'success': return 'bi-check-circle-fill';
      case 'error': return 'bi-exclamation-triangle-fill';
      default: return 'bi-info-circle-fill';
    }
  }
}
