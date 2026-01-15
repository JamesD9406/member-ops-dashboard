import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(
        (m) => m.LoginComponent,
      ),
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'members',
        pathMatch: 'full',
      },
      {
        path: 'members',
        loadComponent: () =>
          import('./features/members/member-search/member-search.component').then(
            (m) => m.MemberSearchComponent,
          ),
      },
      {
        path: 'members/:id',
        loadComponent: () =>
          import('./features/members/member-detail/member-detail.component').then(
            (m) => m.MemberDetailComponent,
          ),
      },
      {
        path: 'service-requests',
        loadComponent: () =>
          import('./features/service-requests/service-request-list/service-request-list.component').then(
            (m) => m.ServiceRequestListComponent,
          ),
      },
      {
        path: 'service-requests/:id',
        loadComponent: () =>
          import('./features/service-requests/service-request-detail/service-request-detail.component').then(
            (m) => m.ServiceRequestDetailComponent,
          ),
      },
      {
        path: 'audit-log',
        canActivate: [roleGuard],
        data: { roles: ['Supervisor', 'Admin'] },
        loadComponent: () =>
          import('./features/audit-log/audit-log.component').then(
            (m) => m.AuditLogComponent,
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'members',
  },
];
