import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { LoginRequest, LoginResponse, User } from '../models/auth.models';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: jasmine.SpyObj<Router>; //mocks the Angular router

  const mockLoginResponse: LoginResponse = {
    token: 'mock-jwt-token',
    username: 'testuser',
    displayName: 'Test User',
    role: 'Staff',
    expiresAt: '2026-01-21T00:00:00Z',
  };

  const mockUser: User = {
    username: 'testuser',
    displayName: 'Test User',
    role: 'Staff',
  };

  beforeEach(() => {
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService,
        { provide: Router, useValue: routerSpy },
      ],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);

    // Clear localStorage before each test
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  describe('login', () => {
    it('should call the login API endpoint with credentials', () => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };

      service.login(credentials).subscribe();

      const req = httpMock.expectOne('http://localhost:5293/api/Auth/login');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(credentials);
      req.flush(mockLoginResponse);
    });

    it('should store token in localStorage on successful login', () => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };

      service.login(credentials).subscribe();

      const req = httpMock.expectOne('http://localhost:5293/api/Auth/login');
      req.flush(mockLoginResponse);

      expect(localStorage.getItem('auth_token')).toBe('mock-jwt-token');
    });

    it('should store user info in localStorage on successful login', () => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };

      service.login(credentials).subscribe();

      const req = httpMock.expectOne('http://localhost:5293/api/Auth/login');
      req.flush(mockLoginResponse);

      const storedUser = JSON.parse(localStorage.getItem('current_user')!);
      expect(storedUser.username).toBe('testuser');
      expect(storedUser.displayName).toBe('Test User');
      expect(storedUser.role).toBe('Staff');
    });

    it('should update currentUser$ observable on successful login', (done) => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };

      service.login(credentials).subscribe(() => {
        service.currentUser$.subscribe((user) => {
          expect(user).toEqual(mockUser);
          done();
        });
      });

      const req = httpMock.expectOne('http://localhost:5293/api/Auth/login');
      req.flush(mockLoginResponse);
    });

    it('should return LoginResponse observable', (done) => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };

      service.login(credentials).subscribe((response) => {
        expect(response.token).toBe('mock-jwt-token');
        expect(response.username).toBe('testuser');
        done();
      });

      const req = httpMock.expectOne('http://localhost:5293/api/Auth/login');
      req.flush(mockLoginResponse);
    });
  });

  describe('logout', () => {
    beforeEach(() => {
      // Setup logged in state
      localStorage.setItem('auth_token', 'mock-jwt-token');
      localStorage.setItem('current_user', JSON.stringify(mockUser));
    });

    it('should remove token from localStorage', () => {
      service.logout();
      expect(localStorage.getItem('auth_token')).toBeNull();
    });

    it('should remove user from localStorage', () => {
      service.logout();
      expect(localStorage.getItem('current_user')).toBeNull();
    });

    it('should set currentUser$ to null', (done) => {
      service.logout();
      service.currentUser$.subscribe((user) => {
        expect(user).toBeNull();
        done();
      });
    });

    it('should navigate to /login', () => {
      service.logout();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('getToken', () => {
    it('should return token when it exists in localStorage', () => {
      localStorage.setItem('auth_token', 'mock-jwt-token');
      expect(service.getToken()).toBe('mock-jwt-token');
    });

    it('should return null when no token exists', () => {
      expect(service.getToken()).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when token exists', () => {
      localStorage.setItem('auth_token', 'mock-jwt-token');
      expect(service.isAuthenticated()).toBeTrue();
    });

    it('should return false when no token exists', () => {
      expect(service.isAuthenticated()).toBeFalse();
    });
  });

  describe('getCurrentUser', () => {
    it('should return current user from BehaviorSubject', () => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };
      service.login(credentials).subscribe();

      const req = httpMock.expectOne('http://localhost:5293/api/Auth/login');
      req.flush(mockLoginResponse);

      const user = service.getCurrentUser();
      expect(user).toEqual(mockUser);
    });

    it('should return null when no user is logged in', () => {
      expect(service.getCurrentUser()).toBeNull();
    });
  });

  describe('getRole', () => {
    it('should return user role when logged in', () => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };
      service.login(credentials).subscribe();

      const req = httpMock.expectOne('http://localhost:5293/api/Auth/login');
      req.flush(mockLoginResponse);

      expect(service.getRole()).toBe('Staff');
    });

    it('should return null when no user is logged in', () => {
      expect(service.getRole()).toBeNull();
    });
  });

  describe('hasRole', () => {
    beforeEach(() => {
      const credentials: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };
      service.login(credentials).subscribe();

      const req = httpMock.expectOne('http://localhost:5293/api/Auth/login');
      req.flush(mockLoginResponse);
    });

    it('should return true when user has one of the specified roles', () => {
      expect(service.hasRole(['Staff', 'Admin'])).toBeTrue();
    });

    it('should return false when user does not have any of the specified roles', () => {
      expect(service.hasRole(['Supervisor', 'Admin'])).toBeFalse();
    });

    it('should return false when no user is logged in', () => {
      service.logout();
      expect(service.hasRole(['Staff'])).toBeFalse();
    });

    it('should return true for exact role match', () => {
      expect(service.hasRole(['Staff'])).toBeTrue();
    });
  });
});
