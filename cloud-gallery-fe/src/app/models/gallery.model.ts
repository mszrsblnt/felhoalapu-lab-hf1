export interface Gallery {
  id: number;
  name: string;
  coverUrl: string;
  createdAt: string;
  isPublic: boolean;
  ownerId: string;
  isOwner?: boolean;
  sharedWithMe?: boolean;
  sharedCanEdit?: boolean;
  photos: Photo[];
  shares: GalleryShare[];
}

import { Photo } from './photo.model';
import { GalleryShare } from './gallery-share.model';
