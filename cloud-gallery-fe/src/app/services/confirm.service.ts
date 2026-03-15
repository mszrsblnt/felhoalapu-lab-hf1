import { Injectable, signal } from '@angular/core';

export interface ConfirmState {
  visible: boolean;
  message: string;
  okLabel: string;
  cancelLabel: string;
  resolve: ((v: boolean) => void) | null;
}

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  readonly state = signal<ConfirmState>({
    visible: false,
    message: '',
    okLabel: 'OK',
    cancelLabel: 'Mégse',
    resolve: null,
  });

  open(message: string, okLabel = 'OK', cancelLabel = 'Mégse'): Promise<boolean> {
    return new Promise(resolve => {
      this.state.set({ visible: true, message, okLabel, cancelLabel, resolve });
    });
  }

  confirm(): void {
    this.state().resolve?.(true);
    this._close();
  }

  cancel(): void {
    this.state().resolve?.(false);
    this._close();
  }

  private _close(): void {
    this.state.set({ visible: false, message: '', okLabel: 'OK', cancelLabel: 'Mégse', resolve: null });
  }
}
