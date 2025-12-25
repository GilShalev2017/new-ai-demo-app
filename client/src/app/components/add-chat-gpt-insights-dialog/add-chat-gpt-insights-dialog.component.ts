import { Component, Inject, TemplateRef, ViewChild } from '@angular/core';
import { Clip } from '../../models/models';
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

  constructor(
    public dialogRef: MatDialogRef<AddChatGptInsightsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private dialog: MatDialog,
    private snackbar: SnackbarService
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
    throw new Error('Method not implemented.');
  }

  runInsight() {
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
