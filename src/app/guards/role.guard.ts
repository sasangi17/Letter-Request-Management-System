import { inject } from '@angular/core';
import {
  CanActivateFn,
  Router
} from '@angular/router';

import { AuthService } from '../services/auth.service';

export const roleGuard = (
  requiredRole: string
): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isLoggedIn()) {
      return router.createUrlTree(['/login']);
    }

    if (auth.hasRole(requiredRole)) {
      return true;
    }

    // Redirect authenticated users to their own area.
    if (auth.hasRole('Admin')) {
      return router.createUrlTree(['/admin']);
    }

    if (auth.hasRole('Employee')) {
      return router.createUrlTree(['/employee']);
    }

    return router.createUrlTree(['/login']);
  };
};