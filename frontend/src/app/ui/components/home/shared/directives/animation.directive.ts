import { Directive, ElementRef, Input, AfterViewInit } from '@angular/core';

@Directive({
  selector: '[appAnimation]'
})
export class AnimationDirective implements AfterViewInit {
  @Input() animationDelay: number = 0;
  @Input() threshold: number = 0.1;

  constructor(private el: ElementRef) {}

  ngAfterViewInit() {
    // Add initial classes
    this.el.nativeElement.style.opacity = '0';
    this.el.nativeElement.style.transition = `all 0.5s cubic-bezier(0.4, 0, 0.2, 1) ${this.animationDelay}ms`;

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            // Add visible class after a small delay to ensure transition works
            setTimeout(() => {
              this.el.nativeElement.style.opacity = '1';
              this.el.nativeElement.style.transform = 'none';
            }, 50);
            observer.unobserve(entry.target);
          }
        });
      },
      {
        threshold: this.threshold,
        rootMargin: '50px'
      }
    );

    observer.observe(this.el.nativeElement);
  }
}