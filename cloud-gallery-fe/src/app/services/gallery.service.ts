import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Gallery } from '../models/gallery.model';
import { GalleryShare } from '../models/gallery-share.model';
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

  create(dto: { name: string; isPublic: boolean; coverUrl?: string }): Observable<Gallery> {
    return this.http.post<Gallery>(this.apiUrl, dto);
  }

  update(id: number, dto: { name: string; isPublic: boolean }): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getShares(galleryId: number): Observable<GalleryShare[]> {
    return this.http.get<GalleryShare[]>(`${this.apiUrl}/${galleryId}/shares`);
  }

  addShare(galleryId: number, userEmail: string, canEdit: boolean): Observable<GalleryShare> {
    return this.http.post<GalleryShare>(`${this.apiUrl}/${galleryId}/shares`, { userEmail, canEdit });
  }

  removeShare(galleryId: number, shareId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${galleryId}/shares/${shareId}`);
  }
}