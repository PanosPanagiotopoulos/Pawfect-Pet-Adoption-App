import { Component } from '@angular/core';
import { NgIconsModule } from '@ng-icons/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-footer',
  template: `
    <div class="relative bg-gray-900 text-white py-12 border-t border-gray-800">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-8">
          <div>
            <h3 class="text-xl font-semibold mb-4">Pawfect</h3>
            <p class="text-gray-400">Finding forever homes for furry friends</p>
          </div>
          <div>
            <h4 class="text-lg font-medium mb-4">Quick Links</h4>
            <ul class="space-y-2">
              <li><a routerLink="/about" class="text-gray-400 hover:text-white transition-colors">About Us</a></li>
              <li><a routerLink="/contact" class="text-gray-400 hover:text-white transition-colors">Contact</a></li>
            </ul>
          </div>
          <div>
            <h4 class="text-lg font-medium mb-4">Contact Us</h4>
            <ul class="space-y-2 text-gray-400">
              <li class="flex items-center">
                <ng-icon name="lucidePhone" [size]="'24'" class="mr-2"></ng-icon>
                (555) 123-4567
              </li>
              <li class="flex items-center">
                <ng-icon name="lucideMail" [size]="'24'" class="mr-2"></ng-icon>
                {{'info@pawfectmatch.com'}}
              </li>
            </ul>
          </div>
          <div>
            <h4 class="text-lg font-medium mb-4">Follow Us</h4>
            <div class="flex space-x-4">
              <a href="#" class="text-gray-400 hover:text-white transition-colors">
                <ng-icon name="lucideHeart" [size]="'24'"></ng-icon>
              </a>
              <a href="#" class="text-gray-400 hover:text-white transition-colors">
                <ng-icon name="lucideMessageCircle" [size]="'24'"></ng-icon>
              </a>
            </div>
          </div>
        </div>
        <div class="mt-8 pt-8 border-t border-gray-800 text-center text-gray-400">
          <p>&copy; {{currentYear}} Pawfect Match. All rights reserved.</p>
        </div>
      </div>
    </div>
  `
})
export class FooterComponent {
  currentYear = new Date().getFullYear();
}