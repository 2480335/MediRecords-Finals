import {
  Directive,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  inject
} from '@angular/core';

@Directive({
  selector: '[appCountUp]',
  standalone: true
})
export class CountUpDirective implements OnChanges, OnDestroy {
  @Input('appCountUp') target = 0;
  @Input() duration = 900;
  @Input() decimals = 0;
  @Input() prefix = '';
  @Input() suffix = '';

  private readonly el = inject(ElementRef<HTMLElement>);
  private rafId: number | null = null;

  ngOnChanges(): void {
    this.animate();
  }

  ngOnDestroy(): void {
    if (this.rafId !== null) cancelAnimationFrame(this.rafId);
  }

  private animate(): void {
    if (this.rafId !== null) cancelAnimationFrame(this.rafId);

    const node = this.el.nativeElement;
    const target = Number(this.target ?? 0);
    if (!Number.isFinite(target)) {
      node.textContent = `${this.prefix}0${this.suffix}`;
      return;
    }

    const start = performance.now();
    const from = 0;
    const tick = (now: number) => {
      const t = Math.min(1, (now - start) / this.duration);
      const eased = 1 - Math.pow(1 - t, 3);
      const value = from + (target - from) * eased;
      node.textContent = this.format(value);
      if (t < 1) {
        this.rafId = requestAnimationFrame(tick);
      } else {
        node.textContent = this.format(target);
        this.rafId = null;
      }
    };
    this.rafId = requestAnimationFrame(tick);
  }

  private format(value: number): string {
    const fixed = value.toFixed(this.decimals);
    const [int, dec] = fixed.split('.');
    const grouped = int.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    const out = dec ? `${grouped}.${dec}` : grouped;
    return `${this.prefix}${out}${this.suffix}`;
  }
}
