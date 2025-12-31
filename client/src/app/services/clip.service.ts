// ============================================
// FILE: services/clip.service.ts
// ============================================
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  ALPRData,
  Clip,
  ClipSearchRequest,
  ClipSearchResponse,
  CreateClipRequest,
  FaceDetection,
  InsightRequest,
  ObjectDetection,
  SentimentData,
  TranscriptQueryResponse,
} from '../models/models';

@Injectable({
  providedIn: 'root',
})
export class ClipService {
  /**
   * Used ONLY for static/media files (thumbnails, videos)
   * API calls go through Angular proxy (/api → https://localhost:7176)
   */
  private readonly serverUrl = 'https://localhost:7176';

  /** API base (proxied) */
  private readonly clipsApi = '/api/clips';

  constructor(private http: HttpClient) {}

  // ============================================
  // CRUD Operations
  // ============================================

  /**
   * GET: /api/clips
   */
  getClips(request?: ClipSearchRequest): Observable<ClipSearchResponse> {
    const params = this.buildSearchParams(request);

    return this.http.get<ClipSearchResponse>(this.clipsApi, { params }).pipe(
      map((response) => ({
        ...response,
        clips: response.clips.map((clip) => ({
          ...clip,
          createdAt: new Date(clip.createdAt),
        })),
      }))
    );
  }

  /**
   * GET: /api/clips/{id}
   */
  getClipById(clipId: string): Observable<Clip> {
    return this.http.get<Clip>(`${this.clipsApi}/${clipId}`).pipe(
      map((clip) => ({
        ...clip,
        createdAt: new Date(clip.createdAt),
      }))
    );
  }

  /**
   * POST: /api/clips
   */
  createClip(request: CreateClipRequest): Observable<Clip> {
    return this.http.post<Clip>(this.clipsApi, request).pipe(
      map((clip) => ({
        ...clip,
        createdAt: new Date(clip.createdAt),
      }))
    );
  }

  /**
   * PUT: /api/clips/{id}
   */
  updateClip(clipId: string, updates: Partial<Clip>): Observable<Clip> {
    return this.http.put<Clip>(`${this.clipsApi}/${clipId}`, updates).pipe(
      map((clip) => ({
        ...clip,
        createdAt: new Date(clip.createdAt),
      }))
    );
  }

  /**
   * DELETE: /api/clips/{id}
   */
  deleteClip(clipId: string): Observable<void> {
    return this.http.delete<void>(`${this.clipsApi}/${clipId}`);
  }

  /**
   * POST: /api/clips/batch-delete
   */
  deleteMultipleClips(clipIds: string[]): Observable<{ deletedCount: number }> {
    return this.http.post<{ deletedCount: number }>(`${this.clipsApi}/batch-delete`, clipIds);
  }

  // ============================================
  // AI / Future Endpoints
  // ============================================

  getTranscription(clipId: string): Observable<string> {
    return this.http.get(`${this.clipsApi}/${clipId}/transcription`, { responseType: 'text' });
  }

  getSentiment(clipId: string): Observable<SentimentData> {
    return this.http.get<SentimentData>(`${this.clipsApi}/${clipId}/sentiment`);
  }

  getFaceDetections(clipId: string): Observable<FaceDetection[]> {
    return this.http.get<FaceDetection[]>(`${this.clipsApi}/${clipId}/faces`);
  }

  getObjectDetections(clipId: string): Observable<ObjectDetection[]> {
    return this.http.get<ObjectDetection[]>(`${this.clipsApi}/${clipId}/objects`);
  }

  getALPRData(clipId: string): Observable<ALPRData[]> {
    return this.http.get<ALPRData[]>(`${this.clipsApi}/${clipId}/alpr`);
  }

  processAI(clipId: string, features?: string[]): Observable<void> {
    return this.http.post<void>(`${this.clipsApi}/${clipId}/process-ai`, { features });
  }

  // ============================================
  // Helpers
  // ============================================

  private buildSearchParams(request?: ClipSearchRequest): HttpParams {
    let params = new HttpParams();

    if (!request) return params;

    if (request.searchTerm) params = params.set('searchTerm', request.searchTerm);

    if (request.searchOperandAnd !== undefined)
      params = params.set('searchOperandAnd', request.searchOperandAnd.toString());

    if (request.tags?.length) params = params.set('tags', request.tags.join(','));

    if (request.channelIds?.length) params = params.set('channelIds', request.channelIds.join(','));

    if (request.fromDate) params = params.set('fromDate', request.fromDate.toISOString());

    if (request.toDate) params = params.set('toDate', request.toDate.toISOString());

    if (request.sortOption !== undefined)
      params = params.set('sortOption', request.sortOption.toString());

    if (request.limit !== undefined) params = params.set('limit', request.limit.toString());

    if (request.offset !== undefined) params = params.set('skip', request.offset.toString()); // aligns with ASP.NET

    return params;
  }

  // ============================================
  // Media URL Helpers (NOT proxied)
  // ============================================

  getVideoUrl(relativePath: string): string {
    return `${this.serverUrl}/${relativePath}`;
  }

  getThumbnailUrl(relativePath: string): string {
    return `${this.serverUrl}/${relativePath}`;
  }

  removeInsights(clipId: string, insightIds: string[]): Observable<void> {
    return this.http.put<void>(`${this.clipsApi}/${clipId}/removeinsights`, insightIds);
  }

  addInsights(clipId: string, insights: InsightRequest[]): Observable<Clip> {
    return this.http.put<Clip>(`${this.clipsApi}/${clipId}/addinsights`, insights).pipe(
      map((clip) => ({
        ...clip,
        createdAt: new Date(clip.createdAt),
        updatedAt: new Date(clip.updatedAt),
      }))
    );
  }

  query(query: string): Observable<TranscriptQueryResponse> {
    return this.http.post<TranscriptQueryResponse>(`${this.clipsApi}/transcript-agent`, { query });
  }
}
