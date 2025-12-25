// ============================================
// FILE: ai-clips-gallery.component.ts
// ============================================
import {
  Component,
  OnInit,
  AfterViewInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '../../shared/material.module';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { ClipService } from '../../services/clip.service';
import { Clip, ClipTag, TimeInterval } from '../../models/models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-ai-clips-gallery',
  standalone: true,
  imports: [CommonModule, FormsModule, MaterialModule],
  templateUrl: './ai-clips-gallery.component.html',
  styleUrls: ['./ai-clips-gallery.component.scss'],
})
export class AiClipsGalleryComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('categoryContainer') categoryContainer!: ElementRef;

  // Search and filter properties
  searchTerm: string = '';
  searchPlaceholder: string = 'Search';
  isSearchExpanded: boolean = false;
  hasText: boolean = false;
  selectedSearch: 'ALL' | 'ANY' = 'ANY';
  searchOperandAnd: boolean = false;

  // Clips data
  clips: Clip[] = [];
  filteredClips: Clip[] = [];
  selectedClips: Clip[] = [];
  isAllClipsSelected: boolean = false;
  limitNumberOfClips: number = 100;

  // Tags/Categories
  availableTags: ClipTag[] = [
    { text: 'All Videos', selected: true },
    { text: 'News', selected: false },
    { text: 'Sports', selected: false },
    { text: 'Entertainment', selected: false },
    { text: 'Interviews', selected: false },
    { text: 'Politics', selected: false },
    { text: 'Technology', selected: false },
  ];
  selectedTags: string[] = [];

  // UI state
  isLowerToolbarHidden: boolean = false;
  welcomeScreenShown: boolean = false;
  isSearching: boolean = false;

  // Sort options
  selectedSortOption: number = 0; // 0=Newest, 1=Oldest, 2=A-Z, 3=Z-A, 4=Longest, 5=Shortest

  // Time filter
  timeInterval: TimeInterval = { intervalType: 'all' };

  // Selected channels
  selectedChannels: any[] = [];
  selectedChannelIds: string[] = [];

  // User permissions (2 = full access)
  userRight: number = 2;

  // RxJS subjects
  private searchTerms = new Subject<string>();
  private searchSubscription: any;

  constructor(
    private clipService: ClipService,
    private cdr: ChangeDetectorRef,
    private el: ElementRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Initialize search with debounce
    this.searchSubscription = this.searchTerms.pipe(debounceTime(700)).subscribe((term) => {
      this.performSearch(term);
    });

    // Load initial data
    this.loadClips();

    // Setup click listener for search expansion
    document.addEventListener('click', this.onDocumentClick.bind(this));
  }

  ngAfterViewInit(): void {
    this.cdr.detectChanges();
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
    document.removeEventListener('click', this.onDocumentClick.bind(this));
  }

  // ============================================
  // Data Loading Methods (Stubs for now)
  // ============================================

  loadClips(): void {
    this.isSearching = true;

    this.clipService.getClips().subscribe({
      next: (response) => {
        this.clips = response.clips; // Access the clips array from the response
        
        this.applyFilters();

        this.isSearching = false;
        this.welcomeScreenShown = response.clips.length === 0;
      },
      error: (error) => {
        console.error('Error loading clips:', error);
        this.isSearching = false;
      },
    });
  }

  // ============================================
  // Search Methods
  // ============================================

  onSearchClips(term: string): void {
    this.searchTerm = term;
    this.searchTerms.next(term);
    this.hasText = term.trim().length > 0;
  }

  performSearch(term: string): void {
    // TODO: Implement actual search logic with backend
    // For now, filter locally
    this.applyFilters();
  }

  onSearchClick(): void {
    this.isSearchExpanded = true;
    this.searchPlaceholder = 'Search in clip name and transcription';
  }

  onInput(): void {
    this.hasText = !!(this.searchTerm && this.searchTerm.trim() !== '');
  }

  onDocumentClick(event: MouseEvent): void {
    const searchDiv = this.el.nativeElement.querySelector('.search-div');
    if (!searchDiv) return;

    const isClickedInside = searchDiv.contains(event.target);
    if (!isClickedInside && !this.searchTerm) {
      this.isSearchExpanded = false;
      this.searchPlaceholder = 'Search';
    }
  }

  searchOperandChanged(): void {
    this.searchTerms.next(this.searchTerm);
  }

  // ============================================
  // Filter Methods
  // ============================================

  applyFilters(): void {
    let result = [...this.clips];

    // Apply search filter
    if (this.searchTerm.trim()) {
      const terms = this.searchTerm.toLowerCase().split(' ');
      result = result.filter((clip) => {
        const searchableText = `${clip.title} ${clip.channelName} ${clip.tags.join(
          ' '
        )}`.toLowerCase();

        if (this.searchOperandAnd) {
          // ALL words must match
          return terms.every((term) => searchableText.includes(term));
        } else {
          // ANY word can match
          return terms.some((term) => searchableText.includes(term));
        }
      });
    }

    // Apply tag filter
    if (this.selectedTags.length > 0 && !this.selectedTags.includes('All Videos')) {
      result = result.filter((clip) => clip.tags.some((tag) => this.selectedTags.includes(tag)));
    }

    // Apply channel filter
    if (this.selectedChannelIds.length > 0) {
      result = result.filter((clip) => this.selectedChannelIds.includes(clip.channelId));
    }

    // Apply time filter
    result = this.applyTimeFilter(result);

    // Apply sorting
    result = this.applySorting(result);

    // Apply limit
    this.filteredClips = result.slice(0, this.limitNumberOfClips);

    this.cdr.detectChanges();
  }

  applyTimeFilter(clips: Clip[]): Clip[] {
    if (this.timeInterval.intervalType === 'all') {
      return clips;
    }

    const now = new Date();
    let fromDate: Date;

    switch (this.timeInterval.intervalType) {
      case 'today':
        fromDate = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        break;
      case 'week':
        fromDate = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
        break;
      case 'month':
        fromDate = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
        break;
      case 'custom':
        fromDate = this.timeInterval.from || new Date(0);
        break;
      default:
        return clips;
    }

    return clips.filter((clip) => clip.createdAt >= fromDate);
  }

  applySorting(clips: Clip[]): Clip[] {
    const sorted = [...clips];

    // const getTime = (date: string | Date): number => {
    //   return date instanceof Date ? date.getTime() : new Date(date).getTime();
    // };

    switch (this.selectedSortOption) {
      case 0: // Newest
        sorted.sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime());
        break;
      case 1: // Oldest
        sorted.sort((a, b) => a.createdAt.getTime() - b.createdAt.getTime());
        break;
      case 2: // A-Z
        sorted.sort((a, b) => a.title.localeCompare(b.title));
        break;
      case 3: // Z-A
        sorted.sort((a, b) => b.title.localeCompare(a.title));
        break;
      case 4: // Longest
        sorted.sort((a, b) => b.duration - a.duration);
        break;
      case 5: // Shortest
        sorted.sort((a, b) => a.duration - b.duration);
        break;
    }

    return sorted;
  }

  onTimeFilterIntervalChanged(interval: TimeInterval): void {
    this.timeInterval = interval;
    this.searchTerms.next(this.searchTerm);
  }

  onChannelSelectionChanged(channels: any[]): void {
    this.selectedChannels = channels;
    this.selectedChannelIds = channels.map((ch) => ch.id);
    this.searchTerms.next(this.searchTerm);
  }

  dateChanged(): void {
    this.searchTerms.next(this.searchTerm);
  }

  // ============================================
  // Tag/Category Methods
  // ============================================

  toggleTagSelection(tag: ClipTag): void {
    tag.selected = !tag.selected;

    // Update selected tags array
    if (tag.selected) {
      if (!this.selectedTags.includes(tag.text)) {
        this.selectedTags.push(tag.text);
      }
    } else {
      this.selectedTags = this.selectedTags.filter((t) => t !== tag.text);
    }

    this.applyFilters();
  }

  unselectTag(tagText: string): void {
    const tag = this.availableTags.find((t) => t.text === tagText);
    if (tag) {
      tag.selected = false;
    }
    this.selectedTags = this.selectedTags.filter((t) => t !== tagText);
    this.applyFilters();
  }

  unselectAllTags(): void {
    this.availableTags.forEach((tag) => (tag.selected = false));
    this.selectedTags = [];
    this.applyFilters();
  }

  scrollCategories(direction: number): void {
    const container = this.categoryContainer.nativeElement;
    const scrollAmount = direction * 200;
    container.scrollTo({
      left: container.scrollLeft + scrollAmount,
      behavior: 'smooth',
    });
  }

  showLeftButton(): boolean {
    const container = this.categoryContainer?.nativeElement;
    return container?.scrollLeft > 0;
  }

  showRightButton(): boolean {
    const container = this.categoryContainer?.nativeElement;
    return container?.scrollLeft < container?.scrollWidth - container?.clientWidth;
  }

  // ============================================
  // Selection Methods
  // ============================================

  selectAllClips(selected: boolean): void {
    this.isAllClipsSelected = selected;
    this.selectedClips = [];

    this.filteredClips.forEach((clip) => {
      clip.selected = selected;
      if (selected) {
        this.selectedClips.push(clip);
      }
    });
  }

  toggleClipSelection(clip: Clip): void {
    clip.selected = !clip.selected;

    if (clip.selected) {
      this.selectedClips.push(clip);
    } else {
      this.selectedClips = this.selectedClips.filter((c) => c.id !== clip.id);
    }

    this.isAllClipsSelected = this.selectedClips.length === this.filteredClips.length;
  }

  deleteSelected(): void {
    // TODO: Show confirmation dialog
    if (confirm(`Are you sure you want to delete ${this.selectedClips.length} clips?`)) {
      // TODO: Call API to delete clips
      // this.clipService.deleteMultipleClips(this.selectedClips.map(c => c.id)).subscribe({
      //   next: () => {
      //     this.loadClips();
      //     this.selectedClips = [];
      //   },
      //   error: (error) => console.error('Error deleting clips:', error)
      // });

      // Mock deletion
      this.clips = this.clips.filter((clip) => !this.selectedClips.some((sc) => sc.id === clip.id));
      this.selectedClips = [];
      this.applyFilters();
    }
  }

  // ============================================
  // UI Methods
  // ============================================

  toggleLowerToolbars(): void {
    this.isLowerToolbarHidden = !this.isLowerToolbarHidden;
  }

  getGalleryHeight(): string {
    return this.isLowerToolbarHidden ? 'calc(100vh - 81px)' : 'calc(100vh - 245px)';
  }

  refreshAll(): void {
    this.loadClips();
  }

  addNewClip(): void {
    // TODO: Open dialog to add new clip
    console.log('Open add new clip dialog');
  }

  goToClip(clip: Clip): void {
    // TODO: Navigate to clip details
    this.router.navigateByUrl(`clips/${clip.id}`);
    console.log('Navigate to clip:', clip.id);
  }

  formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  getTimeAgo(date: Date): string {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffDays > 0) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    if (diffHours > 0) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffMins > 0) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
    return 'Just now';
  }
}
