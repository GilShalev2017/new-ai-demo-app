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

export type Insight =
  | TranscriptionInsight
  | SummaryInsight
  | CelebrityInsight
  | LocationInsight;


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
  PromptName?:string;
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
  ChatGPTPrompt = 'ChatGPTPrompt'
}

export interface InsightTypeOption {
  type: InsightType;
  label: string;
  icon: string;
  description: string;
  requiresLanguage: boolean;
}