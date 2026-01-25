export interface Clip {
  id: string;
  title: string;
  duration: number; // in seconds
  thumbnailUrl: string;
  videoUrl: string;
  channelName: string;
  channelId: string;
  createdAt: Date;
  updatedAt: Date;
  tags: string[];
  broadcastStartTime: Date;
  broadcastEndTime: Date;
  selected?: boolean;

  insights: Insight[];

  // Additional fields for AI insights
  transcription?: string;
  sentiment?: SentimentData;
  keyMoments?: KeyMoment[];
  faces?: FaceDetection[];
  objects?: ObjectDetection[];
  alprData?: ALPRData[];
}

export interface Transcript {
  text: string;
  startInSeconds: number;
  endInSeconds: number;
  //UI fields
  // isActive?: boolean,
  // isPlaying?: boolean
}

// export interface Insight {
//   id: string;
//   insightType: string;
//   transcripts: Transcript[];
//   providerName: string;
//   audioLanguage: string;
// }

export interface BaseInsight {
  id: string;
  insightType: InsightType;
  providerName: string;
  createdAt: string;
}

export interface TranscriptionInsight extends BaseInsight {
  insightType: InsightType.Transcription;
  transcripts: Transcript[];
  audioLanguage: string;
}

export interface SummaryInsight extends BaseInsight {
  insightType: InsightType.Summary;
  summary: string;
}

export interface CelebrityInsight extends BaseInsight {
  insightType: InsightType.Celebrities;
  celebrities: any[];
}

export interface LocationInsight extends BaseInsight {
  insightType: InsightType.Locations;
  locations: any[];
}

export interface ChatGptInsight extends BaseInsight {
  promptText: string;
  insightType: InsightType.ChatGPTPrompt;
  name: string;
  prompt: string;
  result: string;
}

export type Insight =
  | TranscriptionInsight
  | SummaryInsight
  | CelebrityInsight
  | LocationInsight
  | ChatGptInsight;

export interface ClipTag {
  text: string;
  selected: boolean;
}

export interface TimeInterval {
  intervalType: 'all' | 'today' | 'week' | 'month' | 'custom';
  from?: Date;
  to?: Date;
}

export interface SentimentData {
  positive: number;
  neutral: number;
  negative: number;
  overallScore: number;
}

export interface KeyMoment {
  timestamp: number;
  description: string;
  importance: number;
}

export interface FaceDetection {
  timestamp: number;
  personName?: string;
  confidence: number;
  boundingBox: BoundingBox;
}

export interface ObjectDetection {
  timestamp: number;
  objectType: string;
  confidence: number;
  boundingBox: BoundingBox;
}

export interface ALPRData {
  timestamp: number;
  licensePlate: string;
  confidence: number;
  state?: string;
  boundingBox: BoundingBox;
}

