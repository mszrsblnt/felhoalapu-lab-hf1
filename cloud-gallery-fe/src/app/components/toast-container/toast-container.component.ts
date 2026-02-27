import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast-container',
  templateUrl: './toast-container.component.html',
  styleUrls: ['./toast-container.component.scss'],
  imports: [CommonModule],
})
export class ToastContainerComponent implements OnInit {
  toastService = inject(ToastService);

  constructor() { }

  ngOnInit() {
  }

}
