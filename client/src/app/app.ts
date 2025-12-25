import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AiClipsGalleryComponent } from './components/ai-clips-gallery/ai-clips-gallery.component';
import { MaterialModule } from './shared/material.module';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, MaterialModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('ai-demo-app');

  mediaInsightExpanded = true;

  toggleMediaInsight(): void {
    this.mediaInsightExpanded = !this.mediaInsightExpanded;
  }
}
