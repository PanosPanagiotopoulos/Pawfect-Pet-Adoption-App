import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-google-signup-loading',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <div
      *ngIf="isLoading"
      class="absolute inset-0 z-50 bg-gray-900/50 backdrop-blur-sm flex items-center justify-center"
    >
      <div
        class="bg-white/5 backdrop-blur-lg rounded-2xl shadow-xl p-8 space-y-6 border border-white/10 text-center"
      >
        <div
          class="w-16 h-16 mx-auto border-4 border-primary-500 border-t-transparent rounded-full animate-spin"
        ></div>
        <p class="text-gray-400">{{ 'APP.GOOGLE_SIGNUP.LOADING_MESSAGE' | translate }}</p>
      </div>
    </div>
  `,
})
export class GoogleSignupLoadingComponent {
  @Input() isLoading = false;
}
