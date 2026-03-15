import { Injectable, inject, signal, computed } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { PhotoService } from './photo.service';
import { ToastService } from './toast.service';

export interface UploadJob {
  id: string;
  galleryId: number;
  file: File;
  name: string;
  status: 'pending' | 'uploading' | 'done' | 'error';
}

@Injectable({ providedIn: 'root' })
export class UploadQueueService {
  private photoService = inject(PhotoService);
  private toastService = inject(ToastService);

  private _queue = signal<UploadJob[]>([]);
  private _processing = false;
  private _clearTimer?: ReturnType<typeof setTimeout>;

  private _batchComplete$ = new Subject<number>();
  /** Emits the galleryId when a batch finishes (all pending processed). */
  readonly batchComplete$ = this._batchComplete$.asObservable();

  readonly queue = this._queue.asReadonly();
  readonly total     = computed(() => this._queue().length);
  readonly completed = computed(() => this._queue().filter(j => j.status === 'done' || j.status === 'error').length);
  readonly isActive  = computed(() => this._queue().some(j => j.status === 'pending' || j.status === 'uploading'));
  readonly progress  = computed(() => {
    const t = this.total();
    return t === 0 ? 0 : Math.round((this.completed() / t) * 100);
  });

  enqueue(galleryId: number, files: { file: File; name: string }[]) {
    if (this._clearTimer) {
      clearTimeout(this._clearTimer);
      this._clearTimer = undefined;
    }

    const jobs: UploadJob[] = files.map(({ file, name }) => ({
      id: crypto.randomUUID(),
      galleryId,
      file,
      name,
      status: 'pending'
    }));

    this._queue.update(q => [...q, ...jobs]);

    if (!this._processing) {
      this.processNext();
    }
  }

  private processNext() {
    const pending = this._queue().find(j => j.status === 'pending');

    if (!pending) {
      this._processing = false;
      const queue = this._queue();
      const galleryId = queue[0]?.galleryId;
      const errors = queue.filter(j => j.status === 'error').length;
      const done   = queue.filter(j => j.status === 'done').length;
      if (errors === 0) {
        this.toastService.show(
          done === 1 ? 'A kép sikeresen feltöltve!' : `${done} kép sikeresen feltöltve!`,
          'success', 'Feltöltés'
        );
      } else {
        this.toastService.show(
          `${done} kép feltöltve, ${errors} sikertelen.`,
          'warning', 'Feltöltés'
        );
      }
      if (galleryId !== undefined) {
        this._batchComplete$.next(galleryId);
      }
      // Clear queue after a short delay so the progress bar stays visible briefly
      this._clearTimer = setTimeout(() => {
        this._queue.set([]);
        this._clearTimer = undefined;
      }, 1200);
      return;
    }

    this._processing = true;
    this._queue.update(q => q.map(j => j.id === pending.id ? { ...j, status: 'uploading' } : j));

    this.photoService.upload(pending.galleryId, { file: pending.file, name: pending.name }).subscribe({
      next: () => {
        this._queue.update(q => q.map(j => j.id === pending.id ? { ...j, status: 'done' } : j));
        this.processNext();
      },
      error: () => {
        this._queue.update(q => q.map(j => j.id === pending.id ? { ...j, status: 'error' } : j));
        this.toastService.show(`Sikertelen: ${pending.name}`, 'danger', 'Feltöltés');
        this.processNext();
      }
    });
  }
}
