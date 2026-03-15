import { Component, OnInit, OnDestroy, inject, computed, ViewChild, ElementRef, HostListener } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { PhotoService } from '../../services/photo.service';
import { GalleryService } from '../../services/gallery.service';
import { IdentityService } from '../../services/identity.service';
import { ToastService } from '../../services/toast.service';
import { ConfirmService } from '../../services/confirm.service';
import { UploadQueueService } from '../../services/upload-queue.service';
import { Photo } from '../../models/photo.model';
import { Gallery } from '../../models/gallery.model';
import { GalleryShare } from '../../models/gallery-share.model';

const SORT_BY_KEY = 'gallery_sort_by';
const SORT_DESC_KEY = 'gallery_sort_desc';

@Component({
  selector: 'app-gallery-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DatePipe],
  templateUrl: './gallery-detail.component.html',
  styleUrls: ['./gallery-detail.component.css']
})
export class GalleryDetailComponent implements OnInit, OnDestroy {
  route = inject(ActivatedRoute);
  router = inject(Router);
  photoService = inject(PhotoService);
  galleryService = inject(GalleryService);
  auth = inject(IdentityService);
  toast = inject(ToastService);
  confirmService = inject(ConfirmService);
  uploadQueue = inject(UploadQueueService);

  @ViewChild('fileInput') fileInputRef!: ElementRef<HTMLInputElement>;

  galleryId!: number;
  gallery: Gallery | null = null;
  photos: Photo[] = [];
  loading = false;

  sortBy: 'name' | 'date' = (localStorage.getItem(SORT_BY_KEY) as 'name' | 'date') ?? 'date';
  sortDesc: boolean = localStorage.getItem(SORT_DESC_KEY) !== 'false';

  // Upload modal
  uploadModalOpen = false;
  uploadName = '';
  uploadFile: File | null = null;
  uploadPreviewUrl: string | null = null;

  // Drag-and-drop
  isDragging = false;
  private dragCounter = 0;
  private batchSub?: Subscription;

  // Lightbox viewer
  viewerOpen = false;
  viewerUrl: string | null = null;
  viewerPhotoName = '';
  viewerLoading = false;
  private viewerBlobUrl: string | null = null;

  // Edit modal
  editModalOpen = false;
  editName = '';
  editPublic = false;
  saving = false;

  // Share modal
  shareModalOpen = false;
  shares: GalleryShare[] = [];
  sharesLoading = false;
  shareEmail = '';
  shareCanEdit = false;
  addingShare = false;

  isAuthenticated = computed(() => this.auth.isAuthenticated());
  get isOwner(): boolean { return this.gallery?.isOwner ?? false; }
  get sharedWithMe(): boolean {
    if (this.isOwner || !this.isAuthenticated()) return false;
    const email = this.auth.userInfo()?.email;
    return this.gallery?.shares?.some(s => s.userEmail === email) ?? false;
  }
  get myShareCanEdit(): boolean {
    if (this.isOwner || !this.isAuthenticated()) return false;
    const email = this.auth.userInfo()?.email;
    return this.gallery?.shares?.some(s => s.userEmail === email && s.canEdit) ?? false;
  }
  get canDelete(): boolean {
    if (!this.isAuthenticated()) return false;
    if (this.isOwner) return true;
    const email = this.auth.userInfo()?.email;
    return this.gallery?.shares?.some(s => s.userEmail === email && s.canEdit) ?? false;
  }
  get canUpload(): boolean { return this.canDelete; }

  ngOnInit() {
    this.galleryId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadGallery();
    this.loadPhotos();
    this.batchSub = this.uploadQueue.batchComplete$.subscribe(gid => {
      if (gid === this.galleryId) this.loadPhotos();
    });
  }

  ngOnDestroy() {
    this.batchSub?.unsubscribe();
    this.revokeViewerBlob();
    this.revokePreviewBlob();
  }

  loadGallery() {
    this.galleryService.getById(this.galleryId).subscribe({
      next: (g) => {
        this.gallery = g;
        if (g.isOwner) this.loadShares();
      },
      error: () => {}
    });
  }

