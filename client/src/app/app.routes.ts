import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/clips',
    pathMatch: 'full',
  },
  {
    path: 'clips',
    loadComponent: () =>
      import('./components/ai-clips-gallery/ai-clips-gallery.component').then(
        (m) => m.AiClipsGalleryComponent
      ),
    title: 'Clips - Media Insight',
  },
  {
    path: 'clips/:id',
    loadComponent: () =>
      import('./components/clip-details/clip-details.component').then(
        (m) => m.ClipDetailsComponent
      ),
    title: 'Clip Details - Media Insight',
  },
   {
    path: 'transcript-chat',
    loadComponent: () =>
      import('./components/transcript-chat/transcript-chat.component').then(
        (m) => m.TranscriptChatComponent
      ),
    title: 'Transcript Chat',
  },
  {
    path: 'ai-detections',
    loadComponent: () =>
      import('./components/ai-detections/ai-detections.component').then(
        (m) => m.AiDetectionsComponent
      ),
    title: 'AI Detections - Media Insight',
  },
  {
    path: 'scheduled-items',
    loadComponent: () =>
      import('./components/ai-rt-jobs/ai-rt-jobs.component').then(
        (m) => m.AiRtJobsComponent
      ),
    title: 'Scheduled Items - Media Insight',
  },
  {
    path: 'jobs',
    loadComponent: () =>
      import('./components/job-scheduler/job-scheduler.component').then((m) => m.JobSchedulerComponent),
    title: 'Job Scheduler - Media Insight',
  },
  {
    path: 'agents',
    loadComponent: () =>
      import('./components/agents-dashboard/agents-dashboard.component').then(
        (m) => m.AgentsDashboardComponent
      ),
    title: 'Agents - Media Insight',
  },
  {
    path: '**',
    redirectTo: '/clips',
  },
];
