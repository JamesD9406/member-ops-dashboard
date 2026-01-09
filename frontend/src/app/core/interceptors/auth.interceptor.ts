import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

// Purpose: Intercept HTTP requests/responses - modify them before they're sent or after they're received
// When they run:
// authInterceptor: Before every HTTP request is sent
// errorInterceptor: After every HTTP response is received
// Examples:
// authInterceptor: Automatically adds JWT token to every API request
// errorInterceptor: Catches 401 errors and redirects to login

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  // Clone request and add authorization header if token exists
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req);
};
