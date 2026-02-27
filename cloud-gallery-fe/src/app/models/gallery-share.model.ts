export interface GalleryShare {
  id: number;
  galleryId: number;
  canEdit: boolean;
  userEmail: string;
  gallery?: Gallery;
}

import { Gallery } from './gallery.model';
