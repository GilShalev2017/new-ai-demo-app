import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../../shared/material.module';

@Component({
  selector: 'app-scheduled-items',
  standalone: true,
  imports: [CommonModule, MaterialModule],
  template: `
    <div class="scheduled-items-container">
      <h1>Scheduled Items</h1>
      <p>Manage scheduled clip processing - Coming soon!</p>
    </div>
  `,
  styles: [
    `
      .scheduled-items-container {
        padding: 24px;
      }
    `,
  ],
})
export class ScheduledItemsComponent {}
