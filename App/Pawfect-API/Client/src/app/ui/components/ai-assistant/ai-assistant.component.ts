import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import {
  Subject,
  takeUntil,
  debounceTime,
  distinctUntilChanged,
  switchMap,
  of,
} from 'rxjs';
import { AiAssistantService } from '../../../services/ai-assistant.service';
import { AnimalService } from '../../../services/animal.service';
import { AuthService } from '../../../services/auth.service';
import { LogService } from '../../../common/services/log.service';
import {
  CompletionsRequest,
  CompletionsResponse,
  AiMessage,
} from '../../../models/ai-assistant/ai-assistant.model';
import { Animal } from '../../../models/animal/animal.model';
import { AnimalLookup } from '../../../lookup/animal-lookup';
import { QueryResult } from '../../../common/models/query-result';
import { Breed } from '../../../models/breed/breed.model';
import { AnimalType } from '../../../models/animal-type/animal-type.model';
import { File } from '../../../models/file/file.model';
import { nameof } from 'ts-simple-nameof';
import { AiMessageRole } from 'src/app/common/enum/ai-message-role.enum';

@Component({
  selector: 'app-ai-assistant',
  templateUrl: './ai-assistant.component.html',
  styleUrls: ['./ai-assistant.component.css'],
})
export class AiAssistantComponent implements OnInit, OnDestroy {
  @ViewChild('conversationContainer', { static: false }) conversationContainer!: ElementRef;
  
  private destroy$ = new Subject<void>();
  private typewriterInterval: any;

  isOpen = false;
  isLoading = false;
  showAnimalSearch = false;
  isLoggedIn = false;
  isShelter = false;
  isTyping = false;
  hasError = false;

  messageControl = new FormControl('');
  animalSearchControl = new FormControl('');

  currentResponse = '';
  displayedResponse = '';
  conversationHistory: AiMessage[] = [];
  selectedAnimal: Animal | null = null;
  animalSearchResults: Animal[] = [];
  isSearchingAnimals = false;

  AiMessageRole = AiMessageRole;

  constructor(
    private aiAssistantService: AiAssistantService,
    private animalService: AnimalService,
    private authService: AuthService,
    private logService: LogService
  ) {}

  ngOnInit() {
    this.setupAnimalSearch();
    this.setupAuthState();
  }

