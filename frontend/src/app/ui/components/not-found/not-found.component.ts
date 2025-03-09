import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIconsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-900 relative overflow-hidden">
      <!-- Background elements -->
      <div class="fixed inset-0 z-0">
        <div class="absolute inset-0 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900"></div>
        <div class="absolute inset-0 bg-gradient-to-br from-primary-900/20 via-secondary-900/20 to-accent-900/20 animate-gradient"></div>
        <div class="absolute inset-0 bg-gradient-radial from-transparent via-primary-900/10 to-transparent"></div>
      </div>

      <!-- Content -->
      <div class="relative z-10 text-center px-4">
        <!-- 404 Text -->
        <h1 class="text-[150px] sm:text-[200px] font-bold leading-none bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 bg-clip-text text-transparent animate-gradient">
          404
        </h1>

        <!-- Cute Animal Emoji -->
        <div class="text-[50px] sm:text-[70px] mb-8 animate-bounce">
          🐾
        </div>

        <!-- Message -->
        <h2 class="text-2xl sm:text-3xl font-semibold text-white mb-4">
          Ωχ! Χάθηκε το κατοικίδιο...
        </h2>
        <p class="text-gray-400 text-lg mb-8 max-w-md mx-auto">
          Η σελίδα που ψάχνετε έχει χαθεί στο διαδίκτυο. Ας επιστρέψουμε στην αρχική!
        </p>

        <!-- Home Button -->
        <a
          routerLink="/"
          class="inline-flex items-center px-6 py-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1"
        >
          <span class="mr-2">🏠</span>
          Επιστροφή στην Αρχική
        </a>
      </div>

      <!-- Decorative Elements -->
      <div class="absolute -top-20 -left-20 w-60 h-60 bg-gradient-to-br from-primary-500/10 to-transparent rounded-full blur-3xl animate-float-1"></div>
      <div class="absolute -bottom-20 -right-20 w-60 h-60 bg-gradient-to-br from-accent-500/10 to-transparent rounded-full blur-3xl animate-float-2"></div>
    </div>
  `
})
export class NotFoundComponent {}