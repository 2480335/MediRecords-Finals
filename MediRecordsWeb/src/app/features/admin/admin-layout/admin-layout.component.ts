import { CommonModule } from '@angular/common';
import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import {
  NavigationEnd,
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/auth.models';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  exact?: boolean;
  group: 'workspace' | 'operations';
  description?: string;
}

interface BrandInfo {
  tag: string;
}

interface PageMeta {
  title: string;
  subtitle: string;
  icon: string;
}

const NAV_BY_ROLE: Record<UserRole, NavItem[]> = {
  Admin: [
    { label: 'Overview', icon: 'bi-grid-1x2', route: '/admin', exact: true, group: 'workspace', description: 'A snapshot of your clinic' },
    { label: 'Users', icon: 'bi-people', route: '/admin/users', group: 'workspace', description: 'Manage teammates and access' },
    { label: 'Procedure codes', icon: 'bi-receipt', route: '/admin/procedure-codes', group: 'operations', description: 'Maintain billing catalog' },
    { label: 'Billing', icon: 'bi-cash-stack', route: '/admin/billing', group: 'operations', description: 'Charges, billing and exports' },
    { label: 'Reports', icon: 'bi-bar-chart-line', route: '/admin/reports', group: 'operations', description: 'Clinic KPIs, documentation, and utilization' },
    { label: 'Charges', icon: 'bi-cash-coin', route: '/charges', group: 'operations', description: 'Assign and edit visit charges per encounter' },
    { label: 'Appointments', icon: 'bi-calendar-check', route: '/appointments', group: 'operations', description: 'Clinic-wide appointment scheduling' },
    { label: 'Documents', icon: 'bi-folder2-open', route: '/documents', group: 'operations', description: 'Upload patient documents' }
  ],
  Physician: [
    { label: 'Dashboard', icon: 'bi-grid-1x2', route: '/physician', exact: true, group: 'workspace', description: 'Your clinical home' },
    { label: 'Workspace', icon: 'bi-clipboard2-pulse', route: '/workspace', group: 'workspace', description: 'Today\'s encounters and chart status' },
    { label: 'Patients', icon: 'bi-person-vcard', route: '/patients', group: 'workspace', description: 'Look up patient charts' },
    { label: 'SOAP notes', icon: 'bi-journal-medical', route: '/soap-notes', group: 'workspace', description: 'Document an encounter (S/O/A/P)' },
    { label: 'Vitals', icon: 'bi-activity', route: '/vitals', group: 'workspace', description: 'Capture vital signs' },
    { label: 'Care plans', icon: 'bi-clipboard-heart', route: '/care-plans', group: 'workspace', description: 'Create and review care plans' },
    { label: 'Problems', icon: 'bi-bookmark-plus', route: '/problems', group: 'workspace', description: 'Record patient problems' },
    { label: 'Allergies', icon: 'bi-heart-pulse', route: '/allergies', group: 'workspace', description: 'Record patient allergies' },
    { label: 'Medical history', icon: 'bi-journal-text', route: '/medical-history', group: 'workspace', description: 'Record chronic and past conditions' },
    { label: 'Immunizations', icon: 'bi-shield-plus', route: '/immunizations', group: 'workspace', description: 'Track vaccinations administered' },
    { label: 'Follow-ups', icon: 'bi-calendar2-event', route: '/follow-ups', group: 'workspace', description: 'Schedule patient return visits' },
    { label: 'Prescriptions', icon: 'bi-prescription2', route: '/prescriptions', group: 'operations', description: 'Issue and manage prescriptions' },
    { label: 'Lab orders', icon: 'bi-droplet-half', route: '/lab-orders', group: 'operations', description: 'Order and track lab tests' },
    { label: 'Lab results', icon: 'bi-clipboard2-data', route: '/lab-results', group: 'operations', description: 'Review test results' },
    { label: 'Imaging orders', icon: 'bi-radioactive', route: '/imaging-orders', group: 'operations', description: 'Order and review imaging studies' },
    { label: 'Medications', icon: 'bi-capsule', route: '/medications', group: 'operations', description: 'Browse the medication list' },
    { label: 'Nursing notes', icon: 'bi-pencil-square', route: '/nursing-notes', group: 'operations', description: 'Add or edit nursing notes' },
    { label: 'Appointments', icon: 'bi-calendar-check', route: '/appointments', group: 'operations', description: 'View and update appointments' },
    { label: 'Charges', icon: 'bi-cash-coin', route: '/charges', group: 'operations', description: 'Assign visit charges' },
    { label: 'Documents', icon: 'bi-folder2-open', route: '/documents', group: 'operations', description: 'Upload patient documents' }
  ],
  Nurse: [
    { label: 'Dashboard', icon: 'bi-grid-1x2', route: '/nurse', exact: true, group: 'workspace', description: 'Your nursing home' },
    { label: 'Patients', icon: 'bi-person-vcard', route: '/patients', group: 'workspace', description: 'Look up patient charts' },
    { label: 'Vitals', icon: 'bi-activity', route: '/vitals', group: 'workspace', description: 'Capture vital signs' },
    { label: 'Care plans', icon: 'bi-clipboard-heart', route: '/care-plans', group: 'workspace', description: 'Review patient care plans' },
    { label: 'Immunizations', icon: 'bi-shield-plus', route: '/immunizations', group: 'workspace', description: 'View patient immunization records' },
    { label: 'Nursing notes', icon: 'bi-pencil-square', route: '/nursing-notes', group: 'workspace', description: 'Add or edit nursing notes' },
    { label: 'Prescriptions', icon: 'bi-prescription2', route: '/prescriptions', group: 'operations', description: 'View patient prescriptions' },
    { label: 'Lab orders', icon: 'bi-droplet-half', route: '/lab-orders', group: 'operations', description: 'View lab orders' },
    { label: 'Lab results', icon: 'bi-clipboard2-data', route: '/lab-results', group: 'operations', description: 'Review test results' },
    { label: 'Medications', icon: 'bi-capsule', route: '/medications', group: 'operations', description: 'Browse the medication list' },
    { label: 'Appointments', icon: 'bi-calendar-check', route: '/appointments', group: 'operations', description: 'Today\'s schedule' },
    { label: 'Documents', icon: 'bi-folder2-open', route: '/documents', group: 'operations', description: 'Upload patient documents' }
  ],
  LabTech: [
    { label: 'Dashboard', icon: 'bi-grid-1x2', route: '/labtech', exact: true, group: 'workspace', description: 'Your lab home' },
    { label: 'Lab orders', icon: 'bi-droplet-half', route: '/lab-orders', group: 'operations', description: 'View and complete lab orders' },
    { label: 'Lab results', icon: 'bi-clipboard2-data', route: '/lab-results', group: 'operations', description: 'Record and review test results' },
    { label: 'Imaging reports', icon: 'bi-file-earmark-medical', route: '/imaging-reports', group: 'operations', description: 'Submit imaging study reports' }
  ],
  FrontDesk: [
    { label: 'Dashboard', icon: 'bi-grid-1x2', route: '/frontdesk', exact: true, group: 'workspace', description: 'Front desk home' },
    { label: 'Patients', icon: 'bi-person-vcard', route: '/patients', group: 'workspace', description: 'Register and update patients' },
    { label: 'Appointments', icon: 'bi-calendar-check', route: '/appointments', group: 'operations', description: 'Book, check-in and update' },
    { label: 'Prescriptions', icon: 'bi-prescription2', route: '/prescriptions', group: 'operations', description: 'View patient prescriptions' },
    { label: 'Documents', icon: 'bi-folder2-open', route: '/documents', group: 'operations', description: 'Upload patient documents' }
  ]
};

