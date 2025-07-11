import { Routes } from '@angular/router';
import { NotFoundComponent } from './ui/components/not-found/not-found.component';
import { AuthGuard } from './common/guards/auth.guard';
import { Permission } from './common/enum/permission.enum';

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
    path: 'adopt',
    loadChildren: () =>
      import('./ui/components/adopt/adopt.module').then(
        (m) => m.AdoptModule
      ),
    canActivate: [AuthGuard],
    data: {
      permissions: [Permission.CreateAdoptionApplications]
    }
  },
  {
    path: 'profile',
    loadChildren: () =>
      import('./ui/components/profile/profile.module').then(
        (m) => m.ProfileModule
      ),
    canActivate: [AuthGuard],
    data: {
      permissions: [Permission.BrowseUsers]
    }
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