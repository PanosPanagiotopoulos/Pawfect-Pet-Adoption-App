import { Component, Output, EventEmitter, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-google-login-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      type="button"
      (click)="onClick()"
      class="w-full flex items-center justify-center px-4 py-2.5 border border-gray-300 rounded-md
             shadow-sm bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 
             focus:ring-gray-500 transition-all duration-300 transform hover:-translate-y-0.5 hover:shadow-md 
             active:bg-gray-100 group"
    >
      <img
        src="https://www.gstatic.com/firebasejs/ui/2.0.0/images/auth/google.svg"
        alt="Google logo"
        class="w-5 h-5 mr-2 transition-transform duration-300 group-hover:scale-110"
      />
      <span class="text-gray-700 font-medium">{{ text }}</span>
    </button>
  `
})
export class GoogleLoginButtonComponent {
  @Input() text: string = 'Continue with Google';
  @Output() login = new EventEmitter<void>();

  onClick(): void {
    this.login.emit();
  }
}