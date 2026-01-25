import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Message, SemanticSearchResponseDto } from '../../models/models';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '../../shared/material.module';
import { AiService } from '../../services/ai.service';
import { marked } from 'marked';

@Component({
  selector: 'app-transcript-chat.component',
  imports: [CommonModule, FormsModule, MaterialModule],
  templateUrl: './transcript-chat.component.html',
  styleUrl: './transcript-chat.component.scss',
})
export class TranscriptChatComponent implements OnInit {
  messages: Message[] = [];
  input: string = '';
  isLoading: boolean = false;
  showHelp: boolean = false;
  tokenEstimate: number = 0;
  costEstimate: string = '0.0000';

  exampleQueries: string[] = [
    'What topics were discussed in the last meeting?',
    "Find all mentions of 'budget' in Q3 transcripts",
    "Summarize the key decisions from today's call",
    'Who spoke the most in the engineering meeting?',
  ];

  userQueries: string[] = [
    'Search for keywords or phrases xxxxxxxxxxxxxxx',
    'Find speaker statistics',
    'Extract key topics',
    'Date range queries',
    'Sentiment analysis xxxxxxxxxxxxxxxxxxxxxxxxxxx',
  ];

  constructor(private clipService: AiService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.messages.push({
      type: 'system',
      content: 'Welcome to Transcript Query Agent! Ask me anything about your transcripts.',
      timestamp: new Date(),
    });
  }

  onInputChange(): void {
    this.tokenEstimate = this.estimateTokens(this.input);
    this.costEstimate = this.calculateCost(this.tokenEstimate);
  }

  estimateTokens(text: string): number {
    // Rough estimate: ~4 characters per token
    return Math.ceil(text.length / 4);
  }

  calculateCost(tokens: number): string {
    // Example pricing: $0.015 per 1K tokens
    return ((tokens / 1000) * 0.015).toFixed(4);
  }

  sendMessage(): void {
    if (!this.input.trim() || this.isLoading) {
      return;
    }

    const userMessage: Message = {
      type: 'user',
      content: this.input,
      timestamp: new Date(),
      tokens: this.tokenEstimate,
      cost: this.costEstimate,
    };

    this.messages.push(userMessage);

    const query = this.input;

    // reset input UI
    this.input = '';
    this.tokenEstimate = 0;
    this.costEstimate = '0.0000';
    this.isLoading = true;

    this.clipService.query(query).subscribe({
      next: (result: SemanticSearchResponseDto) => {
        const aiMessage: Message = {
          type: 'assistant',
          content: result.answer ?? 'No answer returned.',
          timestamp: new Date(),
        };

        this.messages.push(aiMessage);

        // OPTIONAL: show evidences
        // if (result.evidence?.length) {
        //   result.evidence.forEach((ev) => {
        //     this.messages.push({
        //       type: 'assistant',
        //       content: `📌 ${ev.channelId} @ ${new Date(ev.clipStartTime!).toLocaleString()}\n${
        //         ev.text
        //       }`,
        //       timestamp: new Date(),
        //     });
        //   });
        // }

        this.isLoading = false;
        this.scrollToBottom();
        this.cdr.detectChanges();
      },

      error: (err) => {
        console.error('Transcript query failed', err);

        this.messages.push({
          type: 'assistant',
          content: '❌ Failed to query transcripts. Please try again.',
          timestamp: new Date(),
        });

        this.isLoading = false;
        this.scrollToBottom();
        this.cdr.detectChanges();
      },
    });
  }

  // sendMessage(): void {
  //   if (!this.input.trim() || this.isLoading) {
  //     return;
  //   }

  //   const userMessage: Message = {
  //     type: 'user',
  //     content: this.input,
  //     timestamp: new Date(),
  //     tokens: this.tokenEstimate,
  //     cost: this.costEstimate,
  //   };

  //   this.messages.push(userMessage);
  //   const query = this.input;
  //   this.input = '';
  //   this.tokenEstimate = 0;
  //   this.costEstimate = '0.0000';
  //   this.isLoading = true;

  //   this.clipService.query(query).subscribe((result: SemanticSearchResponseDto) => {});

  //   // Simulate AI response
  //   setTimeout(() => {
  //     const aiMessage: Message = {
  //       type: 'assistant',
  //       content: 'This is a mock response. Replace this with your actual C# agent API call.',
  //       timestamp: new Date(),
  //     };
  //     this.messages.push(aiMessage);
  //     this.isLoading = false;

  //     // Auto-scroll to bottom
  //     this.scrollToBottom();
  //   }, 2000);
  // }

  useExampleQuery(query: string): void {
    this.input = query;
    this.onInputChange();
  }

  useUserQuery(query: string): void {
    this.input = query;
    this.onInputChange();
  }

  toggleHelp(): void {
    this.showHelp = !this.showHelp;
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      const container = document.querySelector('.messages-container');
      if (container) {
        container.scrollTop = container.scrollHeight;
      }
    }, 100);
  }

  convertMdToHtml(answer: string) {
    const html = marked.parse(answer);
    return html;
  }
}