  private setupAuthState() {
    // Check initial auth state synchronously
    this.isLoggedIn = this.authService.isLoggedInSync();
    // Subscribe to auth state changes
    this.authService
      .isLoggedIn()
      .pipe(takeUntil(this.destroy$))
      .subscribe((isLoggedIn) => {
        this.isLoggedIn = isLoggedIn;
        this.isShelter = !!this.authService.getUserShelterId();
        if ((!isLoggedIn || this.isShelter) && this.isOpen) {
          this.isOpen = false;
          this.resetConversation();
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.typewriterInterval) {
      clearInterval(this.typewriterInterval);
    }
  }

  private setupAnimalSearch() {
    this.animalSearchControl.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((query) => {
          if (!query || query.length < 2) {
            this.animalSearchResults = [];
            return of(null);
          }

          this.isSearchingAnimals = true;
          const lookup: AnimalLookup = {
            offset: 0,
            pageSize: 20,
            query: query,
            fields: [
              nameof<Animal>((x) => x.name),
              nameof<Animal>((x) => x.id),
              nameof<Animal>((x) => x.age),
              nameof<Animal>((x) => x.gender),
              [
                nameof<Animal>((x) => x.breed),
                nameof<Breed>((x) => x.name),
              ].join('.'),
              [
                nameof<Animal>((x) => x.animalType),
                nameof<AnimalType>((x) => x.name),
              ].join('.'),
              [
                nameof<Animal>((x) => x.attachedPhotos),
                nameof<File>((x) => x.sourceUrl),
              ].join('.'),
            ],
            sortBy: [nameof<Animal>((x) => x.name)],
            useSemanticSearch: true,
            useVectorSearch: false,
          };

          return this.animalService.query(lookup);
        })
      )
      .subscribe({
        next: (result: QueryResult<Animal> | null) => {
          this.isSearchingAnimals = false;
          if (result) {
            this.animalSearchResults = result.items || [];
          }
        },
        error: (error) => {
          this.isSearchingAnimals = false;
          this.animalSearchResults = [];
          this.logService.logFormatted({
            message: 'Error searching animals',
            error,
          });
        },
      });
  }

  toggleAssistant() {
    this.isOpen = !this.isOpen;
    if (!this.isOpen) {
      this.resetConversation();
    }
  }

  toggleAnimalSearch() {
    this.showAnimalSearch = !this.showAnimalSearch;
    if (!this.showAnimalSearch) {
      this.animalSearchControl.setValue('');
      this.animalSearchResults = [];
      this.selectedAnimal = null;
    }
  }

  selectAnimal(animal: Animal) {
    this.selectedAnimal = animal;
    this.showAnimalSearch = false;
    this.animalSearchControl.setValue('');
    this.animalSearchResults = [];
    this.currentResponse = '';
  }

  sendMessage() {
    const message = this.messageControl.value?.trim();
    if (!message || this.isLoading || this.isTyping) return;

    // Clear error state when sending a new message
    this.hasError = false;
    this.isLoading = true;

    // Store the current message and add user message to conversation immediately
    const currentUserMessage = message;
    const userMessage: AiMessage = {
      role: AiMessageRole.User,
      content: currentUserMessage,
    };
    this.conversationHistory.push(userMessage);
    this.messageControl.setValue('');

    // Scroll to bottom when starting to send message
    this.scrollToBottom();

    const request: CompletionsRequest = {
      prompt: message,
      contextAnimalId: this.selectedAnimal?.id,
      conversationHistory: this.conversationHistory, // Only send completed conversation history
    };

    this.aiAssistantService
      .completions(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: CompletionsResponse) => {
          this.isLoading = false;
          this.currentResponse = response.response;

          // Add AI response to history (user message already added)
          const aiMessage: AiMessage = {
            role: AiMessageRole.Assistant,
            content: response.response,
          };
          this.conversationHistory.push(aiMessage);

          // Start typewriter effect
          this.startTypewriterEffect(response.response);
          
          // Scroll to bottom after adding messages
          this.scrollToBottom();
        },
        error: (error) => {
          this.isLoading = false;
          this.hasError = true;
          this.currentResponse =
            '<p class="text-red-500 font-medium">Sorry, I\'m having trouble right now. Please try again later.</p>';
          this.displayedResponse = this.currentResponse;

          // Remove the user message from history on error
          if (this.conversationHistory.length > 0 && 
              this.conversationHistory[this.conversationHistory.length - 1].role === AiMessageRole.User) {
            this.conversationHistory.pop();
          }

          this.logService.logFormatted({
            message: 'Error getting AI completion',
            error,
          });
          
          // Scroll to bottom even on error
          this.scrollToBottom();
        },
      });
  }

  private startTypewriterEffect(fullText: string) {
    this.displayedResponse = '';
    this.isTyping = true;
    let currentIndex = 0;

    // Clear any existing interval
    if (this.typewriterInterval) {
      clearInterval(this.typewriterInterval);
    }

    // Create a temporary div to parse HTML and get plain text for character counting
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = fullText;
    const textContent = tempDiv.textContent || tempDiv.innerText || '';

    this.typewriterInterval = setInterval(() => {
      if (currentIndex < textContent.length) {
        // Calculate how much of the HTML to show based on character progress
        const progress = (currentIndex + 1) / textContent.length;
        const htmlLength = fullText.length;
        const targetLength = Math.floor(progress * htmlLength);

        // Find a safe cut point that doesn't break HTML tags
        let cutPoint = targetLength;
        let openTags = 0;
        let inTag = false;

        for (let i = 0; i < Math.min(targetLength, fullText.length); i++) {
          if (fullText[i] === '<') {
            inTag = true;
            if (fullText[i + 1] !== '/') {
              openTags++;
            } else {
              openTags--;
            }
          } else if (fullText[i] === '>') {
            inTag = false;
          }
        }

        // If we're in the middle of a tag, extend to the end of the tag
        if (inTag) {
          while (cutPoint < fullText.length && fullText[cutPoint] !== '>') {
            cutPoint++;
          }
          cutPoint++; // Include the closing >
        }

        this.displayedResponse = fullText.substring(0, cutPoint);
        currentIndex++;
      } else {
        clearInterval(this.typewriterInterval);
        this.isTyping = false;
        this.displayedResponse = fullText;
      }
    }, 12); // Fast typing speed (20ms per character)
  }

  private resetConversation() {
    this.currentResponse = '';
    this.displayedResponse = '';
    this.conversationHistory = [];
    this.selectedAnimal = null;
    this.showAnimalSearch = false;
    this.hasError = false;
    this.isTyping = false;
    this.messageControl.setValue('');
    this.animalSearchControl.setValue('');
    this.animalSearchResults = [];

    // Clear typewriter interval
    if (this.typewriterInterval) {
      clearInterval(this.typewriterInterval);
    }
  }

  onKeyPress(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  skipTypewriter() {
    if (this.isTyping && this.typewriterInterval) {
      clearInterval(this.typewriterInterval);
      this.isTyping = false;
      this.displayedResponse = this.currentResponse;
    }
  }

  getAnimalPhotoUrl(animal: Animal): string {
    if (
      animal.attachedPhotos &&
      animal.attachedPhotos.length > 0 &&
      animal.attachedPhotos[0].sourceUrl
    ) {
      return animal.attachedPhotos[0].sourceUrl;
    }
    return 'assets/placeholder.jpg';
  }

  getAnimalDisplayInfo(animal: Animal): string {
    const parts: string[] = [];

    if (animal.breed?.name) {
      parts.push(animal.breed.name);
    }

    if (animal.age) {
      parts.push(`${animal.age} years old`);
    }

    if (animal.gender) {
      parts.push(animal.gender === 1 ? 'Male' : 'Female');
    }

    return parts.join(' • ');
  }

  private scrollToBottom(): void {
    if (this.conversationContainer) {
      setTimeout(() => {
        const element = this.conversationContainer.nativeElement;
        element.scrollTop = element.scrollHeight;
      }, 100);
    }
  }

  trackByAnimalId(index: number, animal: Animal): string {
    return animal.id || index.toString();
  }

  onImageError(event: Event) {
    const element = event.target as HTMLImageElement;
    element.src = 'assets/placeholder.jpg';
  }
}
