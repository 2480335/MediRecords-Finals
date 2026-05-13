import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    // Public — anonymous users can reset a forgotten password.
    // Authenticated users hitting this route work too (the component
    // adapts and locks the email field to the logged-in user).
    path: 'update-password',
    loadComponent: () =>
      import('./features/auth/update-password/update-password.component').then(
        (m) => m.UpdatePasswordComponent
      )
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/admin/admin-layout/admin-layout.component').then(
        (m) => m.AdminLayoutComponent
      ),
    children: [
      // ─── Admin pages ────────────────────────────────────
      {
        path: 'admin',
        pathMatch: 'full',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('./features/admin/overview/overview.component').then(
            (m) => m.OverviewComponent
          )
      },
      {
        path: 'admin/users',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('./features/admin/users/users.component').then((m) => m.UsersComponent)
      },
      {
        path: 'admin/procedure-codes',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('./features/admin/procedure-codes/procedure-codes.component').then(
            (m) => m.ProcedureCodesComponent
          )
      },
      {
        path: 'admin/billing',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('./features/admin/billing/billing.component').then(
            (m) => m.BillingComponent
          )
      },
      {
        path: 'admin/reports',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('./features/admin/reports/reports.component').then(
            (m) => m.ReportsComponent
          )
      },

      // ─── Per-role landing dashboards ────────────────────
      {
        path: 'physician',
        canActivate: [roleGuard(['Physician'])],
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent
          ),
        data: { role: 'Physician' }
      },
      {
        path: 'nurse',
        canActivate: [roleGuard(['Nurse'])],
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent
          ),
        data: { role: 'Nurse' }
      },
      {
        path: 'labtech',
        canActivate: [roleGuard(['LabTech'])],
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent
          ),
        data: { role: 'LabTech' }
      },
      {
        path: 'frontdesk',
        canActivate: [roleGuard(['FrontDesk'])],
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent
          ),
        data: { role: 'FrontDesk' }
      },

      // ─── Shared (cross-role) pages ──────────────────────
      {
        path: 'appointments',
        canActivate: [roleGuard(['Admin', 'FrontDesk', 'Physician', 'Nurse'])],
        loadComponent: () =>
          import('./features/appointments/appointments.component').then(
            (m) => m.AppointmentsComponent
          )
      },
      {
        path: 'allergies',
        canActivate: [roleGuard(['Physician'])],
        loadComponent: () =>
          import('./features/allergies/allergies.component').then(
            (m) => m.AllergiesComponent
          )
      },
      {
        path: 'charges',
        canActivate: [roleGuard(['Admin', 'Physician'])],
        loadComponent: () =>
          import('./features/charges/charges.component').then(
            (m) => m.ChargesComponent
          )
      },
      {
        path: 'care-plans',
        canActivate: [roleGuard(['Physician', 'Nurse'])],
        loadComponent: () =>
          import('./features/care-plans/care-plans.component').then(
            (m) => m.CarePlansComponent
          )
      },
      {
        path: 'documents',
        canActivate: [roleGuard(['Admin', 'Physician', 'Nurse', 'FrontDesk'])],
        loadComponent: () =>
          import('./features/documents/documents.component').then(
            (m) => m.DocumentsComponent
          )
      },
      {
        path: 'workspace',
        canActivate: [roleGuard(['Physician'])],
        loadComponent: () =>
          import('./features/workspace/workspace.component').then(
            (m) => m.WorkspaceComponent
          )
      },
      {
        path: 'follow-ups',
        canActivate: [roleGuard(['Physician'])],
        loadComponent: () =>
          import('./features/follow-ups/follow-ups.component').then(
            (m) => m.FollowUpsComponent
          )
      },
      {
        path: 'imaging-orders',
        canActivate: [roleGuard(['Physician'])],
        loadComponent: () =>
          import('./features/imaging-orders/imaging-orders.component').then(
            (m) => m.ImagingOrdersComponent
          )
      },
      {
        path: 'imaging-reports',
        canActivate: [roleGuard(['LabTech'])],
        loadComponent: () =>
          import('./features/imaging-reports/imaging-reports.component').then(
            (m) => m.ImagingReportsComponent
          )
      },
      {
        path: 'immunizations',
        canActivate: [roleGuard(['Physician', 'Nurse'])],
        loadComponent: () =>
          import('./features/immunizations/immunizations.component').then(
            (m) => m.ImmunizationsComponent
          )
      },
      {
        path: 'lab-orders',
        canActivate: [roleGuard(['Admin', 'Physician', 'Nurse', 'LabTech'])],
        loadComponent: () =>
          import('./features/lab-orders/lab-orders.component').then(
            (m) => m.LabOrdersComponent
          )
      },
      {
        path: 'lab-results',
        canActivate: [roleGuard(['Admin', 'Physician', 'Nurse', 'LabTech'])],
        loadComponent: () =>
          import('./features/lab-results/lab-results.component').then(
            (m) => m.LabResultsComponent
          )
      },
      {
        path: 'medical-history',
        canActivate: [roleGuard(['Physician'])],
        loadComponent: () =>
          import('./features/medical-history/medical-history.component').then(
            (m) => m.MedicalHistoryComponent
          )
      },
      {
        path: 'medications',
        canActivate: [roleGuard(['Admin', 'Physician', 'Nurse', 'LabTech', 'FrontDesk'])],
        loadComponent: () =>
          import('./features/medications/medications.component').then(
            (m) => m.MedicationsComponent
          )
      },
      {
        path: 'nursing-notes',
        canActivate: [roleGuard(['Admin', 'Physician', 'Nurse'])],
        loadComponent: () =>
          import('./features/nursing-notes/nursing-notes.component').then(
            (m) => m.NursingNotesComponent
          )
      },
      {
        path: 'patients',
        canActivate: [roleGuard(['FrontDesk', 'Physician', 'Nurse'])],
        loadComponent: () =>
          import('./features/patients/patients.component').then(
            (m) => m.PatientsComponent
          )
      },
      {
        path: 'prescriptions',
        canActivate: [roleGuard(['Admin', 'Physician', 'FrontDesk', 'Nurse'])],
        loadComponent: () =>
          import('./features/prescriptions/prescriptions.component').then(
            (m) => m.PrescriptionsComponent
          )
      },
      {
        path: 'problems',
        canActivate: [roleGuard(['Physician'])],
        loadComponent: () =>
          import('./features/problems/problems.component').then(
            (m) => m.ProblemsComponent
          )
      },
      {
        path: 'soap-notes',
        canActivate: [roleGuard(['Physician'])],
        loadComponent: () =>
          import('./features/soap-notes/soap-notes.component').then(
            (m) => m.SOAPNotesComponent
          )
      },
      {
        path: 'vitals',
        canActivate: [roleGuard(['Admin', 'Physician', 'Nurse'])],
        loadComponent: () =>
          import('./features/vitals/vitals.component').then(
            (m) => m.VitalsComponent
          )
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
