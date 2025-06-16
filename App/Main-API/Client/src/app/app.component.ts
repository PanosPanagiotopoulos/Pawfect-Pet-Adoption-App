import { Component, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';
import { LoggedAccount } from './models/auth/auth.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  
  constructor(private authService: AuthService) {}
  
  ngOnInit(): void {
    // The me() request will be shared with the auth guard
    this.authService.me().subscribe();
  }

}
