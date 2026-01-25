import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MaterialModule } from '../../shared/material.module';
import { AiService } from '../../services/ai.service';
import {
  CelebrityInsight,
  Clip,
  LocationInsight,
  SummaryInsight,
  Transcript,
  TranscriptionInsight,
} from '../../models/models';
import { ChangeDetectorRef } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { AddInsightsDialogComponent } from '../add-insights-dialog/add-insights-dialog.component';
import { InsightRequest } from '../../models/models';
import { AddChatGptInsightsDialogComponent } from '../add-chat-gpt-insights-dialog/add-chat-gpt-insights-dialog.component';
import * as vision from '@mediapipe/tasks-vision';
import { FormsModule } from '@angular/forms';
const { FaceLandmarker, FilesetResolver } = vision;

@Component({
  selector: 'app-clip-details',
  standalone: true,
  imports: [CommonModule, FormsModule, MaterialModule],
  templateUrl: './clip-details.component.html',
  styleUrls: ['./clip-details.component.scss'],
})
export class ClipDetailsComponent implements OnDestroy, OnInit, AfterViewInit {
  clip: Clip | null = null;
  clipId: string | null = null;
  loading = true;
  selectedTab = 'transcription';
  viewMode: 'single' | 'double' = 'single';
  leftPaneSize = 60;

  celebrities = [
    { name: 'Elon Musk', role: 'Entrepreneur, owner of Twitter', nationality: 'American' },
    { name: 'Angus Crawford', role: 'Reporter for BBC News', nationality: 'British' },
    { name: 'Jannegad Gill', role: 'Reporter, presumably for BBC News', nationality: 'Unknown' },
    { name: 'Joe Robertson', role: 'Member of Parliament', nationality: 'British' },
    { name: 'Dave Jones', role: 'Chief Analyst at Ember', nationality: 'Unknown' },
  ];
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
  currentTime = 0;
  isSync = true;
  activeTranscriptIndex: number | null = null;
  processingInsights = new Set<string>();

  @ViewChild('videoPlayer') videoPlayer!: ElementRef<HTMLVideoElement>;
  @ViewChild('outputCanvas') canvasRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('videoProgress') videoProgressRef!: ElementRef<HTMLInputElement>;
  @ViewChild('videoBlendShapes') videoBlendShapesRef!: ElementRef<HTMLUListElement>;
  private faceLandmarker!: vision.FaceLandmarker;
  public faceLandmarkerReady: boolean = false;
  public videoLoaded: boolean = false;
  public isPlaying: boolean = false;
  public videoDuration: number = 0;
  public videoCurrentTime: number = 0;
  private drawingUtils!: vision.DrawingUtils;
  private canvasCtx!: CanvasRenderingContext2D;
  private animationFrameId: number | null = null;
  private lastVideoTime: number = -1;
  showFaceDetection = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clipService: AiService,
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

  manageInsights(): void {
    console.log('Manage Insights');
  }

