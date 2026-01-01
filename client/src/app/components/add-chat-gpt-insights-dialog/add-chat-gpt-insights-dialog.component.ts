import { ChangeDetectorRef, Component, Inject, TemplateRef, ViewChild } from '@angular/core';
import { Clip, Insight, InsightRequest, InsightType, ChatGptInsight } from '../../models/models';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSelect, MatSelectChange, MatOption } from '@angular/material/select';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialog } from '@angular/material/dialog';
import { ClipService } from '../../services/clip.service';
import { MaterialModule } from '../../shared/material.module';
import { marked } from 'marked';

export interface DialogData {
  existingInsights: string[];
  isUserDefined?: boolean;
  aiClip?: Clip;
}

@Component({
  selector: 'app-add-chat-gpt-insights-dialog.component',
  imports: [CommonModule, FormsModule, MaterialModule],
  templateUrl: './add-chat-gpt-insights-dialog.component.html',
  styleUrl: './add-chat-gpt-insights-dialog.component.scss',
})
export class AddChatGptInsightsDialogComponent {
  hasRun: any;
  selectedInsights: any;
  selectedLanguages: any;

  queryType: any;
  isTestEnabled: any;
  insightName: any;
  availableInsightTypeDefinitions: any;
  languages: any;
  isDirty: any;

  @ViewChild('dialogTemplate') dialogTemplate: TemplateRef<any> | undefined;
  helpDialogRef?: MatDialogRef<any>;
  snackBar: any;

  insightResult: ChatGptInsight | null = null; // <-- store the result to show
  insightProcessing = false;
  insightProcessingDone = false;
  userDefinedPrompt = '';

  constructor(
    public dialogRef: MatDialogRef<AddChatGptInsightsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private dialog: MatDialog,
    private clipService: ClipService,
    private cdr: ChangeDetectorRef
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
    if (!this.data.aiClip || !this.userDefinedPrompt) return;

    this.hasRun = true;
    this.insightProcessing = true;
    this.insightProcessingDone = false;
    this.insightResult = null;

    const chatGptInsight: InsightRequest = {
      insightType: InsightType.ChatGPTPrompt,
      PromptText: this.userDefinedPrompt,
    };

    this.clipService.addInsights(this.data.aiClip.id!, [chatGptInsight]).subscribe({
      next: (updatedClip) => {
        // Find the new insight
        const newInsight = updatedClip.insights?.find(
          (i) =>
            i.insightType === InsightType.ChatGPTPrompt && i.promptText === this.userDefinedPrompt // match the prompt text you sent
        );
        if (newInsight && 'result' in newInsight) {
          this.insightResult = newInsight as ChatGptInsight;
        }
        this.insightProcessing = false;
        this.insightProcessingDone = true;
        this.cdr.detectChanges();
      },
      error: () => {
        this.insightProcessing = false;
        this.insightProcessingDone = false;
        this.snackBar.open('Failed to add insight. Please try again.', 'Close', { duration: 4000 });
      },
    });
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
    this.dialogRef.close();
  }
  onAddInsights() {
    throw new Error('Method not implemented.');
  }
  convertMdToHtml(insightContent: string) {
    const html = marked.parse(insightContent);
    return html;
  }
}
