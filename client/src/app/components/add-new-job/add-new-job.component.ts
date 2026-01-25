import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Router } from '@angular/router';
import {
  AiJobRequest,
  InsightTypeDefinition,
  LanguageDm,
  NotificationDefinition,
  Rule,
  RuleRecurrenceEnum,
  TimeRange,
  VisibilityEnum,
} from '../../models/models';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { HttpErrorResponse } from '@angular/common/http';
import { MatSelectChange } from '@angular/material/select';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../../shared/material.module';
import { AiService } from '../../services/ai.service';

export type RepeatMode = 'repeat-none' | 'repeat-daily' | 'repeat-weekly' | 'continous';
export type WeekDays = 'sun' | 'mon' | 'tue' | 'wed' | 'thu' | 'fri' | 'sat';

// Custom validator to ensure at least one item is selected
export function atLeastOneItemSelectedValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const selected = control.value;

    // If nothing selected or empty array
    if (!selected || (Array.isArray(selected) && selected.length === 0)) {
      console.debug('No items selected');
      return { atLeastOneRequired: true };
    }

    console.debug('Something selected');
    return null;
  };
}

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MaterialModule],
  templateUrl: './add-new-job.component.html',
  styleUrls: ['./add-new-job.component.scss'],
})
export class AddNewJobComponent implements OnInit {
  aiJobDm: AiJobRequest = new AiJobRequest();
  availableChannels: string[] = ['Channel 1', 'Channel 2', 'Channel 3', 'Channel 4'];
  selectedChannels: string[] = [];
  languages: LanguageDm[] = [
    { EnglishName: 'en', DisplayName: 'English' },
    { EnglishName: 'es', DisplayName: 'Spanish' },
    { EnglishName: 'fr', DisplayName: 'French' },
    { EnglishName: 'de', DisplayName: 'German' },
  ];
  availableNotifications: NotificationDefinition[] = [
    {
      Id: 'gils',
      Name: 'Gil Shalev',
      Visibility: VisibilityEnum.Everyone,
      CreatedDate: new Date(),
    },
    {
      Id: 'gilshalev',
      Name: 'Gils',
      Visibility: VisibilityEnum.Everyone,
      CreatedDate: new Date(),
    },
    {
      Id: 'brooklyn',
      Name: 'Brooklyn Kopper',
      Visibility: VisibilityEnum.Everyone,
      CreatedDate: new Date(),
    },
  ];
  // KEYWORD TAB
  selectedKeywordLanguage: LanguageDm | null = null;
  searchTerms = '';
  selectedNotifications: NotificationDefinition[] = [];
  // CLOSED CAPTIONS TAB
  saveClosedCaptions = true;
  selectedLanguages: LanguageDm[] = [];
  // FACE / LOGO DETECTION
  enableFaceDetection = false;
  enableLogoDetection = false;
  repeatForm!: FormGroup;
  selectedDate!: Date;
  fromTime!: string;
  toTime!: string;
  VisibilityEnum = VisibilityEnum;
  repeatMode: RepeatMode = 'repeat-none';
  repeatWeekDays: WeekDays[] = [];
  private readonly DaysMap: Map<WeekDays, number> = new Map([
    ['sun', 1],
    ['mon', 2],
    ['tue', 4],
    ['wed', 8],
    ['thu', 16],
    ['fri', 32],
    ['sat', 64],
  ]);
  constructor(
    private dialogRef: MatDialogRef<AddNewJobComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { jobType: 'continous' | 'batch' },
    private aiService: AiService,
    private router: Router,
    private fb: FormBuilder,
  ) {}

  ngOnInit(): void {
    this.initForms();
    this.setDefaults();
    this.initDateTimeDefaults();
  }

  private initForms(): void {
    this.repeatForm = this.fb.group({
      repeatModeControl: ['repeat-none'],
      repeatWeekDaysControl: [[]],
    });
  }

