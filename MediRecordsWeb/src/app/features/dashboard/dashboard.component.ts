import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../core/models/auth.models';

interface DashboardCard {
  icon: string;
  title: string;
  description: string;
  color: string;
  route?: string;
}

const DASHBOARDS: Record<UserRole, DashboardCard[]> = {
  Admin: [
    { icon: 'bi-people', title: 'Manage users', description: 'Create, update, and deactivate accounts.', color: 'primary', route: '/admin/users' },
    { icon: 'bi-receipt', title: 'Procedure codes', description: 'Maintain billing procedure catalog.', color: 'success', route: '/admin/procedure-codes' },
    { icon: 'bi-cash-stack', title: 'Billing & exports', description: 'Mark charges billed and export reports.', color: 'warning', route: '/admin/billing' },
    { icon: 'bi-calendar-check', title: 'Appointments', description: 'Manage clinic-wide appointments.', color: 'info', route: '/appointments' },
    { icon: 'bi-folder2-open', title: 'Documents', description: 'Upload patient documents.', color: 'success', route: '/documents' }
  ],
  Physician: [
    { icon: 'bi-clipboard2-pulse', title: 'Encounter workspace', description: 'Today\'s encounters and chart status.', color: 'primary', route: '/workspace' },
    { icon: 'bi-clipboard-heart', title: 'Care plans', description: 'Create and review long-term patient care plans.', color: 'info', route: '/care-plans' },
    { icon: 'bi-heart-pulse', title: 'Record allergies', description: 'Document drug, food and environmental allergies.', color: 'danger', route: '/allergies' },
    { icon: 'bi-calendar2-event', title: 'Follow-ups', description: 'Schedule patient return visits.', color: 'warning', route: '/follow-ups' },
    { icon: 'bi-radioactive', title: 'Imaging orders', description: 'Order and review imaging studies.', color: 'info', route: '/imaging-orders' },
    { icon: 'bi-calendar-check', title: 'Appointments', description: 'View and update your appointments.', color: 'warning', route: '/appointments' },
    { icon: 'bi-cash-coin', title: 'Visit charges', description: 'Assign procedure-code charges to encounters.', color: 'success', route: '/charges' },
    { icon: 'bi-folder2-open', title: 'Documents', description: 'Upload referrals, consents, reports and photos.', color: 'info', route: '/documents' }
  ],
  Nurse: [
    { icon: 'bi-activity', title: 'Capture vitals', description: 'Record BP, HR, temperature and SpO2.', color: 'danger' },
    { icon: 'bi-pencil-square', title: 'Nursing notes', description: 'Add and update nursing notes.', color: 'primary' },
    { icon: 'bi-clipboard-heart', title: 'Care plans', description: 'Review patient care plans and goals.', color: 'success', route: '/care-plans' },
    { icon: 'bi-calendar-check', title: 'Appointments', description: 'Check today\'s schedule.', color: 'info', route: '/appointments' },
    { icon: 'bi-folder2-open', title: 'Documents', description: 'Upload patient documents.', color: 'warning', route: '/documents' }
  ],
  LabTech: [
    { icon: 'bi-droplet-half', title: 'Lab results', description: 'Enter results for completed lab orders.', color: 'primary' },
    { icon: 'bi-file-earmark-medical', title: 'Imaging reports', description: 'Submit findings and impressions for imaging orders.', color: 'warning', route: '/imaging-reports' },
    { icon: 'bi-check2-circle', title: 'Order status', description: 'Update lab order status.', color: 'success' }
  ],
  FrontDesk: [
    { icon: 'bi-person-plus', title: 'Register patient', description: 'Onboard new patients and update details.', color: 'primary' },
    { icon: 'bi-calendar-check', title: 'Appointments', description: 'Book, check-in, and update appointments.', color: 'success', route: '/appointments' },
    { icon: 'bi-folder2-open', title: 'Documents', description: 'Upload patient documents.', color: 'warning', route: '/documents' }
  ]
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  @Input({ required: true }) role!: UserRole;

  protected readonly auth = inject(AuthService);

  get cards(): DashboardCard[] {
    return DASHBOARDS[this.role] ?? [];
  }
}