export interface BoundingBox {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface ClipSearchRequest {
  searchTerm?: string;
  searchOperandAnd?: boolean;
  tags?: string[];
  channelIds?: string[];
  fromDate?: Date;
  toDate?: Date;
  sortOption?: number;
  limit?: number;
  offset?: number;
}

export interface ClipSearchResponse {
  clips: Clip[];
  totalCount: number;
  hasMore: boolean;
}

export interface CreateClipRequest {
  title: string;
  channelIds: string[];
  fromTime: Date;
  toTime: Date;
  tags?: string[];
  processAI?: boolean;
}

export interface InsightRequest {
  insightType: InsightType;
  PromptText?: string;
  PromptName?: string;
  language?: string;
  config?: any;
}

export enum InsightType {
  Transcription = 'Transcription',
  Translation = 'Translation',
  Summary = 'Summary',
  FaceDetection = 'FaceDetection',
  Celebrities = 'Celebrities',
  Locations = 'Locations',
  ObjectDetection = 'ObjectDetection',
  ALPR = 'ALPR',
  Sentiment = 'Sentiment',
  ChatGPTPrompt = 'ChatGPTPrompt',
}

export interface InsightTypeOption {
  type: InsightType;
  label: string;
  icon: string;
  description: string;
  requiresLanguage: boolean;
}

export interface Message {
  type: 'user' | 'assistant' | 'system';
  content: string;
  timestamp: Date;
  tokens?: number;
  cost?: string;
}

export interface SemanticSearchResponseDto {
  answer: string;
  evidence: EvidenceDto[];
}
export interface EvidenceDto {
  clipId: string;
  channelId: string;
  clipStartTime: Date;
  text: string;
  score: number;
}

export class ActEnums {
  constructor(public Value: string) {}
  public static getActEnumObjectByValue = (enum_obj: any, value: string) => {
    const key = Object.keys(enum_obj).find((key) => key === value);
    return key !== undefined ? enum_obj[key] : undefined;
  };
}

export class VisibilityEnum extends ActEnums {
  static Private = new VisibilityEnum('Private');
  static Everyone = new VisibilityEnum('Everyone');
  constructor(Value: string) {
    super(Value);
  }
}

export class RuleRecurrenceEnum extends ActEnums {
  static readonly Once: RuleRecurrenceEnum = new RuleRecurrenceEnum('Once');
  static readonly Recurring: RuleRecurrenceEnum = new RuleRecurrenceEnum('Recurring');
  static readonly Continuous: RuleRecurrenceEnum = new RuleRecurrenceEnum('Continuous');
  constructor(value: string) {
    super(value);
  }
}

export class Rule {
  Recurrence!: RuleRecurrenceEnum;
  Days: number = 0b1111111; // Represents each weekday in binary (Mon to Fri)
  RuleStart?: string;
  RuleEnd?: string;
}

type JobStatus = 'Pending' | 'In Progress' | 'Completed' | 'Failed' | 'Paused' | 'Stopped';

export interface ReportRunHistoryEntry {
  ReportResultId: string;
  ActualRunStartTime: Date;
  ActualRunEndTime: Date;
  ProcessDurationInMinutes?: number;
  Status: string;
  Error?: string;
  FileSizeBytes?: number;
  //UI fields
  DownloadUrl?: string;
  ErrorMessage?: string;
  ErrorDetails?: string;
  WasEmailSent?: boolean;
  Format?: 'pdf' | 'csv';
}

export class ChannelResultStatistics {
  KeywordDetectedAlertsSent?: number;
  KeywordsDetectionsFound?: string[];
  FaceDetectedAlertSent?: number;
  // FaceDetectionsFound?: string[];
  LogoDetectedAlertSent?: number;
  LogoDetectionsFound?: string[];
  Mp4FilesProcessed?: number;
  Mp3FilesCreated?: number;
  DistinctAudioLanguages?: string[];
  DistinctTranslatedLanguages?: string[];
  Errors?: string[];
}

export class ResultStatistics {
  ProcessDurationInMinutes?: number;
  ChannelStatistics?: { [channelId: string]: ChannelResultStatistics };
}

export class RunHistoryEntry {
  ActualRunStartTime?: Date; // Represents the start time of the run
  ActualRunEndTime?: Date; // Represents the end time of the run
  BroadcastStartTime?: Date;
  BroadcastEndTime?: Date;
  Statistics?: ResultStatistics; // The statistics object for the run
  errors?: number;
}

export interface Channel {
  id: number;
  displayName: string;
  recordingRoot: string;
  liveRecordingRoot: string;
  physicalName: string;
  liveThumbnailUrl: string;
  serverId: number;
  serverName: string;
  backupServerName: string;
  groupId: number;
  position_in_group: number;
  logoUrl: string;
  color: string;
  loudnessEnabled: boolean;
  description: string;
  player?: any;
  thumb?: string;
  teletextLng: string[];
  subtitleLng: string[];
  audioLng: string[];
  audioOnly: boolean;
  framesRoot?: string;
  framesize?: string;
  bitrate?: string;
  ts_id?: number;
  license?: string[];
  sibling_channel_ids?: number[];
  category?: any;
}

export class AiJobRequest {
  Id?: string;
  Name!: string;
  ChannelIds?: number[];
  Visibility?: VisibilityEnum;
  Operations?: string[];
  BroadcastStartTime?: Date | null;
  BroadcastEndTime?: Date | null;
  NotificationIds!: string[];
  Status?: JobStatus;
  Error?: string;
  NextScheduledTime?: Date | undefined;
  RequestRule?: Rule;
  TranslationLanguages?: string[];
  CreatedAt?: Date | null;
  CreatedBy?: string;
  Keywords?: string[];
  KeywordsLangauges?: string[];
  RunHistory: RunHistoryEntry[] = [];
}

export class AiJobRequestX extends AiJobRequest {
  channels: Channel[] = [];
  notifications: string[] = [];
  channelsStr: string = '';
  keywordsStr: string = '';
  operationsStr: string = '';
  notificationsStr: string = '';
  errors: number = 0;
}

export class JobRequestFilter {
  Start?: Date;
  End?: Date;
  ChannelIds?: number[];
  Operation?: string;
  //Text?: string;
  //AiJobRequestId?: string;
  //Keywords?: string[];
  SearchTerm?: string;
  SortDirection?: number;
}

export class NotificationWebhookSetup {
  // Define NotificationWebhookSetup properties
}

class ACL {
  // Define ACL properties
}
export class NotificationDefinition {
  Id?: string;
  Name?: string;
  ToEmails?: string;
  SendSNMP?: boolean;
  WebhookSetup?: NotificationWebhookSetup[];
  Acl?: ACL;
  Visibility: VisibilityEnum = VisibilityEnum.Private;
  LastUpdatedBy?: string;
  LastUpdateDate?: string;
  CreatedDate: Date = new Date();
  MaxAccessRight?: string;
}

export type IntervalType = 'all' | 'today' | 'lweek' | 'l2week' | 'lmonth' | 'custom';

export interface Interval {
  intervalType: IntervalType;
  from?: Date | null;
  to?: Date | null;
}

export class UITag {
  text?: string;
  selected?: boolean;
}

export class LanguageDm {
  EnglishName?: string;
  DisplayName: string = '';
  ProvidersLanguageCode?: { [providerId: string]: string } = {};
  isTranslated?: boolean;
}

export class TimeRange {
  private readonly _from: Date;
  get from() {
    return this._from;
  }

  private readonly _to: Date;
  get to() {
    return this._to;
  }

  constructor(from: Date, to: Date) {
    this._from = from;
    this._to = to;
  }

  toString(): string {
    return `${this.from} - ${this.to}`;
  }

  // duration in mSecs
  durationMSecs(): number {
    return this.to.getTime() - this.from.getTime();
  }

  // duration in secs
  durationSecs(): number {
    return Math.round(this.durationMSecs() / 1_000);
  }

  isInRange(d: Date): boolean {
    return d.getTime() >= this.from.getTime() && d.getTime() <= this.to.getTime();
  }
}

export class ProviderDm {
    ProviderInternalId?: string;
    InsightTypes: string[] = [];
    DisplayName?: string;
    Description?: string;
    LastUpdatedBy?: string;
    LastUpdateDate: Date;
    Cost: number;
    constructor() {
        this.LastUpdateDate = new Date();
        this.Cost = 0;
    }
}

export interface InsightTypeDefinition {
  Name: string;
  DisplayName: string;
  SourceInsightType?: string;
  InsightProviders: ProviderDm[];
  Prompt?: string;
  IsCustom?: boolean;
}
