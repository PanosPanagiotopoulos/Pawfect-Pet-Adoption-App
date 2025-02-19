import { Routes } from '@angular/router';

export const routes: Routes = [
  { 
    path: '', 
    loadComponent: () => import('./ui/components/home/home.component').then(m => m.HomeComponent) 
  },
  { 
    path: 'login', 
    loadComponent: () => import('./ui/components/auth/login.component').then(m => m.LoginComponent) 
  },
  { 
    path: 'signup', 
    loadComponent: () => import('./ui/components/auth/signup.component').then(m => m.SignupComponent) 
  },
  { 
    path: 'search', 
    loadComponent: () => import('./ui/components/search/search.component').then(m => m.SearchComponent) 
  },
  { 
    path: '**', 
    redirectTo: '' 
  }
];