import { Injectable, signal } from '@angular/core';

export interface Toast {
  message: string;
  type: 'success' | 'danger' | 'info' | 'warning';
  title?: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toasts = signal<Toast[]>([]);

  show(message: string, type: Toast['type'] = 'info', title?: string) {
    const toast: Toast = { message, type, title };
    this.toasts.update(t => [...t, toast]);

    // 5 másodperc után automatikus eltüntetés
    setTimeout(() => this.remove(toast), 5000);
  }

  remove(toast: Toast) {
    this.toasts.update(t => t.filter(x => x !== toast));
  }
}