const BRAND_BY_ROLE: Record<UserRole, BrandInfo> = {
  Admin: { tag: 'Admin Console' },
  Physician: { tag: 'Clinical Workspace' },
  Nurse: { tag: 'Nursing Console' },
  LabTech: { tag: 'Lab Console' },
  FrontDesk: { tag: 'Front Desk' }
};

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss'
})
export class AdminLayoutComponent implements OnDestroy {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly sidebarOpen = signal(false);
  readonly currentMeta = signal<PageMeta>({
    title: 'Dashboard',
    subtitle: '',
    icon: 'bi-grid-1x2'
  });
  readonly now = signal(new Date());
  private clockId: ReturnType<typeof setInterval> | null = null;

  readonly nav = computed<NavItem[]>(() => {
    const role = this.auth.role();
    return role ? NAV_BY_ROLE[role] ?? [] : [];
  });

  readonly brandTag = computed(() => {
    const role = this.auth.role();
    return role ? BRAND_BY_ROLE[role].tag : 'Console';
  });

  readonly workspaceNav = computed(() =>
    this.nav().filter((n) => n.group === 'workspace')
  );
  readonly operationsNav = computed(() =>
    this.nav().filter((n) => n.group === 'operations')
  );

  readonly initials = computed(() => {
    const n = this.auth.user()?.name ?? this.auth.user()?.email ?? 'A';
    return n
      .split(/\s+|@/)
      .filter(Boolean)
      .slice(0, 2)
      .map((s) => s.charAt(0).toUpperCase())
      .join('');
  });

  constructor() {
    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => {
        this.updateMeta();
        this.sidebarOpen.set(false);
      });
    this.updateMeta();

    this.clockId = setInterval(() => this.now.set(new Date()), 30_000);
  }

  ngOnDestroy(): void {
    if (this.clockId) clearInterval(this.clockId);
  }

  toggleSidebar(): void {
    this.sidebarOpen.update((v) => !v);
  }

  closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  logout(): void {
    this.auth.logout();
  }

  private updateMeta(): void {
    const url = this.router.url.split('?')[0];
    const items = this.nav();
    const match = [...items]
      .sort((a, b) => b.route.length - a.route.length)
      .find((n) => (n.exact ? url === n.route : url === n.route || url.startsWith(n.route + '/')));
    if (match) {
      this.currentMeta.set({
        title: match.label,
        subtitle: match.description ?? '',
        icon: match.icon
      });
    } else if (url.startsWith('/update-password')) {
      this.currentMeta.set({
        title: 'Update password',
        subtitle: 'Choose a new password',
        icon: 'bi-key'
      });
    } else {
      this.currentMeta.set({
        title: 'Dashboard',
        subtitle: '',
        icon: 'bi-grid-1x2'
      });
    }
  }
}
