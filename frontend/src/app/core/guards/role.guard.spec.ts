import { TestBed } from '@angular/core/testing';
import {
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
} from '@angular/router';
import { roleGuard } from './role.guard';
import { AuthService } from '../services/auth.service';

describe('roleGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let mockRoute: ActivatedRouteSnapshot;
  let mockState: RouterStateSnapshot;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', [
      'isAuthenticated',
      'hasRole',
    ]);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy },
      ],
    });

    // Create mock route with roles data
    mockRoute = {
      data: { roles: ['Supervisor', 'Admin'] },
    } as unknown as ActivatedRouteSnapshot;

    mockState = { url: '/audit-log' } as RouterStateSnapshot;
  });

  it('should allow navigation when user is authenticated and has required role', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);
    authServiceSpy.hasRole.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() => {
      return roleGuard(mockRoute, mockState);
    });

    expect(result).toBeTrue();
    expect(authServiceSpy.hasRole).toHaveBeenCalledWith([
      'Supervisor',
      'Admin',
    ]);
    expect(routerSpy.navigate).not.toHaveBeenCalled();
  });

  it('should redirect to login when user is not authenticated', () => {
    authServiceSpy.isAuthenticated.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => {
      return roleGuard(mockRoute, mockState);
    });

    expect(result).toBeFalse();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
    expect(authServiceSpy.hasRole).not.toHaveBeenCalled();
  });

  it('should redirect to home when user is authenticated but lacks required role', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);
    authServiceSpy.hasRole.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => {
      return roleGuard(mockRoute, mockState);
    });

    expect(result).toBeFalse();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should check against the roles specified in route data', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);
    authServiceSpy.hasRole.and.returnValue(true);

    // Test with different roles
    mockRoute = {
      data: { roles: ['Admin'] },
    } as unknown as ActivatedRouteSnapshot;

    TestBed.runInInjectionContext(() => {
      return roleGuard(mockRoute, mockState);
    });

    expect(authServiceSpy.hasRole).toHaveBeenCalledWith(['Admin']);
  });

  it('should handle Staff-only routes', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);
    authServiceSpy.hasRole.and.returnValue(true);

    mockRoute = {
      data: { roles: ['Staff', 'Supervisor', 'Admin'] },
    } as unknown as ActivatedRouteSnapshot;

    const result = TestBed.runInInjectionContext(() => {
      return roleGuard(mockRoute, mockState);
    });

    expect(result).toBeTrue();
    expect(authServiceSpy.hasRole).toHaveBeenCalledWith([
      'Staff',
      'Supervisor',
      'Admin',
    ]);
  });

  it('should deny Staff user access to Supervisor-only route', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);
    authServiceSpy.hasRole.and.returnValue(false); // Staff doesn't have Supervisor/Admin role

    mockRoute = {
      data: { roles: ['Supervisor', 'Admin'] },
    } as unknown as ActivatedRouteSnapshot;

    const result = TestBed.runInInjectionContext(() => {
      return roleGuard(mockRoute, mockState);
    });

    expect(result).toBeFalse();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
  });
});
