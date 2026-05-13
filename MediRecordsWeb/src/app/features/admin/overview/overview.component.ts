import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { BillingService } from '../../../core/services/billing.service';
import { ProcedureCodeService } from '../../../core/services/procedure-code.service';
import { UserService } from '../../../core/services/user.service';
import { UserView } from '../../../core/models/user.models';
import { UnbilledEncounter } from '../../../core/models/billing.models';
import { ProcedureCodeView } from '../../../core/models/procedure-code.models';

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './overview.component.html',
  styleUrl: './overview.component.scss'
})
export class OverviewComponent implements OnInit {
  private readonly users = inject(UserService);
  private readonly codes = inject(ProcedureCodeService);
  private readonly billing = inject(BillingService);
  private readonly auth = inject(AuthService);

  /**
   * Clicking a Team breakdown role row signs the admin out and sends them
   * to the login page so they can sign in as that role.
   */
  signInAs(_role: string): void {
    this.auth.logout();
  }

  readonly loading = signal(true);
  readonly errored = signal(false);

  readonly allUsers = signal<UserView[]>([]);
  readonly procedureCodes = signal<ProcedureCodeView[]>([]);
  readonly unbilled = signal<UnbilledEncounter[]>([]);
  readonly unbilledTotal = signal(0);

  readonly totalUsers = computed(() => this.allUsers().length);
  readonly activeUsers = computed(
    () => this.allUsers().filter((u) => u.status === 'Active').length
  );
  readonly inactiveUsers = computed(() => this.totalUsers() - this.activeUsers());

  readonly roleBreakdown = computed(() => {
    const map = new Map<string, number>();
    for (const u of this.allUsers()) {
      map.set(u.roleName, (map.get(u.roleName) ?? 0) + 1);
    }
    return [...map.entries()]
      .map(([name, count]) => ({ name, count }))
      .sort((a, b) => b.count - a.count);
  });

  readonly unbilledRevenue = computed(() =>
    this.unbilled().reduce((sum, e) => sum + (e.totalChargeAmount ?? 0), 0)
  );

  readonly recentEncounters = computed(() => this.unbilled().slice(0, 5));

  ngOnInit(): void {
    forkJoin({
      users: this.users.getAll().pipe(catchError(() => of([] as UserView[]))),
      codes: this.codes.getAll().pipe(catchError(() => of([] as ProcedureCodeView[]))),
      unbilled: this.billing
        .getUnbilledEncounters({ page: 1, pageSize: 25 })
        .pipe(
          catchError(() =>
            of({
              data: [] as UnbilledEncounter[],
              totalCount: 0,
              page: 1,
              pageSize: 25,
              totalPages: 0,
              hasNext: false,
              hasPrevious: false
            })
          )
        )
    }).subscribe({
      next: ({ users, codes, unbilled }) => {
        this.allUsers.set(users ?? []);
        this.procedureCodes.set(codes ?? []);
        this.unbilled.set(unbilled.data ?? []);
        this.unbilledTotal.set(unbilled.totalCount ?? 0);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errored.set(true);
      }
    });
  }
}
