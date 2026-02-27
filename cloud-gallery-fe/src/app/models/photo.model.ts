import { Gallery } from './gallery.model';

export interface Photo {
  id: number;
  name: string;
  uploadedAt: string;
  imageData?: string;
  contentType?: string;
  galleryId?: number;
  gallery?: Gallery;
}