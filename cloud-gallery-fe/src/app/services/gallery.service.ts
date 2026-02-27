import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Gallery } from '../models/gallery.model';
import { environment } from '../../environments/environment.development';
import { buildQueryString } from '../utils/query-string-builder';

@Injectable({ providedIn: 'root' })
export class GalleryService {
  private apiUrl = environment.apiUrl + '/api/Galleries';

  constructor(private http: HttpClient) {}

  getAll(filters?: { isPublic?: boolean; mine?: boolean; sharedWithMe?: boolean }): Observable<Gallery[]> {
    const params = buildQueryString(filters ?? {});
    return this.http.get<Gallery[]>(this.apiUrl + params);
  }

  getById(id: number): Observable<Gallery> {
    return this.http.get<Gallery>(`${this.apiUrl}/${id}`);
  }

  create(gallery: Gallery): Observable<Gallery> {
    return this.http.post<Gallery>(this.apiUrl, gallery);
  }

  update(id: number, gallery: Gallery): Observable<Gallery> {
    return this.http.put<Gallery>(`${this.apiUrl}/${id}`, gallery);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}