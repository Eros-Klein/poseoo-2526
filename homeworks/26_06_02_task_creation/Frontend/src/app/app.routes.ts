import { Routes } from '@angular/router';
import { StagesList } from './stages-list/stages-list';
import { StageDetail } from './stage-detail/stage-detail';
import { Statistics } from './statistics/statistics';

export const routes: Routes = [
    { path: 'stages', component: StagesList },
    { path: 'stages/:id', component: StageDetail },
    { path: 'statistics', component: Statistics },
    { path: '', redirectTo: '/stages', pathMatch: 'full' }
];
