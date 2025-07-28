import {
  Component,
  Output,
  EventEmitter,
  Input,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleAuthService } from 'src/app/services/google-auth.service';

@Component({
  selector: 'app-google-login-button',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      type="button"
      (click)="onClick()"
      [disabled]="isLoading"
      [attr.aria-busy]="isLoading"
      class="w-full flex items-center justify-center px-4 py-2.5 border border-gray-300 rounded-md
             shadow-sm bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 
             focus:ring-gray-500 transition-all duration-300 transform hover:-translate-y-0.5 hover:shadow-md 
             active:bg-gray-100 group disabled:opacity-70 disabled:hover:transform-none"
    >
      <div *ngIf="isLoading" class="mr-2">
        <svg
          class="animate-spin h-5 w-5 text-gray-500"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            class="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            stroke-width="4"
          ></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          ></path>
        </svg>
      </div>
      <img
        *ngIf="!isLoading"
        src="https://www.gstatic.com/firebasejs/ui/2.0.0/images/auth/google.svg"
        alt="Google logo"
        class="w-5 h-5 mr-2 transition-transform duration-300 group-hover:scale-110"
      />
      <span class="text-gray-700 font-medium">{{ text }}</span>
    </button>
  `,
})
export class GoogleLoginButtonComponent {
  @Input() text: string = 'Συνέχεια με Google';
  @Input() isLoading: boolean = false;
  @Input() isSignup: boolean = false;

  constructor(private readonly googleAuthService: GoogleAuthService) {}

  onClick(): void {
    if (!this.isLoading) {
      const authUrl = this.googleAuthService.getAuthUrl(this.isSignup);
      window.location.href = authUrl;
    }
  }
}
