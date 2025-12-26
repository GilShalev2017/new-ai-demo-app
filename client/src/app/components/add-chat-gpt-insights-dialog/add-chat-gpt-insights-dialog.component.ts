import { Component, Inject, TemplateRef, ViewChild } from '@angular/core';
import { Clip, InsightRequest, InsightType } from '../../models/models';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelect, MatSelectChange, MatOption } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  MatDialogContent,
  MatDialogActions,
  MAT_DIALOG_DATA,
  MatDialogRef,
  MatDialog,
} from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { SnackbarService } from '../../services/snackbar-service';
import { ClipService } from '../../services/clip.service';

export interface DialogData {
  existingInsights: string[];
  isUserDefined?: boolean;
  aiClip?: Clip;
}

@Component({
  selector: 'app-add-chat-gpt-insights-dialog.component',
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelect,
    MatOption,
    MatTooltipModule,
    MatDialogContent,
    MatDialogActions,
    MatIcon,
  ],
  templateUrl: './add-chat-gpt-insights-dialog.component.html',
  styleUrl: './add-chat-gpt-insights-dialog.component.scss',
})
export class AddChatGptInsightsDialogComponent {
  hasRun: any;
  selectedInsights: any;
  selectedLanguages: any;
  userDefinedPrompt: any;
  insightProcessingDone: any;
  queryType: any;
  isTestEnabled: any;
  insightName: any;
  availableInsightTypeDefinitions: any;
  languages: any;
  isDirty: any;

  @ViewChild('dialogTemplate') dialogTemplate: TemplateRef<any> | undefined;
  helpDialogRef?: MatDialogRef<any>;
  snackBar: any;

  constructor(
    public dialogRef: MatDialogRef<AddChatGptInsightsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private dialog: MatDialog,
    private snackbar: SnackbarService,
    private clipService: ClipService
  ) {}

  onSelectionChange($event: MatSelectChange) {
    throw new Error('Method not implemented.');
  }
  capitalizeFirstLetter(arg0: any): any {
    throw new Error('Method not implemented.');
  }
  onPromptChanged(event: Event) {
    const target = event.target as HTMLTextAreaElement;
    this.userDefinedPrompt = this.capitalizeFirstLetter(target.value);
  }

  openPromptInfoDialog() {
    if (!this.dialogTemplate) return;

    this.helpDialogRef = this.dialog.open(this.dialogTemplate, {
      width: '900px',
      maxWidth: '95vw',
    });
  }

  onCloseHelpDialog() {
    this.helpDialogRef?.close();
  }

  onClose() {
    this.dialogRef.close();
  }

  runInsight() {
    const chatGptInsight: InsightRequest = { insightType: InsightType.ChatGPTPrompt,PromptText:this.userDefinedPrompt };
    if (this.data.aiClip != null) {
      this.clipService.addInsights(this.data.aiClip.id!, [chatGptInsight]).subscribe({
        next: () => {
          // Reload clip after backend had time to finish
          setTimeout(() => {
            if (this.data.aiClip!.id!) {
              this.loadClip(this.clipId);
            }

            // ✅ stop spinners
            //this.clearInsightsProcessing(insights);
          }, 2000);
        },
        error: (error) => {
          console.error('Error adding insights:', error);

          // ❌ stop spinners on error
          //this.clearInsightsProcessing(insights);

          this.snackBar.open('Failed to add insights. Please try again.', 'Close', {
            duration: 4000,
          });
        },
      });
    }
  }
  loadClip(clipId: any) {
    throw new Error('Method not implemented.');
  }
  clipId(clipId: any) {
    throw new Error('Method not implemented.');
  }
  clearInsightsProcessing(insights: any) {
    throw new Error('Method not implemented.');
  }
  saveInsight() {
    throw new Error('Method not implemented.');
  }
  publishInsight() {
    throw new Error('Method not implemented.');
  }
  onCloseAddNewPrompt() {
    throw new Error('Method not implemented.');
  }
  onAddInsights() {
    throw new Error('Method not implemented.');
  }
}
