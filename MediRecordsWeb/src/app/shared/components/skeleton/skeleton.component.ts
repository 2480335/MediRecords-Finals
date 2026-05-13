import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="sk"
      [class.sk-circle]="shape === 'circle'"
      [class.sk-pill]="shape === 'pill'"
      [style.width]="width"
      [style.height]="height">
    </span>
  `,
  styles: [
    `
      :host { display: inline-block; }
      .sk {
        display: inline-block;
        background: linear-gradient(90deg, #eef2f7 0%, #f8fafc 50%, #eef2f7 100%);
        background-size: 200% 100%;
        animation: shimmer 1.4s ease-in-out infinite;
        border-radius: 6px;
      }
      .sk-circle { border-radius: 50%; }
      .sk-pill { border-radius: 999px; }
      @keyframes shimmer {
        0% { background-position: 100% 0; }
        100% { background-position: -100% 0; }
      }
    `
  ]
})
export class SkeletonComponent {
  @Input() width = '100%';
  @Input() height = '14px';
  @Input() shape: 'rect' | 'circle' | 'pill' = 'rect';
}
