import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgxSilkComponent } from '@omnedia/ngx-silk';
import { NgxWordMorphComponent } from '@omnedia/ngx-word-morph';
import { HealthService } from './services/health.service';
import { errorContext } from 'rxjs/internal/util/errorContext';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgxSilkComponent,NgxWordMorphComponent],
  templateUrl: './app.html',  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly title = signal('cloud-gallery-fe');
  backendStatus = signal('Checking...');
  databaseStatus = signal('Checking...');

  constructor(private healthService: HealthService) {}

  ngOnInit() {
    this.healthService.getHealth().subscribe({
      next: (res) => {
        this.backendStatus.set(res.backend || 'Healthy')
        this.databaseStatus.set(res.database || 'Unknown')
      },
      error: (err) => this.backendStatus.set('Unavailable: ' + JSON.stringify(err))
    });
  }
}