  private setDefaults(): void {
    this.aiJobDm.Visibility = VisibilityEnum.Everyone;
    this.selectedKeywordLanguage = this.languages[0]; // English
  }
  private initDateTimeDefaults(): void {
    const now = new Date();

    this.selectedDate = new Date(now.getFullYear(), now.getMonth(), now.getDate());

    const roundedFrom = this.roundUpToNext5Minutes(now);
    const roundedTo = new Date(roundedFrom.getTime() + 5 * 60000);

    this.fromTime = this.formatTime(roundedFrom);
    this.toTime = this.formatTime(roundedTo);
  }

  private roundUpToNext5Minutes(date: Date): Date {
    const rounded = new Date(date);
    const minutes = rounded.getMinutes();
    const remainder = minutes % 5;

    if (remainder !== 0) {
      rounded.setMinutes(minutes + (5 - remainder));
    }

    rounded.setSeconds(0);
    rounded.setMilliseconds(0);

    return rounded;
  }

  private formatTime(date: Date): string {
    const h = date.getHours().toString().padStart(2, '0');
    const m = date.getMinutes().toString().padStart(2, '0');
    return `${h}:${m}`;
  }

  onKeywordLanguageChange(language: LanguageDm): void {
    this.selectedKeywordLanguage = language;
  }

  onAlertDestinationsChange(selectedNotifications: NotificationDefinition[]): void {
    this.selectedNotifications = selectedNotifications;
  }

  gotoNotifications(): void {
    this.dialogRef.close();
    this.router.navigateByUrl('/media-insight/ai-settings', {
      state: {
        index: 1,
      },
    });
  }

  isDialogValid(): boolean {
    return (
      !!this.aiJobDm.Name &&
      this.selectedChannels.length > 0 &&
      (this.searchTerms.trim().length > 0 ||
        this.saveClosedCaptions ||
        this.enableFaceDetection ||
        this.enableLogoDetection)
    );
  }

