
import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet, RouterModule } from '@angular/router';
import { NgxSilkComponent } from '@omnedia/ngx-silk';
import { NgxWordMorphComponent } from '@omnedia/ngx-word-morph';
import { HealthService } from './services/health.service';
import { NavbarComponent } from './layout/navbar/navbar.component';
import { ToastContainerComponent } from "./components/toast-container/toast-container.component";
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule, NgxSilkComponent, NavbarComponent, ToastContainerComponent, DatePipe],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly title = signal('cloud-gallery-fe');
  backendStatus = signal('Checking...');
  databaseStatus = signal('Checking...');
  timestamp = signal<Date|null>(null);

  constructor(private healthService: HealthService) {}

  ngOnInit() {
    this.healthService.getHealth().subscribe({
      next: (res) => {
        this.backendStatus.set(res.backend || 'Healthy')
        this.databaseStatus.set(res.database || 'Unknown')
        this.timestamp.set(res.timeStamp  || null)
      },
      error: (err) => this.backendStatus.set('Unavailable: ' + JSON.stringify(err))
    });
  }


  getPrimaryColor() {
    const style = getComputedStyle(document.documentElement);
    return style.getPropertyValue('--bs-primary').trim() || '#454545';
  }
}
