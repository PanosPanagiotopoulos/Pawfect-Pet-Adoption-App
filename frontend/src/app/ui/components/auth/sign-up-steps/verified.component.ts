import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { User, UserRole } from 'src/app/models/user/user.model';

@Component({
  selector: 'app-verified',
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-900">
      <!-- Background elements -->
      <div class="fixed inset-0 z-0">
        <div
          class="absolute inset-0 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900"
        ></div>
        <div
          class="absolute inset-0 bg-gradient-to-br from-primary-900/20 via-secondary-900/20 to-accent-900/20 animate-gradient"
        ></div>
        <div
          class="absolute inset-0 bg-gradient-radial from-transparent via-primary-900/10 to-transparent"
        ></div>
      </div>

      <div class="max-w-md w-full mx-4 z-10">
        <div
          class="bg-white/5 backdrop-blur-lg rounded-2xl shadow-xl p-8 space-y-8 border border-white/10 text-center"
        >
          <!-- Loading State -->
          <div *ngIf="isLoading" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto rounded-full border-4 border-primary-500 border-t-transparent animate-spin"
            ></div>
            <p class="text-gray-400">Επαλήθευση email σε εξέλιξη...</p>
          </div>

          <!-- Success State -->
          <div *ngIf="isVerified" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto bg-gradient-to-r from-primary-500 to-accent-500 rounded-full flex items-center justify-center"
            >
              <ng-icon
                name="lucideCheck"
                [size]="'32'"
                class="text-white"
              ></ng-icon>
            </div>
            <h2 class="text-2xl font-bold text-white">Επιτυχής Επαλήθευση!</h2>
            <p class="text-gray-400">
              {{ getVerificationMessage() }}
            </p>
          </div>

          <!-- Error State -->
          <div *ngIf="error" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto bg-red-500/20 rounded-full flex items-center justify-center"
            >
              <ng-icon
                name="lucideX"
                [size]="'32'"
                class="text-red-500"
              ></ng-icon>
            </div>
            <h2 class="text-2xl font-bold text-white">Σφάλμα Επαλήθευσης</h2>
            <p class="text-red-400">{{ error }}</p>
          </div>

          <!-- Action Button -->
          <div class="pt-4">
            <button
              (click)="navigateHome()"
              class="w-full px-4 py-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1"
            >
              Επιστροφή στην Αρχική
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class VerifiedComponent implements OnInit {
  isLoading = true;
  isVerified = false;
  error: string | null = null;
  userRole: UserRole | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.error = 'Δεν έχετε επαληθέυση τον λογαριασμό σας.';
      this.isLoading = false;
      return;
    }

    this.authService.verifyEmail(token).subscribe(
      (model: User) => {
        this.isVerified = true;
        this.isLoading = false;
        this.error = null;
        this.userRole = model.role!;
      },
      (error) => {
        this.isVerified = false;
        this.isLoading = false;
        console.error('Email verification error:', error);
        this.error = 'Το email επιβεβαίωσης δεν ισχύει πια.';
      }
    );
  }

  getVerificationMessage(): string {
    if (this.userRole === UserRole.Shelter) {
      return 'Η επαλήθευση ολοκληρώθηκε. Ένας διαχειριστής θα εξετάσει την εγγραφή σας. Παρακολουθείτε το email σας για ενημερώσεις.';
    }
    return 'Το email σας έχει επαληθευτεί επιτυχώς.';
  }

  navigateHome(): void {
    this.router.navigate(['/']);
  }
}
