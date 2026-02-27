import { Injectable, signal, effect } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse, InfoResponse, InfoRequest, TwoFactorRequest, TwoFactorResponse } from '../models/authresponse.model';

@Injectable({ providedIn: 'root' })
export class IdentityService {
  private apiUrl = environment.apiUrl;

  token = signal<string | null>(localStorage.getItem('token'));
  userInfo = signal<InfoResponse | null>(null);

  constructor(private http: HttpClient) {}

  login(data: { email: string; password: string; twoFactorCode?: string | null; twoFactorRecoveryCode?: string | null }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data).pipe(
      tap(res => this.handleToken(res))
    );
  }

  refresh(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken: localStorage.getItem('refreshToken') }).pipe(
      tap(res => this.handleToken(res))
    );
  }

  private handleToken(res: AuthResponse) {
    if (res?.accessToken) {
      localStorage.setItem('token', res.accessToken);
      localStorage.setItem('refreshToken', res.refreshToken);
      this.token.set(res.accessToken);
    }
  }

  getToken(): string | null { return this.token(); }
  
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    this.token.set(null);
    this.userInfo.set(null);
  }

  register(email: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, { email, password });
  }

  private _userInfoEffect = effect(() => {
    if (this.token()) {
      this.getManageInfo().subscribe({ next: info => this.userInfo.set(info), error: () => this.userInfo.set(null) });
    } else {
      this.userInfo.set(null);
    }
  });

  confirmEmail(userId: string, code: string, changedEmail?: string): Observable<any> {
    let params = new HttpParams().set('userId', userId).set('code', code);
    if (changedEmail) params = params.set('changedEmail', changedEmail);
    return this.http.get(`${this.apiUrl}/confirmEmail`, { params });
  }

  resendConfirmationEmail(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resendConfirmationEmail`, { email });
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgotPassword`, { email });
  }

  resetPassword(email: string, resetCode: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resetPassword`, { email, resetCode, newPassword });
  }

  manage2fa(data: TwoFactorRequest): Observable<TwoFactorResponse> {
    return this.http.post<TwoFactorResponse>(`${this.apiUrl}/manage/2fa`, data);
  }

  getManageInfo(): Observable<InfoResponse> {
    return this.http.get<InfoResponse>(`${this.apiUrl}/manage/info`);
  }

  updateManageInfo(data: InfoRequest): Observable<InfoResponse> {
    return this.http.post<InfoResponse>(`${this.apiUrl}/manage/info`, data);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}