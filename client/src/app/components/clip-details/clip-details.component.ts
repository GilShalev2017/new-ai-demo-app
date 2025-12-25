import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MaterialModule } from '../../shared/material.module';
import { ClipService } from '../../services/clip.service';
import {
  CelebrityInsight,
  Clip,
  LocationInsight,
  SummaryInsight,
  Transcript,
  TranscriptionInsight,
} from '../../models/models';
import { ChangeDetectorRef } from '@angular/core';
import { Observable } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { AddInsightsDialogComponent } from '../add-insights-dialog/add-insights-dialog.component';
import { InsightRequest } from '../../models/models';
import { AddChatGptInsightsDialogComponent } from '../add-chat-gpt-insights-dialog/add-chat-gpt-insights-dialog.component';

@Component({
  selector: 'app-clip-details',
  standalone: true,
  imports: [CommonModule, MaterialModule],
  templateUrl: './clip-details.component.html',
  styleUrls: ['./clip-details.component.scss'],
})
export class ClipDetailsComponent implements OnInit {
  clip: Clip | null = null;
  // clip$!: Observable<Clip>;
  clipId: string | null = null;
  loading = true;

  // View options
  selectedTab = 'transcription';
  viewMode: 'single' | 'double' = 'single';

  // Splitter
  leftPaneSize = 60;

  // Transcription data
  transcriptionLines = [
    { timestamp: '00:00', speaker: 'Reporter', text: "I'm going to be a video photographer." },
    { timestamp: '00:03', speaker: 'Reporter', text: "I'm going to be a video photographer." },
    { timestamp: '00:05', speaker: 'Reporter', text: "I'm going to be a video photographer." },
    {
      timestamp: '00:08',
      speaker: 'Narrator',
      text: "Ex formerly Twitter bought in 2022 by the world's richest man Elon Musk.",
    },
    {
      timestamp: '00:14',
      speaker: 'Narrator',
      text: 'He vowed to purge illegal content from the platform.',
    },
    {
      timestamp: '00:18',
      speaker: 'Official',
      text: 'The company told us it has zero tolerance for child sexual abuse material',
    },
    {
      timestamp: '00:23',
      speaker: 'Official',
      text: 'and says it remains a top priority to tackle those who seek to exploit children on our platform.',
    },
    {
      timestamp: '00:30',
      speaker: 'Narrator',
      text: 'Somewhere in Indonesia then, criminals profiting from images of child abuse.',
    },
  ];

  // Celebrities/Personalities
  celebrities = [
    { name: 'Elon Musk', role: 'Entrepreneur, owner of Twitter', nationality: 'American' },
    { name: 'Angus Crawford', role: 'Reporter for BBC News', nationality: 'British' },
    { name: 'Jannegad Gill', role: 'Reporter, presumably for BBC News', nationality: 'Unknown' },
    { name: 'Joe Robertson', role: 'Member of Parliament', nationality: 'British' },
    { name: 'Dave Jones', role: 'Chief Analyst at Ember', nationality: 'Unknown' },
  ];

  // Locations
  locations = [
    {
      name: 'Indonesia',
      context: 'criminals profit from images of child abuse',
      lat: -0.789275,
      lng: 113.921327,
    },
    {
      name: 'Isle of White',
      context: 'helicopter crash killed three people',
      lat: 50.6938,
      lng: -1.3047,
    },
    { name: 'Shanklin and Ventnor, Isle of White', context: 'crash site location' },
  ];

