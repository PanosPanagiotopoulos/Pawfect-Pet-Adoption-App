import { Component, Input } from '@angular/core';
import { NgIconsModule } from '@ng-icons/core';

@Component({
  selector: 'app-feature-card',
  template: `
    <div class="bg-white p-10 rounded-2xl shadow-xl transform hover:-translate-y-1 transition-transform duration-300">
      <div [class]="'rounded-full w-16 h-16 flex items-center justify-center mb-6 mx-auto ' + bgColor">
        <ng-icon [name]="icon" [class]="iconColor" [size]="'32'"></ng-icon>
      </div>
      <h3 class="text-2xl font-semibold mb-4 text-gray-800">{{title}}</h3>
      <p class="text-gray-600 leading-relaxed">{{description}}</p>
    </div>
  `,
  standalone: false
})
export class FeatureCardComponent {
  @Input() icon!: string;
  @Input() title!: string;
  @Input() description!: string;
  @Input() bgColor!: string;
  @Input() iconColor!: string;
}