import { Routes } from '@angular/router';
import { ToDoList } from './to-do-list/to-do-list';
import { ToDoAdd } from './to-do-add/to-do-add';

export const routes: Routes = [
    { path: 'list', component: ToDoList },
    { path: 'list/add', component: ToDoAdd },
    { path: '', redirectTo: '/list', pathMatch: 'full' }
];
