import { TestBed } from '@angular/core/testing';
import {
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
} from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let mockRoute: ActivatedRouteSnapshot;
  let mockState: RouterStateSnapshot;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy },
      ],
    });

    // Create mock route and state objects
    mockRoute = {} as ActivatedRouteSnapshot;
    mockState = { url: '/members' } as RouterStateSnapshot;
  });

  it('should allow navigation when user is authenticated', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() => {
      return authGuard(mockRoute, mockState);
    });

    expect(result).toBeTrue();
    expect(routerSpy.navigate).not.toHaveBeenCalled();
  });

  it('should redirect to login when user is not authenticated', () => {
    authServiceSpy.isAuthenticated.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => {
      return authGuard(mockRoute, mockState);
    });

    expect(result).toBeFalse();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: '/members' },
    });
  });

  it('should preserve the attempted URL in returnUrl query param', () => {
    authServiceSpy.isAuthenticated.and.returnValue(false);
    mockState = { url: '/members/123/details' } as RouterStateSnapshot;

    TestBed.runInInjectionContext(() => {
      return authGuard(mockRoute, mockState);
    }); // authGuard is not a class but a functional guard, and needs to be injected

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: '/members/123/details' },
    });
  });

  it('should handle root URL redirect', () => {
    authServiceSpy.isAuthenticated.and.returnValue(false);
    mockState = { url: '/' } as RouterStateSnapshot;

    TestBed.runInInjectionContext(() => {
      return authGuard(mockRoute, mockState);
    });

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: '/' },
    });
  });
});
