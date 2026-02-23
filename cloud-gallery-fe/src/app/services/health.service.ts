import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environment';

@Injectable({
  providedIn: 'root'
})
export class HealthService {
  constructor(private http: HttpClient) {}

  getHealth(): Observable<string> {
    return this.http.get(`${environment.apiUrl}/health`, { responseType: 'text' });
  }
}
