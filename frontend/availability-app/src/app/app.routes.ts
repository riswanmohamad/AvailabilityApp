import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ServiceFormComponent } from './components/service-form/service-form.component';
import { AvailabilityManagerComponent } from './components/availability-manager/availability-manager.component';
import { PublicServiceViewComponent } from './components/public-service-view/public-service-view.component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'services/create', component: ServiceFormComponent, canActivate: [AuthGuard] },
  { path: 'services/:id/edit', component: ServiceFormComponent, canActivate: [AuthGuard] },
  { path: 'services/:id/availability', component: AvailabilityManagerComponent, canActivate: [AuthGuard] },
  { path: 'public/:token', component: PublicServiceViewComponent },
  { path: '**', redirectTo: '/login' }
];
