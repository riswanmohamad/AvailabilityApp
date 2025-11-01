import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, of } from 'rxjs';
import { 
  User, 
  LoginRequest, 
  RegisterRequest, 
  AuthResponse, 
  ApiResponse 
} from '../models/models';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl; // Update with your API URL
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private tokenSubject = new BehaviorSubject<string | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public token$ = this.tokenSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadStoredAuth();
  }

  private loadStoredAuth(): void {
    const token = localStorage.getItem('token');
    const user = localStorage.getItem('user');
    
    if (token && user) {
      this.tokenSubject.next(token);
      this.currentUserSubject.next(JSON.parse(user));
    }
  }

  login(credentials: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        // On successful structured response, persist auth
        tap(response => {
          if (response && response.success && response.data) {
            this.setAuth(response.data.token, response.data.user);
          }
        }),
        // Normalize HTTP errors (400/401/etc) into ApiResponse so callers receive a consistent object
        catchError((err) => {
          const apiResp = (err && err.error && typeof err.error === 'object')
            ? err.error as ApiResponse<AuthResponse>
            : { success: false, message: err?.message || 'Server error', data: undefined } as ApiResponse<AuthResponse>;
          // Do not call setAuth here since it's an error
          return of(apiResp);
        })
      );
  }

  register(userData: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/auth/register`, userData)
      .pipe(
        tap(response => {
          if (response && response.success && response.data) {
            this.setAuth(response.data.token, response.data.user);
          }
        }),
        catchError((err) => {
          const apiResp = (err && err.error && typeof err.error === 'object')
            ? err.error as ApiResponse<AuthResponse>
            : { success: false, message: err?.message || 'Server error', data: undefined } as ApiResponse<AuthResponse>;
          return of(apiResp);
        })
      );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.tokenSubject.next(null);
    this.currentUserSubject.next(null);
  }

  private setAuth(token: string, user: User): void {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
    this.tokenSubject.next(token);
    this.currentUserSubject.next(user);
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}