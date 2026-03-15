import { Component, OnInit, OnDestroy, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { GalleryService } from '../../services/gallery.service';
import { PhotoService } from '../../services/photo.service';
import { IdentityService } from '../../services/identity.service';
import { ToastService } from '../../services/toast.service';
import { Gallery } from '../../models/gallery.model';

@Component({
  selector: 'app-gallery',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './gallery.component.html',
  styleUrls: ['./gallery.component.css']
})
export class GalleryComponent implements OnInit, OnDestroy {
  galleryService = inject(GalleryService);
  photoService = inject(PhotoService);
  auth = inject(IdentityService);
  toast = inject(ToastService);
  router = inject(Router);

  galleries: Gallery[] = [];
  loading = false;
  filter: 'all' | 'public' | 'mine' | 'shared' = 'all';
  coverUrls = new Map<number, string>();

  createModalOpen = false;
  newGalleryName = '';
  newGalleryPublic = true;

  isAuthenticated = computed(() => this.auth.isAuthenticated());
  currentUserId = computed(() => this.auth.userInfo()?.email ?? null);

  ngOnInit() {
    if (!this.isAuthenticated()) this.filter = 'public';
    this.loadGalleries();
  }

  ngOnDestroy() {
    this.coverUrls.forEach(url => URL.revokeObjectURL(url));
    this.coverUrls.clear();
  }

  loadGalleries() {
    this.loading = true;
    const filters =
      this.filter === 'mine' ? { mine: true } :
      this.filter === 'shared' ? { sharedWithMe: true } :
      this.filter === 'all' ? {} :
      { isPublic: true };

    this.galleryService.getAll(filters).subscribe({
      next: (galleries) => {
        this.galleries = galleries;
        this.loading = false;
        this.loadCovers(galleries);
      },
      error: () => { this.loading = false; }
    });
  }

  private loadCovers(galleries: Gallery[]) {
    // Revoke previous blob URLs
    this.coverUrls.forEach(url => URL.revokeObjectURL(url));
    this.coverUrls.clear();

    for (const gallery of galleries) {
      this.photoService.getCover(gallery.id).subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          this.coverUrls.set(gallery.id, url);
        },
      });
    }
  }

  setFilter(f: 'all' | 'public' | 'mine' | 'shared') {
    this.filter = f;
    this.loadGalleries();
  }

  closeCreateModal() {
    this.createModalOpen = false;
    this.newGalleryName = '';
    this.newGalleryPublic = true;
  }

  createGallery() {
    if (!this.newGalleryName.trim()) return;
    this.galleryService.create({ name: this.newGalleryName, isPublic: this.newGalleryPublic }).subscribe({
      next: () => {
        this.toast.show('Galéria létrehozva!', 'success', 'Rendszer');
        this.closeCreateModal();
        this.loadGalleries();
      },
      error: () => this.toast.show('Hiba a galéria létrehozásakor!', 'danger', 'Hiba')
    });
  }

  openGallery(id: number) {
    this.router.navigate(['/galleries', id]);
  }
}