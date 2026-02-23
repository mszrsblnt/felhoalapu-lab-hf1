import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgxSilkComponent } from '@omnedia/ngx-silk';
import { NgxWordMorphComponent } from '@omnedia/ngx-word-morph';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgxSilkComponent,NgxWordMorphComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('cloud-gallery-fe');
}