  onCreateJob(): void {
    const payload = {
      job: this.aiJobDm,
      channels: this.selectedChannels,
      keywords: {
        language: this.selectedKeywordLanguage,
        terms: this.searchTerms.split(',').map((t) => t.trim()),
        destinations: this.selectedNotifications,
      },
      closedCaptions: {
        enabled: this.saveClosedCaptions,
        languages: this.selectedLanguages,
      },
      detections: {
        face: this.enableFaceDetection,
        logo: this.enableLogoDetection,
      },
    };

    console.log('Creating AI Job:', payload);

    // TODO: send payload to backend
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
/*
 ngOnInit(): void {
    this.populateLanguages();
    this.initTimeControls();
    this.aiJobDm.Visibility = VisibilityEnum.Everyone;
    this.reportType = 'unified';
    this.getAllNotifications();
    this.enabledKeywordsDetection = true;

    this.availableNotifications = this.aiService.availableNotifications;

    this.repeatForm = this.fb.group({
      isRequired: [false],
      repeatModeControl: ['repeat-none'],
      repeatWeekDaysControl: [[]],
    });

    const repeatWeekDaysControl = this.repeatForm.get('repeatWeekDaysControl');
    if (repeatWeekDaysControl) {
      repeatWeekDaysControl.valueChanges.subscribe((selectedDays: WeekDays[]) => {
        this.repeatWeekDays = selectedDays;
      });
      repeatWeekDaysControl.markAsTouched();
    }

    const repeatModeControl = this.repeatForm.get('repeatModeControl');
    if (repeatModeControl) {
      repeatModeControl.valueChanges.subscribe((selectedMode: RepeatMode) => {
        this.repeatMode = selectedMode;
        this.evalEnableRepeatWeekDaysControl();
        // This is crucial - update validity after changing validators
        if (repeatWeekDaysControl) {
          repeatWeekDaysControl.updateValueAndValidity();
        }
      });
    }

    this.evalEnableRepeatWeekDaysControl();

    this.isTranscriptionSupported =
      this.aiService.availableInsightTypeDefinitions.find(
        (insightDef) => insightDef.DisplayName.toLowerCase() === 'transcription',
      ) !== undefined
        ? true
        : false;
    this.isTranslationSupported =
      this.aiService.availableInsightTypeDefinitions.find(
        (insightDef: InsightTypeDefinition) =>
          insightDef.DisplayName.toLowerCase() === 'translation',
      ) !== undefined
        ? true
        : false;
    this.isFaceDetectionSupported =
      this.aiService.availableInsightTypeDefinitions.find(
        (insightDef: InsightTypeDefinition) =>
          insightDef.DisplayName.toLowerCase() === 'facedetection',
      ) !== undefined
        ? true
        : false;
    this.isLicensePlateDetectionSupported =
      this.aiService.availableInsightTypeDefinitions.find(
        (insightDef: InsightTypeDefinition) =>
          insightDef.DisplayName.toLowerCase() === 'licenseplatedetection',
      ) !== undefined
        ? true
        : false;
  }

  ngOnDestroy(): void {
    // Signal to complete all subscriptions
    this.destroy$.next();
    this.destroy$.complete();
  }

  getAllNotifications() {
    this.aiService
      .getAllNotifications()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notifications: NotificationDefinition[]) => {
          this.aiService.availableNotifications = notifications;
        },
        error: (err: HttpErrorResponse) => {
          console.log(err);
        },
      });
  }

  populateLanguages() {
    this.aiService.providerLanguages$
      .pipe(takeUntil(this.destroy$))
      .subscribe((languages: LanguageDm[]) => {
        this.languages = languages;
        // if (languages.length > 0) {
        //   this.selectedKeywordLanguage = languages[0];
        // }
      });
  }

  onChannelSelectionChanged(selectedChannels: any[]) {
    this.aiJobDm.ChannelIds = selectedChannels.map((channel) => channel.id);
  }

  onRangeChanged(selectedTimeRange: TimeRange) {
    this.userSelectedTimeRange = selectedTimeRange;
  }

  initTimeControls() {
    const currentTime = new Date();
    currentTime.setMinutes(currentTime.getMinutes() - 30); // Subtract 30 minutes
    const minutes = currentTime.getMinutes();
    const roundedMinutes = Math.round(minutes / 5) * 5; // Round to the nearest multiple of 5
    currentTime.setMinutes(roundedMinutes); // Set rounded minutes
    currentTime.setMinutes(currentTime.getMinutes() + 5); // Add 5 minutes
    currentTime.setSeconds(0);

    this.timeRange = new TimeRange(
      new Date(currentTime.setMinutes(currentTime.getMinutes() - 5)),
      new Date(currentTime.setMinutes(currentTime.getMinutes() + 5)),
    );
    this.userSelectedTimeRange = new TimeRange(
      new Date(currentTime.setMinutes(currentTime.getMinutes() - 5)),
      new Date(currentTime.setMinutes(currentTime.getMinutes() + 5)),
    );
  }

  onKeywordsDetectionChange(event: MatSlideToggleChange): void {
    this.enabledKeywordsDetection = event.checked;
  }

  onLanguageMismatchChange(event: MatSlideToggleChange): void {
    this.enabledLanguageMismatch = event.checked;
  }

  onKeywordLanguageChange(event: MatSelectChange): void {
    this.selectedKeywordLanguage = event.value;
  }

  onAlertDestinationsChange(event: any): void {
    this.selectedAlertDestinations = event.value;
    //console.log("Updated Shared Alert Destinations:", this.selectedAlertDestinations);
  }

  gotoNotifications() {
    this.dialogRef.close();
    this.router.navigateByUrl('/media-insight/ai-settings', {
      state: {
        index: 1,
      },
    });
  }

  onClose() {
    this.dialogRef.close();
  }

  isDialogValid() {
    if (!this.aiJobDm.Name) return false;
    if (this.aiJobDm.ChannelIds?.length === 0) return false;
    if (
      this.repeatMode !== 'continous' &&
      this.aiJobDm.BroadcastStartTime! >= this.aiJobDm.BroadcastEndTime!
    )
      return false;
    if (
      this.repeatMode !== 'continous' &&
      this.aiJobDm.BroadcastStartTime! >= this.aiJobDm.BroadcastEndTime!
    )
      return false;
    if (
      this.repeatMode !== 'continous' &&
      this.repeatMode === 'repeat-weekly' &&
      this.repeatWeekDays.length === 0
    ) {
      return false;
    }
    if (this.getOperations().length === 0) {
      return false;
    }

    return true;
  }

  private createRequestRule(): Rule {
    let requestRule: Rule;
    switch (this.repeatMode) {
      case 'repeat-none':
      default:
        requestRule = {
          Recurrence: RuleRecurrenceEnum.Once,
          Days: 0,
        };
        break;
      case 'repeat-daily':
        requestRule = {
          Recurrence: RuleRecurrenceEnum.Recurring,
          Days: 127, // All days of the week
        };
        break;
      case 'repeat-weekly':
        requestRule = {
          Recurrence: RuleRecurrenceEnum.Recurring,
          Days: this.repeatWeekDays
            .map((day) => this.DaysMap.get(day))
            .filter((val): val is number => val !== undefined) // This ensures we only have numbers
            .reduce((acc, val) => acc | val, 0),
        };
        break;
    }

    return requestRule;
  }

  private getOperations() {
    const operations = [];
    if (this.searchTerms !== '') operations.push('DetectKeywords');
    if (this.enabledLanguageMismatch) operations.push('VerifyAudioLanguage');
    if (this.saveClosedCaptions) operations.push('CreateClosedCaptions');
    if (this.enableFaceDetection) operations.push('DetectFaces');
    if (this.enableLicensePlateDetection) operations.push('DetectLicensePlates');

    return operations;
  }

  onCreateJob() {
    this.aiJobDm.BroadcastStartTime = this.userSelectedTimeRange?.from!;
    this.aiJobDm.BroadcastEndTime = this.userSelectedTimeRange?.to!;

    const requestRule = this.createRequestRule();

    const operations = this.getOperations();

    const trimmedLowerCaseKeywords = Array.from(
      new Set(
        this.searchTerms
          .split(',')
          .map((keyword) => keyword.trim().toLowerCase()) //important look for trimmed, lower-case distinct list of words!
          .filter((keyword) => keyword.length > 0),
      ),
    );

    const aiJobRequest: AiJobRequest = {
      //Visibility: this.aiJobDm.Visibility,
      Name: this.aiJobDm.Name,
      Operations: operations!,
      NotificationIds: this.selectedNotifications,
      ChannelIds: this.aiJobDm.ChannelIds,
      BroadcastStartTime: this.aiJobDm.BroadcastStartTime,
      BroadcastEndTime: this.aiJobDm.BroadcastEndTime,
      RequestRule: requestRule,
      Keywords: trimmedLowerCaseKeywords,
      KeywordsLangauges: this.selectedKeywordLanguage?.EnglishName
        ? [this.selectedKeywordLanguage?.EnglishName]
        : [],
      RunHistory: [],
    };

    if (this.selectedLanguages?.length > 0) {
      aiJobRequest?.Operations?.push('TranslateTranscription');
      if (this.selectedKeywordLanguage?.EnglishName) {
        aiJobRequest.TranslationLanguages = [];
        aiJobRequest.TranslationLanguages.push(this.selectedKeywordLanguage.EnglishName);
      }
    }
    if (this.selectedKeywordLanguage != null) {
      if (!aiJobRequest?.Operations?.includes('TranslateTranscription')) {
        aiJobRequest.Operations!.push('TranslateTranscription');
      }
      if (aiJobRequest.TranslationLanguages == undefined) {
        aiJobRequest.TranslationLanguages = [];
        aiJobRequest.TranslationLanguages.push(this.selectedKeywordLanguage?.EnglishName ?? '');
      } else if (
        !aiJobRequest.TranslationLanguages?.includes(
          this.selectedKeywordLanguage?.EnglishName ?? '',
        )
      ) {
        aiJobRequest.TranslationLanguages.push(this.selectedKeywordLanguage?.EnglishName ?? '');
      }
    }

    if (this.data.jobType === 'continous') {
      aiJobRequest.BroadcastStartTime = new Date();
      aiJobRequest.BroadcastEndTime = new Date();
      aiJobRequest.RequestRule!.Recurrence = RuleRecurrenceEnum.Continuous;
    }

    this.dialogRef.close(aiJobRequest);
  }

  formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are zero-based
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');
    return `${year}_${month}_${day}_${hours}_${minutes}_${seconds}`;
  }

  private evalEnableRepeatWeekDaysControl() {
    const repeatWeekDaysControl = this.repeatForm.get('repeatWeekDaysControl');
    if (this.repeatMode === 'repeat-weekly') {
      repeatWeekDaysControl?.enable();
      repeatWeekDaysControl?.setValidators([atLeastOneItemSelectedValidator()]);
    } else {
      repeatWeekDaysControl?.disable();
      repeatWeekDaysControl?.clearValidators();
    }
  }

  get timeRange() {
    return this.timeRange_;
  }

  set timeRange(timeRange) {
    this.timeRange_ = timeRange;
  }*/

/*
  aiJobDm: AiJobRequest = new AiJobRequest();
  VisibilityEnum = VisibilityEnum;
  initialChannelIds = [];
  timeRange_: TimeRange | undefined;
  userSelectedTimeRange: TimeRange | undefined;
  repeatForm!: FormGroup;
  repeatMode: RepeatMode = 'repeat-none';
  repeatWeekDays: WeekDays[] = [];
  private readonly DaysMap: Map<WeekDays, number> = new Map([
    ['sun', 1],
    ['mon', 2],
    ['tue', 4],
    ['wed', 8],
    ['thu', 16],
    ['fri', 32],
    ['sat', 64],
  ]);
  enabledKeywordsDetection: boolean = true;
  checked = true;
  enabledLanguageMismatch: boolean = false;
  searchTerms = '';
  selectedChannels: string[] = [];
  selectedNotifications: string[] = [];
  selectedAlertDestinations: string[] = [];
  selectedLanguages: LanguageDm[] = [];
  languages: LanguageDm[] = [];
  selectedKeywordLanguage: LanguageDm | undefined;
  keywordMode: string = 'singleLang'; //"multiLang";
  produceReport: boolean = false;
  reportType: string | undefined;
  selectedAlerts: string[] = [];
  exportFormats = {
    pdf: true,
    word: false,
    csv: false,
  };
  saveTranscripts = false;
  translateTranscripts = false;
  saveClosedCaptions = false;
  addRatings = false;
  createAutoEPG = false;
  identifyChildrenContent = false;
  azranCheckbox = false;
  identifyAdsVsPrograms = false;
  isTranslationSupported = false;
  isTranscriptionSupported = false;
  isFaceDetectionSupported = true;
  isLicensePlateDetectionSupported = true;
  enableFaceDetection = false;
  enableLicensePlateDetection = false;
  availableNotifications: NotificationDefinition[] = [];
  reportType: string;
  enabledKeywordsDetection: boolean;
  repeatForm: any;
  repeatWeekDays: WeekDays[];
  repeatMode: string;
  isTranscriptionSupported: boolean;
  isTranslationSupported: boolean;
  isFaceDetectionSupported: boolean;
  isLicensePlateDetectionSupported: boolean;
  userSelectedTimeRange: TimeRange;
  enabledLanguageMismatch: boolean;
  selectedKeywordLanguage: any;
  selectedAlertDestinations: any;
  DaysMap: any;
  searchTerms: string;
  saveClosedCaptions: any;
  enableFaceDetection: any;
  enableLicensePlateDetection: any;
  selectedNotifications: string[];
  selectedLanguages: any;
  timeRange_: any;
  */
