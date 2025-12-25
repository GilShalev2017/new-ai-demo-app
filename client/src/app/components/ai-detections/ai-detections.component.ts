import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../../shared/material.module';

@Component({
  selector: 'app-ai-detections',
  standalone: true,
  imports: [CommonModule, MaterialModule],
  template: `
    <div class="ai-detections-container">
      <h1>AI Detections</h1>
      <p>Real-time face detection, object recognition, and ALPR - Coming soon!</p>
    </div>
  `,
  styles: [
    `
      .ai-detections-container {
        padding: 24px;
      }
    `,
  ],
})
export class AiDetectionsComponent {}
