import { Routes } from '@angular/router';
import { Login } from './login/login';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';
import { EmployeeLetter } from './employee/employee-letter/employee-letter';
import {MyRequests} from "./employee/my-request/my-request";

export const routes: Routes = [
  {
    path: 'login',
    component: Login
  },
  {
    path: 'employee',
    canActivate: [authGuard, roleGuard('Employee')],
    loadComponent: () =>
      import('./employee/employee')
        .then(m => m.Employee)
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },
  {
  path: 'employee/employee-letter',
  component: EmployeeLetter
},
{
  path: 'employee/my-requests',
  component: MyRequests
},
  {
    path: '**',
    redirectTo: 'login'
  }
];