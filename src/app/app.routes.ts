import { Routes } from '@angular/router';

import { Login } from './login/login';

import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';

import { Employee } from './employee/employee';
import { EmployeeLetter } from './employee/employee-letter/employee-letter';
import { MyRequests } from './employee/my-request/my-request';
import { AdminDashboard } from './admin/dashboard/dashboard';

import { RequestManagement } from './admin/request-management/request-management';

export const routes: Routes = [

  // Login
  {
    path: 'login',
    component: Login
  },

  // Employee Dashboard
  {
    path: 'employee',
    component: Employee,
    canActivate: [authGuard, roleGuard('Employee')]
  },

  // Employee Letter Request
  {
    path: 'employee/employee-letter',
    component: EmployeeLetter,
    canActivate: [authGuard, roleGuard('Employee')]
  },

  // Employee My Requests
  {
    path: 'employee/my-requests',
    component: MyRequests,
    canActivate: [authGuard, roleGuard('Employee')]
  },

  // Admin routes will be added here
 {
  path: 'admin',
  component: AdminDashboard,
  canActivate: [authGuard, roleGuard('Admin')]
},
{
  path: 'admin/requests',
  component: RequestManagement,
  canActivate: [authGuard, roleGuard('Admin')]
},
  // Default route
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },

  // Unknown routes
  {
    path: '**',
    redirectTo: 'login'
  }
];