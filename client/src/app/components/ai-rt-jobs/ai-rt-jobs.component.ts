import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../../shared/material.module';
import { HttpErrorResponse } from '@angular/common/http';
import { map, Subject, takeUntil } from 'rxjs';
import { AddNewJobComponent } from '../add-new-job/add-new-job.component';
import { MatDialog } from '@angular/material/dialog';
import { AiService } from '../../services/ai.service';
import {
  AiJobRequest,
  AiJobRequestX,
  Channel,
  Interval,
  JobRequestFilter,
  UITag,
} from '../../models/models';
import { MatSnackBar } from '@angular/material/snack-bar';
@Component({
  selector: 'app-ai-jobs',
  standalone: true,
  imports: [CommonModule, MaterialModule],
  templateUrl: './ai-rt-jobs.component.html',
  styleUrls: ['./ai-rt-jobs-component.scss'],
})
export class AiRtJobsComponent implements OnInit {
  destroy$ = new Subject<void>();
  isLoading = true;
  filteredJobs: AiJobRequestX[] = [];
  jobs: AiJobRequestX[] = [];
  readonly statuses: UITag[] = [];
  readonly repetitionFilter: string[] = [];
  dateRange: Interval = { intervalType: 'all' };
  channels: Channel[] = [];

  constructor(
    private dialog: MatDialog,
    private aiService: AiService,
    private snackBar: MatSnackBar) {}

  ngOnInit(): void {}

  addRTJob() {
    const dialogRef = this.dialog.open(AddNewJobComponent, {
      disableClose: true,
      data: { jobType: 'batch' },
      width: '800px',
      maxWidth: '95vw',
    });
    dialogRef
      .afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe((newJob: AiJobRequest) => {
        if (newJob) {
          this.aiService
            .addNewJob(newJob)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.getFilteredJobRequests();
              },
              error: (err: HttpErrorResponse) => {
                console.log(err);
              },
            });
        }
      });
  }

  getChannelById(chnl_id: number): Channel | undefined {
    return this.channels.find((chnl) => chnl.id == chnl_id);
  }

  private getFilteredJobRequests() {
    this.isLoading = true;
    const filter = new JobRequestFilter();
    //filter.Operation = 'DetectKeywords';
    //filter.Start = this.dateRange.from; client side filtering
    //filter.End = this.dateRange.to; client side filtering
    //filter.ChannelIds = this.filtered_selectedChannelsIds; client side filtering
    //filter.Keywords = this.filtered_keywords; client side filtering
    filter.SortDirection = 0;

    this.aiService
      .getFilteredJobRequests(filter)
      .pipe(
        takeUntil(this.destroy$),
        map((jobs) =>
          jobs.map((job) => {
            const channels =
              job.ChannelIds?.map((chnl) => this.getChannelById(chnl)).filter((ch) => ch != null) ??
              [];
            const availNots = this.aiService.availableNotifications;
            const notifications = (
              job?.NotificationIds
                ?.map((id) => availNots.find((n) => n.Id == id)?.Name)
                .filter((name): name is string => name !== undefined) ?? []
            );

            job.RunHistory.forEach((h) => {
              h.errors =
                Object.values(h.Statistics?.ChannelStatistics ?? {}).reduce(
                  (acc, st) => acc + (st?.Errors?.length ?? 0),
                  0,
                ) ?? 0;
            });
            const jx: AiJobRequestX = {
              ...job,
              channels,
              channelsStr: channels.map((chnl) => chnl.displayName).join(', '),
              notifications,
              notificationsStr: notifications.join(', '),
              keywordsStr: job.Keywords?.join(', ') ?? '',
              operationsStr: job.Operations?.join(', ') ?? '',
              errors: job.RunHistory.reduce((acc, h) => acc + (h?.errors ?? 0), 0),
              NextScheduledTime: job.NextScheduledTime || undefined,
            };
            return jx;
          }),
        ),
      )
      .subscribe({
        next: (jobs: AiJobRequestX[]) => {
          this.jobs = jobs;
          this.applyFilters();
          this.isLoading = false;
        },
        error: (err: HttpErrorResponse) => {
          console.error(err);
          this.snackBar.open(`Failed to load jobs ${err.message}`);
        },
      });
  }
  private applyFilters(): void {
    const selectedStatuses = this.statuses
      .filter((status) => status.selected)
      .map((status) => status.text);
    this.filteredJobs = this.jobs.filter(
      (job) =>
        (selectedStatuses.length === 0 || selectedStatuses.includes(job.Status)) &&
        (job.Status === 'In Progress' ||
          job.Status === 'Pending' ||
          (job.BroadcastStartTime && job.BroadcastStartTime >= this.dateRange.from!)) &&
        (this.repetitionFilter.length === 0 ||
          this.repetitionFilter.includes(job.RequestRule?.Recurrence?.Value!)),
    );
  }
}
