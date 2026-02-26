import { Routes } from '@angular/router';
import { Dashboard } from './dashboard/dashboard';
import { Editor } from './editor/editor';

export const routes: Routes = [
  { path: 'dashboard', component: Dashboard },
  { path: 'editor', component: Editor },
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
];
