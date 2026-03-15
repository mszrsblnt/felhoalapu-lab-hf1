
import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet, RouterModule } from '@angular/router';
import { NgxDarkVeilComponent } from '@omnedia/ngx-dark-veil';
import { Title } from '@angular/platform-browser';
import { HealthService } from './services/health.service';
import { NavbarComponent } from './layout/navbar/navbar.component';
import { ToastContainerComponent } from "./components/toast-container/toast-container.component";
import { ConfirmModalComponent } from './components/confirm-modal/confirm-modal.component';
import { DatePipe } from '@angular/common';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule,  NavbarComponent, ToastContainerComponent, ConfirmModalComponent, DatePipe, NgxDarkVeilComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly title = signal(environment.appTitle);
  backendStatus = signal('Checking...');
  databaseStatus = signal('Checking...');
  timestamp = signal<Date|null>(null);

  constructor(private healthService: HealthService, private titleService: Title) {
    this.titleService.setTitle(environment.appTitle);
  }

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

}
