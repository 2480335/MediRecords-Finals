import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  HostListener,
  Input,
  OnChanges,
  SimpleChanges,
  ViewChild,
  computed,
  forwardRef,
  signal
} from '@angular/core';
import {
  ControlValueAccessor,
  FormsModule,
  NG_VALUE_ACCESSOR
} from '@angular/forms';

export interface SearchableOption {
  value: number | string;
  label: string;
}

/**
 * Reusable searchable dropdown that implements ControlValueAccessor so it
 * plugs into Reactive Forms via `formControlName` exactly like a native
 * `<select>`. The user can type to filter the visible options. Used across
 * patient, provider, and encounter pickers.
 */
@Component({
  selector: 'app-searchable-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './searchable-select.component.html',
  styleUrl: './searchable-select.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchableSelectComponent),
      multi: true
    }
  ]
})
export class SearchableSelectComponent implements ControlValueAccessor, OnChanges {
  @Input() options: SearchableOption[] = [];
  @Input() placeholder = '-- Select --';
  /** Label rendered for the "no selection" option. Set null to hide it. */
  @Input() emptyLabel: string | null = null;
  /** Value to emit when the empty option is chosen. Defaults to '' (any-filter). */
  @Input() emptyValue: number | string = '';

  @ViewChild('searchInput') searchInput?: ElementRef<HTMLInputElement>;

  readonly open = signal(false);
  readonly query = signal('');
  readonly selectedValue = signal<number | string | null>(null);
  readonly disabledState = signal(false);

  readonly selectedLabel = computed(() => {
    const v = this.selectedValue();
    if (v === null || v === '' || v === 0) {
      return this.emptyLabel ?? '';
    }
    const match = this.options.find((o) => o.value === v);
    return match?.label ?? '';
  });

  readonly filtered = computed(() => {
    const q = this.query().trim().toLowerCase();
    if (!q) return this.options;
    return this.options.filter(
      (o) =>
        o.label.toLowerCase().includes(q) ||
        String(o.value).toLowerCase().includes(q)
    );
  });

  /* ─── ControlValueAccessor ─── */
  private onChange: (v: number | string | null) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(v: number | string | null): void {
    this.selectedValue.set(v);
  }
  registerOnChange(fn: (v: number | string | null) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabledState.set(isDisabled);
  }

  /* ─── Interaction ─── */
  toggle(): void {
    if (this.disabledState()) return;
    if (this.open()) {
      this.close();
    } else {
      this.open.set(true);
      this.query.set('');
      setTimeout(() => this.searchInput?.nativeElement.focus(), 0);
    }
  }

  close(): void {
    this.open.set(false);
    this.onTouched();
  }

  select(o: SearchableOption): void {
    this.selectedValue.set(o.value);
    this.onChange(o.value);
    this.close();
  }

  selectEmpty(): void {
    this.selectedValue.set(this.emptyValue);
    this.onChange(this.emptyValue);
    this.close();
  }

  setQuery(v: string): void {
    this.query.set(v);
  }

  trackByValue = (_: number, o: SearchableOption): number | string => o.value;

  /* ─── Close on outside click ─── */
  constructor(private readonly host: ElementRef<HTMLElement>) {}

  @HostListener('document:click', ['$event'])
  onDocClick(event: MouseEvent): void {
    if (!this.open()) return;
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.close();
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.open()) this.close();
  }

  ngOnChanges(_: SimpleChanges): void {
    /* keep selectedLabel reactive when options arrive after writeValue */
  }
}
