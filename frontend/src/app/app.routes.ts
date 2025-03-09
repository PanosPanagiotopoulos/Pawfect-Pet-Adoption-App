import { Routes } from '@angular/router';
import { NotFoundComponent } from './ui/components/not-found/not-found.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full',
  },
  {
    path: 'home',
    loadChildren: () =>
      import('./ui/components/home/home.module').then((m) => m.HomeModule),
  },
  {
    path: 'auth',
    loadChildren: () =>
      import('./ui/components/auth/auth.module').then((m) => m.AuthModule),
  },
  {
    path: 'search',
    loadChildren: () =>
      import('./ui/components/search/search.module').then(
        (m) => m.SearchModule
      ),
  },
  {
    path: '404',
    component: NotFoundComponent
  },
  {
    path: '**',
    redirectTo: '/404',
  },
];