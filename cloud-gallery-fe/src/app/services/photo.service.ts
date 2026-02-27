import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Photo } from '../models/photo.model';
import { PhotoUploadRequest } from '../models/photo-upload-request.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PhotoService {
  private apiUrl = environment.apiUrl + '/api/photos';

  constructor(private http: HttpClient) {}

  getAll(galleryId: number): Observable<Photo[]> {
    return this.http.get<Photo[]>(`${this.apiUrl}?galleryId=${galleryId}`);
  }

  getImage(galleryId: number, photoId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${photoId}/view?galleryId=${galleryId}`, { responseType: 'blob' });
  }

  upload(galleryId: number, request: PhotoUploadRequest): Observable<any> {
    const formData = new FormData();
    formData.append('File', request.file);
    formData.append('Name', request.name);
    return this.http.post(`${this.apiUrl}?galleryId=${galleryId}`, formData);
  }

  delete(galleryId: number, photoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${photoId}?galleryId=${galleryId}`);
  }
}