  loadPhotos() {
    this.loading = true;
    this.photoService.getAll(this.galleryId, this.sortBy, this.sortDesc).subscribe({
      next: (photos) => { this.photos = photos; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  toggleSort(by: 'name' | 'date') {
    if (this.sortBy === by) {
      this.sortDesc = !this.sortDesc;
    } else {
      this.sortBy = by;
      this.sortDesc = by === 'date';
    }
    localStorage.setItem(SORT_BY_KEY, this.sortBy);
    localStorage.setItem(SORT_DESC_KEY, String(this.sortDesc));
    this.loadPhotos();
  }

  // ── Drag-and-drop ─────────────────────────────────────────────────────────

  @HostListener('document:dragenter', ['$event'])
  onDocDragEnter(e: DragEvent) {
    if (!this.canUpload) return;
    e.preventDefault();
    this.dragCounter++;
    this.isDragging = true;
  }

  @HostListener('document:dragover', ['$event'])
  onDocDragOver(e: DragEvent) {
    if (!this.canUpload) return;
    e.preventDefault();
  }

  @HostListener('document:dragleave', ['$event'])
  onDocDragLeave(e: DragEvent) {
    this.dragCounter--;
    if (this.dragCounter <= 0) {
      this.dragCounter = 0;
      this.isDragging = false;
    }
  }

  @HostListener('document:drop', ['$event'])
  onDocDrop(e: DragEvent) {
    e.preventDefault();
    this.isDragging = false;
    this.dragCounter = 0;
    if (!this.canUpload) return;
    const files = Array.from(e.dataTransfer?.files ?? []).filter(f => f.type.startsWith('image/'));
    if (files.length === 0) return;
    if (files.length > 1) {
      this.uploadQueue.enqueue(this.galleryId, files.map(f => ({
        file: f,
        name: f.name.replace(/\.[^.]+$/, '').substring(0, 40)
      })));
      return;
    }
    this.openUploadModal(files[0]);
  }

  // ── Upload modal ──────────────────────────────────────────────────────────

  openUploadModal(file?: File) {
    this.uploadModalOpen = true;
    if (file) {
      this.uploadFile = file;
      this.revokePreviewBlob();
      this.uploadPreviewUrl = URL.createObjectURL(file);
      if (!this.uploadName) {
        this.uploadName = file.name.replace(/\.[^.]+$/, '').substring(0, 40);
      }
    }
  }

  closeUploadModal() {
    this.uploadModalOpen = false;
    this.resetUploadForm();
  }

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []).filter(f => f.type.startsWith('image/'));
    if (files.length === 0) return;
    if (files.length > 1) {
      this.uploadQueue.enqueue(this.galleryId, files.map(f => ({
        file: f,
        name: f.name.replace(/\.[^.]+$/, '').substring(0, 40)
      })));
      this.closeUploadModal();
      return;
    }
    const file = files[0];
    this.uploadFile = file;
    this.revokePreviewBlob();
    this.uploadPreviewUrl = URL.createObjectURL(file);
    if (!this.uploadName) {
      this.uploadName = file.name.replace(/\.[^.]+$/, '').substring(0, 40);
    }
  }

  upload() {
    if (!this.uploadFile || !this.uploadName.trim()) return;
    this.uploadQueue.enqueue(this.galleryId, [{ file: this.uploadFile, name: this.uploadName.trim() }]);
    this.closeUploadModal();
  }

  resetUploadForm() {
    this.uploadFile = null;
    this.uploadName = '';
    this.revokePreviewBlob();
    if (this.fileInputRef?.nativeElement) {
      this.fileInputRef.nativeElement.value = '';
    }
  }

  async deletePhoto(photo: Photo, event?: Event) {
    event?.stopPropagation();
    const ok = await this.confirmService.open(`Biztosan törlöd: "${photo.name}"?`, 'Törlés');
    if (!ok) return;
    this.photoService.delete(this.galleryId, photo.id).subscribe({
      next: () => {
        this.toast.show('Kép törölve!', 'success', 'Rendszer');
        this.photos = this.photos.filter(p => p.id !== photo.id);
        if (this.viewerOpen && this.viewerPhotoName === photo.name) {
          this.closeViewer();
        }
      },
      error: () => this.toast.show('Hiba a törlés során!', 'danger', 'Hiba')
    });
  }

  viewPhoto(photo: Photo) {
    this.viewerPhotoName = photo.name;
    this.viewerUrl = null;
    this.viewerOpen = true;
    this.viewerLoading = true;
    this.revokeViewerBlob();
    this.photoService.getImage(this.galleryId, photo.id).subscribe({
      next: (blob) => {
        this.viewerBlobUrl = URL.createObjectURL(blob);
        this.viewerUrl = this.viewerBlobUrl;
        this.viewerLoading = false;
      },
      error: () => {
        this.viewerLoading = false;
        this.toast.show('Nem sikerült betölteni a képet!', 'danger', 'Hiba');
      }
    });
  }

  closeViewer() {
    this.viewerOpen = false;
    this.viewerLoading = false;
    this.revokeViewerBlob();
    this.viewerUrl = null;
  }

  private revokeViewerBlob() {
    if (this.viewerBlobUrl) {
      URL.revokeObjectURL(this.viewerBlobUrl);
      this.viewerBlobUrl = null;
    }
  }

  private revokePreviewBlob() {
    if (this.uploadPreviewUrl) {
      URL.revokeObjectURL(this.uploadPreviewUrl);
      this.uploadPreviewUrl = null;
    }
  }

  // ── Edit ─────────────────────────────────────────────────────────────────

  openEdit() {
    if (!this.gallery) return;
    this.editName = this.gallery.name;
    this.editPublic = this.gallery.isPublic;
    this.editModalOpen = true;
  }

  cancelEdit() {
    this.editModalOpen = false;
  }

  saveEdit() {
    if (!this.gallery || !this.editName.trim()) return;
    this.saving = true;
    this.galleryService.update(this.gallery.id, { name: this.editName.trim(), isPublic: this.editPublic }).subscribe({
      next: () => {
        this.gallery = { ...this.gallery!, name: this.editName.trim(), isPublic: this.editPublic };
        this.editModalOpen = false;
        this.saving = false;
        this.toast.show('Galéria frissítve!', 'success', 'Rendszer');
      },
      error: () => {
        this.saving = false;
        this.toast.show('Hiba a mentés során!', 'danger', 'Hiba');
      }
    });
  }

  // ── Shares ───────────────────────────────────────────────────────────────

  openShares() {
    this.shareModalOpen = true;
    this.loadShares();
  }

  closeShares() {
    this.shareModalOpen = false;
  }

  loadShares() {
    this.sharesLoading = true;
    this.galleryService.getShares(this.galleryId).subscribe({
      next: (s) => { this.shares = s; this.sharesLoading = false; },
      error: () => { this.sharesLoading = false; }
    });
  }

  addShare() {
    if (!this.shareEmail.trim()) return;
    this.addingShare = true;
    this.galleryService.addShare(this.galleryId, this.shareEmail.trim(), this.shareCanEdit).subscribe({
      next: (s) => {
        const idx = this.shares.findIndex(x => x.id === s.id);
        if (idx >= 0) this.shares[idx] = s; else this.shares = [...this.shares, s];
        this.shareEmail = '';
        this.shareCanEdit = false;
        this.addingShare = false;
        this.toast.show('Megosztva!', 'success', 'Rendszer');
      },
      error: () => {
        this.addingShare = false;
        this.toast.show('Hiba a megosztás során!', 'danger', 'Hiba');
      }
    });
  }

  removeShare(share: GalleryShare) {
    this.galleryService.removeShare(this.galleryId, share.id).subscribe({
      next: () => {
        this.shares = this.shares.filter(s => s.id !== share.id);
        this.toast.show('Megosztás visszavonva!', 'success', 'Rendszer');
      },
      error: () => this.toast.show('Hiba a visszavonás során!', 'danger', 'Hiba')
    });
  }

  async deleteGallery() {
    if (!this.gallery) return;
    const ok = await this.confirmService.open(`Biztosan törlöd a "${this.gallery.name}" albumot? Ez a művelet nem vonható vissza.`, 'Törlés');
    if (!ok) return;
    this.galleryService.delete(this.galleryId).subscribe({
      next: () => {
        this.toast.show('Album törölve!', 'success', 'Rendszer');
        this.router.navigate(['/galleries']);
      },
      error: () => this.toast.show('Hiba a törlés során!', 'danger', 'Hiba')
    });
  }
}
