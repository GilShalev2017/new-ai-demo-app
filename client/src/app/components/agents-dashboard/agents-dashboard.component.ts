import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../../shared/material.module';

@Component({
  selector: 'app-agents-dashboard',
  standalone: true,
  imports: [CommonModule, MaterialModule],
  template: `
    <div class="agents-dashboard-container">
      <h1>Agents Dashboard</h1>
      <p>MCS and other agents monitoring - Coming soon!</p>
    </div>
  `,
  styles: [
    `
      .agents-dashboard-container {
        padding: 24px;
      }
    `,
  ],
})
export class AgentsDashboardComponent {}
