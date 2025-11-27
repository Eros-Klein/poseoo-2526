import { Routes } from '@angular/router';
import { TimeEntryList } from './time-entry-list/time-entry-list';
import { Edit } from './edit/edit';

export const routes: Routes = [
    { path: '', component: TimeEntryList },
    { path: 'edit/:id', component: Edit }
];
