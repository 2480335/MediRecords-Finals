import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';
import { UserService } from '../../../core/services/user.service';
import {
  RoleOption,
  UserRegisterRequest,
  UserUpdateRequest,
  UserView,
  describeRole
} from '../../../core/models/user.models';

const STRONG_PASSWORD = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

type StatusFilter = 'all' | 'active' | 'inactive';

const passwordsMatch: ValidatorFn = (
  group: AbstractControl
): ValidationErrors | null => {
  const pwd = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return pwd && confirm && pwd !== confirm ? { mismatch: true } : null;
};

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  private readonly api = inject(UserService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly users = signal<UserView[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly statusFilter = signal<StatusFilter>('all');
  readonly roleFilter = signal<string>('all');

  readonly roles = signal<RoleOption[]>([]);

  describe(name: string): string {
    return describeRole(name);
  }

  private roleNameToId(name: string): number | null {
    return this.roles().find((r) => r.name === name)?.roleId ?? null;
  }

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const status = this.statusFilter();
    const role = this.roleFilter();
    return this.users().filter((u) => {
      if (status === 'active' && u.status !== 'Active') return false;
      if (status === 'inactive' && u.status === 'Active') return false;
      if (role !== 'all' && u.roleName !== role) return false;
      if (term) {
        const blob = `${u.name} ${u.email} ${u.phone ?? ''} ${u.roleName}`.toLowerCase();
        if (!blob.includes(term)) return false;
      }
      return true;
    });
  });

  /* ───── Modal state ───── */
  readonly showCreate = signal(false);
  readonly showEdit = signal(false);
  readonly showDelete = signal(false);
  readonly editingUser = signal<UserView | null>(null);
  readonly deleting = signal<UserView | null>(null);
  readonly saving = signal(false);
  readonly showPwd = signal(false);

  readonly createForm = this.fb.nonNullable.group(
    {
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      roleId: [0, [Validators.required, Validators.min(1)]],
      status: [true],
      password: [
        '',
        [Validators.required, Validators.minLength(8), Validators.pattern(STRONG_PASSWORD)]
      ],
      confirmPassword: ['', Validators.required]
    },
    { validators: passwordsMatch }
  );

  readonly editForm = this.fb.nonNullable.group({
    userID: [0],
    name: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    roleID: [0, [Validators.required, Validators.min(1)]],
    status: [true]
  });

  ngOnInit(): void {
    this.api.getRoles().subscribe({
      next: (roles) => this.roles.set(roles ?? []),
      error: () => this.toast.error('Could not load role list')
    });
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.api.getAll().subscribe({
      next: (list) => {
        this.users.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Failed to load users');
      }
    });
  }

  setSearch(value: string): void {
    this.searchTerm.set(value);
  }

  setStatus(value: StatusFilter): void {
    this.statusFilter.set(value);
  }

  setRole(value: string): void {
    this.roleFilter.set(value);
  }

  /* ───── Create ───── */
  openCreate(): void {
    this.createForm.reset({
      name: '',
      email: '',
      phone: '',
      roleId: 0,
      status: true,
      password: '',
      confirmPassword: ''
    });
    this.showPwd.set(false);
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
    const payload: UserRegisterRequest = {
      name: v.name,
      email: v.email,
      phone: v.phone || undefined,
      roleId: Number(v.roleId),
      password: v.password,
      status: v.status
    };
    this.saving.set(true);
    this.api.register(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('User created');
        this.closeCreate();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not create user'));
      }
    });
  }

  /* ───── Edit ───── */
  openEdit(user: UserView): void {
    this.editingUser.set(user);
    this.editForm.reset({
      userID: user.userId,
      name: user.name,
      email: user.email,
      phone: user.phone ?? '',
      roleID: this.roleNameToId(user.roleName) ?? 0,
      status: user.status === 'Active'
    });
    this.showEdit.set(true);
  }

  closeEdit(): void {
    this.showEdit.set(false);
    this.editingUser.set(null);
  }

  submitEdit(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    const v = this.editForm.getRawValue();
    const payload: UserUpdateRequest = {
      userID: v.userID,
      name: v.name,
      email: v.email,
      phone: v.phone,
      roleID: Number(v.roleID),
      status: v.status
    };
    this.saving.set(true);
    this.api.update(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('User updated');
        this.closeEdit();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not update user'));
      }
    });
  }

  /* ───── Delete ───── */
  openDelete(user: UserView): void {
    this.deleting.set(user);
    this.showDelete.set(true);
  }

  closeDelete(): void {
    this.showDelete.set(false);
    this.deleting.set(null);
  }

  confirmDelete(): void {
    const user = this.deleting();
    if (!user) return;
    this.saving.set(true);
    this.api.softDelete(user.userId).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('User deactivated');
        this.closeDelete();
        this.refresh();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(this.errorText(err, 'Could not deactivate user'));
      }
    });
  }

  togglePwd(): void {
    this.showPwd.update((v) => !v);
  }

  initials(name: string): string {
    return name
      .split(' ')
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