  isErrorProcessing: boolean = false;
  isTranslating: boolean = false;
  @ViewChild('videoPlayer') videoPlayer!: ElementRef<HTMLVideoElement>;
  currentTime = 0;
  isSync = true;
  activeTranscriptIndex: number | null = null;
  processingInsights = new Set<string>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clipService: ClipService,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.clipId = this.route.snapshot.paramMap.get('id');
    if (this.clipId) {
      this.loadClip(this.clipId);
    }
  }

  loadClip(id: string): void {
    this.loading = true;

    // this.clip$ = this.clipService.getClipById(id);

    this.clipService.getClipById(id).subscribe({
      next: (clip) => {
        this.clip = clip;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading clip:', error);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/clips']);
  }

  selectTab(tab: string): void {
    this.selectedTab = tab;
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'single' ? 'double' : 'single';
  }

  onSplitterDrag(event: any): void {
    // Handle splitter drag if needed
  }

  // Action buttons

  manageInsights(): void {
    console.log('Manage Insights');
  }

  chatGPTPrompts(): void {
    //NEW CODE
    const dialogRef = this.dialog.open(AddChatGptInsightsDialogComponent, {
      width: '1000px',
      maxWidth: '95vw',
      //height: '750px',
      //disableClose: true,
      data: {
        existingInsights: ['Transcription', 'Summary'] /*currentInsights*/,
        isUserDefined: true,
        aiClip: this.clip,
      },
    });

    // const currentInsights = this.getClipInsightTypes();

    // const dialogRef = this.dialog.open(AddInsightsComponent, {
    //   width: '750px',
    //   //height: '750px',
    //   disableClose: true,
    //   data: { existingInsights: currentInsights, isUserDefined: true, aiClip: this.aiClip },
    // });

    // dialogRef.afterClosed().subscribe((newInsightRequests) => {
    //   if (newInsightRequests) {
    //     this.mediaInsightService
    //       .addNewInsights(this.aiClip.Id, newInsightRequests)
    //       .pipe(finalize(() => this.setIsAllInsigtExist()))
    //       .subscribe({
    //         next: (clipDm) => {
    //           this.aiClip = clipDm;
    //           this.mediaInsightService.insightAdded$.next(this.aiClip.Id);
    //         },
    //         error: (err: HttpErrorResponse) => {
    //           console.log(err);
    //         },
    //       });
    //   }
    // });
  }

  shareClip(): void {
    console.log('Share Clip');
  }

  reportClip(): void {
    console.log('Report Clip');
  }

  downloadClip(): void {
    console.log('Download Clip');
  }

  addTags(): void {
    console.log('Add Tags');
  }

  syncTranscript(): void {
    console.log('Sync Transcript with Audio');
  }

  saveTranscript(): void {
    console.log('Save Transcript');
  }

  translateTranscript(): void {
    console.log('Translate Transcript');
  }

  formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  getVideoUrl(videoUrl: string): string {
    const serverUrl = 'https://localhost:7176';
    return `${serverUrl}${videoUrl}`;
  }

  /**
   * Returns array-based insights: Transcription, Celebrities, Locations
   */
  getInsightArray(insightType: 'Transcription' | 'Celebrities' | 'Locations'): any[] {
    if (!this.clip?.insights) return [];

    const insight = this.clip.insights.find((i) => i.insightType === insightType);
    if (!insight) return [];

    switch (insightType) {
      case 'Transcription':
        return (insight as TranscriptionInsight).transcripts || [];
      case 'Celebrities':
        return (insight as CelebrityInsight).celebrities || [];
      case 'Locations':
        return (insight as LocationInsight).locations || [];
      default:
        return [];
    }
  }

  /**
   * Returns string-based insight: Summary
   */
  getInsightString(insightType: 'Summary'): string {
    if (!this.clip?.insights) return '';

    const insight = this.clip.insights.find((i) => i.insightType === insightType);
    if (!insight) return '';

    return (insight as SummaryInsight).summary || '';
  }

  /**
   * Delete a specific insight type
   * @param insightType The type of insight to delete (e.g., 'Transcription', 'Summary', 'Celebrities', 'Locations')
   */
  deleteInsight(insightType: string): void {
    if (!this.clip || !this.clipId) return;

    // Find the insight(s) with this type
    const insightsToDelete =
      this.clip.insights?.filter((insight) => insight.insightType === insightType) || [];

    if (insightsToDelete.length === 0) {
      this.snackBar.open(`No ${insightType} insights found to delete`, 'Close', { duration: 3000 });
      return;
    }

    // Get insight IDs
    const insightIds = insightsToDelete.map((insight) => insight.id);

    // Confirm deletion
    const confirmMessage = `Are you sure you want to delete ${insightType} insight${
      insightsToDelete.length > 1 ? 's' : ''
    }?`;

    if (!confirm(confirmMessage)) {
      return;
    }

    // Call API to delete
    this.clipService.removeInsights(this.clipId, insightIds).subscribe({
      next: () => {
        // Remove from local clip object
        if (this.clip && this.clip.insights) {
          this.clip.insights = this.clip.insights.filter(
            (insight) => !insightIds.includes(insight.id)
          );
        }

        this.snackBar.open(`${insightType} deleted successfully`, 'Close', { duration: 3000 });
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error deleting insight:', error);
        this.snackBar.open('Failed to delete insight', 'Close', { duration: 3000 });
      },
    });
  }

  /**
   * Delete the transcription insight
   */
  deleteTranscription(): void {
    this.deleteInsight('Transcription');
  }

  /**
   * Delete the summary insight
   */
  deleteSummary(): void {
    this.deleteInsight('Summary');
  }

  /**
   * Delete the celebrities insight
   */
  deleteCelebrities(): void {
    this.deleteInsight('Celebrities');
  }

  /**
   * Delete the locations insight
   */
  deleteLocations(): void {
    this.deleteInsight('Locations');
  }

  /**
   * Delete the demo insight
   */
  deleteDemoInsight(): void {
    this.deleteInsight('DemoInsight');
  }

  // ============================================
  // HELPER METHODS
  // ============================================

  /**
   * Check if a specific insight type exists
   */
  hasInsight(insightType: string): boolean {
    return this.clip?.insights?.some((insight) => insight.insightType === insightType) || false;
  }

  /**
   * Open dialog to add new insights
   */
  addClipInsights(): void {
    if (!this.clip || !this.clipId) return;

    const dialogRef = this.dialog.open(AddInsightsDialogComponent, {
      width: '700px',
      data: { clip: this.clip },
    });

    dialogRef.afterClosed().subscribe((selectedInsights: InsightRequest[] | undefined) => {
      if (selectedInsights && selectedInsights.length > 0 && this.clipId) {
        this.processAddInsights(selectedInsights);
      }
    });
  }

  /**
   * Process adding insights via API
   */
  private processAddInsights(insights: InsightRequest[]): void {
    if (!this.clipId) return;

    // 🔥 mark as processing
    this.markInsightsProcessing(insights);

    this.cdr.detectChanges();

    this.snackBar.open(`Processing ${insights.length} insight(s)...`, 'Close', {
      duration: 2000,
    });

    this.clipService.addInsights(this.clipId, insights).subscribe({
      next: () => {
        this.snackBar.open('Insights added successfully! Processing in background...', 'Close', {
          duration: 4000,
        });

        // Reload clip after backend had time to finish
        setTimeout(() => {
          if (this.clipId) {
            this.loadClip(this.clipId);
          }

          // ✅ stop spinners
          this.clearInsightsProcessing(insights);
        }, 2000);
      },
      error: (error) => {
        console.error('Error adding insights:', error);

        // ❌ stop spinners on error
        this.clearInsightsProcessing(insights);

        this.snackBar.open('Failed to add insights. Please try again.', 'Close', {
          duration: 4000,
        });
      },
    });
  }

  jumpToTimeFromLabel(transcript: Transcript): void {
    if (!this.videoPlayer) return;
    this.videoPlayer.nativeElement.currentTime = Number(transcript.startInSeconds);
  }

  pad(val: number): string {
    return val < 10 ? `0${val}` : val.toString();
  }

  formatTime(seconds: string): string {
    const totalSeconds = parseFloat(seconds);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const sec = Math.floor(totalSeconds % 60);
    return `${this.pad(hours)}:${this.pad(minutes)}:${this.pad(sec)}`;
  }

  scrollToActiveTranscript(): void {
    // const container = document.querySelector('.transcript-container') as HTMLElement;

    // const activeLine = document.querySelector('.transcript-line.activeTranscript') as HTMLElement;

    // if (!container || !activeLine) return;

    // const offset = activeLine.offsetTop - container.offsetTop - 80;

    // container.scrollTop = offset;
    const activeLine = document.querySelector('.transcript-line.activeTranscript') as HTMLElement;

    if (!activeLine) return;

    activeLine.scrollIntoView({
      behavior: 'smooth',
      block: 'center',
    });
  }

  onTimeUpdate(): void {
    if (!this.videoPlayer || !this.isSync) return;

    const video = this.videoPlayer.nativeElement;
    this.currentTime = video.currentTime;

    const transcripts = this.getInsightArray('Transcription') as Transcript[];
    if (!transcripts.length) {
      this.activeTranscriptIndex = null;
      return;
    }

    let newActiveIndex: number | null = null;

    for (let i = 0; i < transcripts.length; i++) {
      const t = transcripts[i];
      const start = Number(t.startInSeconds);
      const end = Number(t.endInSeconds);

      if (this.currentTime >= start && this.currentTime <= end) {
        newActiveIndex = i;
        break; // ⬅️ first match wins
      }
    }

    // Only update + scroll if changed
    if (newActiveIndex !== this.activeTranscriptIndex) {
      this.activeTranscriptIndex = newActiveIndex;

      if (this.activeTranscriptIndex !== null) {
        setTimeout(() => this.scrollToActiveTranscript(), 0);
      }
    }
  }

  isInsightProcessing(insightType: string): boolean {
    return this.processingInsights.has(insightType);
  }

  markInsightsProcessing(insights: InsightRequest[]) {
    insights.forEach((i) => this.processingInsights.add(i.insightType));
  }

  clearInsightsProcessing(insights: InsightRequest[]) {
    insights.forEach((i) => this.processingInsights.delete(i.insightType));
  }
}
