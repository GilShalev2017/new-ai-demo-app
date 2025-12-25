import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '../../shared/material.module';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import {
  InsightType,
  InsightTypeOption,
  InsightRequest,
  Clip,
  Insight
} from '../../models/models';

export interface AddInsightsDialogData {
  clip: Clip;
}

@Component({
  selector: 'app-add-insights-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MaterialModule],
  templateUrl: './add-insights-dialog.component.html',
  styleUrls: ['./add-insights-dialog.component.scss'],
})
export class AddInsightsDialogComponent implements OnInit {
  // All possible insight types
  allInsightTypes: InsightTypeOption[] = [
    {
      type: InsightType.Transcription,
      label: 'Transcription',
      icon: 'description',
      description: 'Convert speech to text with timestamps',
      requiresLanguage: true,
    },
    {
      type: InsightType.Translation,
      label: 'Translation',
      icon: 'translate',
      description: 'Translate to another language',
      requiresLanguage: true,
    },
    {
      type: InsightType.Summary,
      label: 'Summary',
      icon: 'summarize',
      description: 'AI-generated content summary',
      requiresLanguage: true,
    },
    {
      type: InsightType.FaceDetection,
      label: 'Face Detection',
      icon: 'face',
      description: 'Detect and identify faces',
      requiresLanguage: false,
    },
    {
      type: InsightType.Celebrities,
      label: 'Celebrities',
      icon: 'star',
      description: 'Identify public figures',
      requiresLanguage: false,
    },
    {
      type: InsightType.Locations,
      label: 'Locations',
      icon: 'location_on',
      description: 'Extract location mentions',
      requiresLanguage: true,
    },
    {
      type: InsightType.ObjectDetection,
      label: 'Object Detection',
      icon: 'category',
      description: 'Detect objects and scenes',
      requiresLanguage: false,
    },
    {
      type: InsightType.ALPR,
      label: 'License Plates',
      icon: 'local_shipping',
      description: 'License plate recognition',
      requiresLanguage: false,
    },
    {
      type: InsightType.Sentiment,
      label: 'Sentiment',
      icon: 'sentiment_satisfied',
      description: 'Analyze emotional tone',
      requiresLanguage: true,
    },
  ];

  // Filtered list shown to the user
  availableInsightTypes: InsightTypeOption[] = [];

  // Selected insights
  selectedInsights = new Map<InsightType, InsightRequest>();

  // Language options
  languages = [
    { code: 'en', name: 'English' },
    { code: 'es', name: 'Spanish' },
    { code: 'fr', name: 'French' },
    { code: 'de', name: 'German' },
    { code: 'he', name: 'Hebrew' },
    { code: 'ar', name: 'Arabic' },
  ];

  constructor(
    public dialogRef: MatDialogRef<AddInsightsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AddInsightsDialogData
  ) {}

  ngOnInit(): void {
    this.filterAvailableInsights();
  }

  private filterAvailableInsights(): void {
    const existingTypes = new Set<InsightType>(
      (this.data.clip.insights ?? []).map(
        (i: Insight) => i.insightType
      )
    );

    this.availableInsightTypes = this.allInsightTypes.filter(
      (option: InsightTypeOption) => !existingTypes.has(option.type)
    );
  }

  isSelected(type: InsightType): boolean {
    return this.selectedInsights.has(type);
  }

  toggleInsight(option: InsightTypeOption): void {
    if (this.selectedInsights.has(option.type)) {
      this.selectedInsights.delete(option.type);
    } else {
      const request: InsightRequest = {
        insightType: option.type,
        language: option.requiresLanguage ? 'en' : undefined,
      };
      this.selectedInsights.set(option.type, request);
    }
  }

  getInsightRequest(type: InsightType): InsightRequest | undefined {
    return this.selectedInsights.get(type);
  }

  updateLanguage(type: InsightType, language: string): void {
    const request = this.selectedInsights.get(type);
    if (request) {
      request.language = language;
    }
  }

  hasSelectedInsights(): boolean {
    return this.selectedInsights.size > 0;
  }

  getSelectedInsightsArray(): InsightRequest[] {
    return Array.from(this.selectedInsights.values());
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.hasSelectedInsights()) {
      this.dialogRef.close(this.getSelectedInsightsArray());
    }
  }
}
