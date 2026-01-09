import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Purpose: Control navigation - decide if a user can access a route
// When they run: Before navigating to a route
// Examples:
// authGuard: "Is the user logged in? If not, redirect to login"
// roleGuard: "Does the user have admin role? If not, deny access"

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Store the attempted URL for redirecting after login
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