  chatGPTPrompts(): void {
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

  formatDuration(seconds?: number): string {
    if (seconds === undefined || seconds === null) {
      return '0:00';
    }
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  getVideoUrl(videoUrl: string): string {
    const serverUrl = 'https://localhost:7176';
    return `${serverUrl}${videoUrl}`;
  }

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

  getInsightString(insightType: 'Summary'): string {
    if (!this.clip?.insights) return '';

    const insight = this.clip.insights.find((i) => i.insightType === insightType);
    if (!insight) return '';

    return (insight as SummaryInsight).summary || '';
  }

  deleteInsight(insightType: string): void {
    if (!this.clip || !this.clipId) return;

    const insightsToDelete =
      this.clip.insights?.filter((insight) => insight.insightType === insightType) || [];

    if (insightsToDelete.length === 0) {
      this.snackBar.open(`No ${insightType} insights found to delete`, 'Close', { duration: 3000 });
      return;
    }

    const insightIds = insightsToDelete.map((insight) => insight.id);

    const confirmMessage = `Are you sure you want to delete ${insightType} insight${
      insightsToDelete.length > 1 ? 's' : ''
    }?`;

    if (!confirm(confirmMessage)) {
      return;
    }

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

  deleteTranscription(): void {
    this.deleteInsight('Transcription');
  }

  deleteSummary(): void {
    this.deleteInsight('Summary');
  }

  deleteCelebrities(): void {
    this.deleteInsight('Celebrities');
  }

  deleteLocations(): void {
    this.deleteInsight('Locations');
  }

  deleteDemoInsight(): void {
    this.deleteInsight('DemoInsight');
  }

  hasInsight(insightType: string): boolean {
    return this.clip?.insights?.some((insight) => insight.insightType === insightType) || false;
  }

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

  ngAfterViewInit(): void {
    if (!this.videoPlayer || !this.canvasRef) {
      console.error(
        'VideoFileFaceDetectionComponent: Missing required elements - videoPlayer:',
        !!this.videoPlayer,
        'canvasRef:',
        !!this.canvasRef
      );
      return;
    }

    const canvasElement = this.canvasRef.nativeElement;

    this.canvasCtx = canvasElement.getContext('2d')!;

    this.drawingUtils = new vision.DrawingUtils(this.canvasCtx);

    // Initialize face landmarker
    this.createFaceLandmarker()
      .then(() => {
        console.log('Face landmarker initialization complete');
      })
      .catch((error: any) => {
        console.error('Error initializing face landmarker:', error);
      });
  }

  async createFaceLandmarker(): Promise<void> {
    try {
      const filesetResolver = await FilesetResolver.forVisionTasks(
        'https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.3/wasm'
      );

      this.faceLandmarker = await vision.FaceLandmarker.createFromOptions(filesetResolver, {
        baseOptions: {
          modelAssetPath: `https://storage.googleapis.com/mediapipe-models/face_landmarker/face_landmarker/float16/1/face_landmarker.task`,
          delegate: 'CPU', //'GPU',
        },
        outputFaceBlendshapes: true,
        runningMode: 'VIDEO',
        numFaces: 10,
        minFaceDetectionConfidence: 0.3, // Lower confidence threshold for more sensitive detection
      });

      if (this.faceLandmarker) {
        this.faceLandmarkerReady = true;
        console.log('FaceLandmarker initialized successfully');
      } else {
        throw new Error('FaceLandmarker initialization failed');
      }

      this.cdr.detectChanges();
    } catch (error) {
      console.error('Error creating FaceLandmarker:', error);
      this.faceLandmarkerReady = false;
      this.cdr.detectChanges();
    }
  }

  ngOnDestroy(): void {
    // Remove resize listener
    window.removeEventListener('resize', this.handleResize);

    // Stop any ongoing animation frame
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = null;
    }

    // Close face landmarker if it exists
    if (this.faceLandmarker) {
      this.faceLandmarker.close();
    }

    // Clean up video source if needed
    if (this.videoPlayer?.nativeElement?.src) {
      URL.revokeObjectURL(this.videoPlayer.nativeElement.src);
    }
  }

  onVideoPlay(): void {
    console.log('Video play event triggered');
    this.isPlaying = true;

    // If face detection is ready, start predicting
    if (this.faceLandmarkerReady && this.faceLandmarker) {
      console.log('Starting face detection on play');
      if (this.showFaceDetection) {
        this.predictVideoFrame();
      }
    }

    // Update the play state
    this.cdr.detectChanges();
  }

  onVideoPause(): void {
    console.log('Video pause event triggered');
    this.isPlaying = false;

    // Stop any ongoing face detection
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = null;
    }

    this.cdr.detectChanges();
  }

  onVideoLoadedMetadata(): void {
    console.log('onVideoLoadedMetadata called');
    if (!this.videoPlayer) {
      console.error('onVideoLoadedMetadata: Missing videoPlayerRef');
      return;
    }

    const video = this.videoPlayer.nativeElement;
    console.log('Video duration:', video.duration);
    this.videoDuration = video.duration;
    this.videoCurrentTime = video.currentTime;

    // Set canvas dimensions to match video dimensions
    if (!this.canvasRef) {
      console.error('onVideoLoadedMetadata: Missing canvasRef');
      return;
    }

    const canvasElement = this.canvasRef.nativeElement;
    canvasElement.width = video.videoWidth;
    canvasElement.height = video.videoHeight;

    // Set up resize handling
    this.handleResize();
    window.addEventListener('resize', this.handleResize);

    // If video is already playing (e.g., autoplay), start face detection
    if (!video.paused && this.faceLandmarkerReady && this.faceLandmarker) {
      console.log('Video already playing, starting face detection');
      this.isPlaying = true;
      this.predictVideoFrame();
    }

    this.cdr.detectChanges();
  }

  private handleResize = (): void => {
    if (!this.videoPlayer || !this.canvasRef) return;

    const video = this.videoPlayer.nativeElement;
    const canvas = this.canvasRef.nativeElement;

    // Calculate new dimensions while maintaining aspect ratio
    const aspectRatio = video.videoWidth / video.videoHeight;
    const containerWidth = video.parentElement?.clientWidth || video.videoWidth;
    const containerHeight = containerWidth / aspectRatio;

    canvas.width = containerWidth;
    canvas.height = containerHeight;
  };

  private predictVideoFrame(forceDetection: boolean = false): void {
    if (this.showFaceDetection === false) {
      return;
    }
    // If we're not playing and not forcing detection, just return
    if (!this.isPlaying && !forceDetection) {
      return;
    }

    // Check if we have all required elements and they're ready
    if (
      !this.videoPlayer?.nativeElement ||
      !this.canvasRef?.nativeElement ||
      !this.faceLandmarker ||
      !this.drawingUtils
    ) {
      // If we're supposed to be playing, schedule a retry
      if (this.isPlaying) {
        this.animationFrameId = requestAnimationFrame(() => this.predictVideoFrame(forceDetection));
      }
      return;
    }

    const video = this.videoPlayer.nativeElement;
    const canvasElement = this.canvasRef.nativeElement;

    // Make sure video has valid dimensions
    if (video.videoWidth === 0 || video.videoHeight === 0) {
      if (this.isPlaying) {
        this.animationFrameId = requestAnimationFrame(() => this.predictVideoFrame(forceDetection));
      }
      return;
    }

    // Update canvas dimensions to match video if needed
    if (canvasElement.width !== video.videoWidth || canvasElement.height !== video.videoHeight) {
      canvasElement.width = video.videoWidth;
      canvasElement.height = video.videoHeight;
    }

    try {
      // Only detect if the video time has changed or if forced
      const currentTime = video.currentTime;
      if (this.lastVideoTime !== currentTime || forceDetection) {
        this.lastVideoTime = currentTime;

        // Perform face detection
        const startTimeMs = performance.now();
        const results = this.faceLandmarker.detectForVideo(video, startTimeMs);

        // Draw results if we have a valid canvas context
        if (this.canvasCtx) {
          this.canvasCtx.save();
          this.canvasCtx.clearRect(0, 0, canvasElement.width, canvasElement.height);

          // Draw face landmarks if detected
          if (results.faceLandmarks && results.faceLandmarks.length > 0) {
            for (const landmarks of results.faceLandmarks) {
              // Draw face mesh
              this.drawingUtils.drawConnectors(
                landmarks,
                vision.FaceLandmarker.FACE_LANDMARKS_TESSELATION,
                { color: '#C0C0C070', lineWidth: 1 }
              );

              // Draw eyes
              this.drawingUtils.drawConnectors(
                landmarks,
                vision.FaceLandmarker.FACE_LANDMARKS_RIGHT_EYE,
                // { color: '#FF3030' }
                { color: 'blue' }
              );
              this.drawingUtils.drawConnectors(
                landmarks,
                vision.FaceLandmarker.FACE_LANDMARKS_LEFT_EYE,
                //{ color: '#30FF30' }
                { color: 'blue' }
              );

              // Draw face oval
              this.drawingUtils.drawConnectors(
                landmarks,
                vision.FaceLandmarker.FACE_LANDMARKS_FACE_OVAL,
                { color: '#E0E0E0' }
              );

              // Draw lips
              this.drawingUtils.drawConnectors(
                landmarks,
                vision.FaceLandmarker.FACE_LANDMARKS_LIPS,
                //{ color: '#E0E0E0' }
                { color: 'red' }
              );
            }
          }
          this.canvasCtx.restore();
        }

        // Update blend shapes if available
        if (this.videoBlendShapesRef?.nativeElement && results.faceBlendshapes) {
          this.drawBlendShapes(this.videoBlendShapesRef.nativeElement, results.faceBlendshapes);
        }
      }

      // Schedule next frame if still playing
      if (this.isPlaying) {
        this.animationFrameId = requestAnimationFrame(() => this.predictVideoFrame(forceDetection));
      }
    } catch (error) {
      console.error('Error during face detection:', error);
      // Even if there's an error, try to continue with the next frame
      if (this.isPlaying) {
        this.animationFrameId = requestAnimationFrame(() => this.predictVideoFrame(forceDetection));
      }
    }
  }

  drawBlendShapes(el: HTMLElement, blendShapes: any[]): void {
    if (!blendShapes || !blendShapes.length) {
      el.innerHTML = '';
      return;
    }

    let htmlMaker = '';
    blendShapes[0].categories.map((shape: any) => {
      htmlMaker += `
        <li class="blend-shapes-item">
          <span class="blend-shapes-label">${shape.displayName || shape.categoryName}</span>
          <span class="blend-shapes-value" style="width: calc(${
            +shape.score * 100
          }% - 120px)">${(+shape.score).toFixed(4)}</span>
        </li>
      `;
    });

    el.innerHTML = htmlMaker;
  }
  onFaceDetectionToggle(): void {
    if (this.showFaceDetection === true) {
      // If turning on, start detection if video is playing
      if (this.videoPlayer?.nativeElement && !this.videoPlayer.nativeElement.paused) {
        this.predictVideoFrame(true);
      }
    } else {
      // If turning off, clear the canvas
      if (this.canvasRef?.nativeElement && this.canvasCtx) {
        this.canvasCtx.clearRect(
          0,
          0,
          this.canvasRef.nativeElement.width,
          this.canvasRef.nativeElement.height
        );
      }
      // Cancel any pending animation frames
      if (this.animationFrameId) {
        cancelAnimationFrame(this.animationFrameId);
        this.animationFrameId = null;
      }
    }
  }
}
