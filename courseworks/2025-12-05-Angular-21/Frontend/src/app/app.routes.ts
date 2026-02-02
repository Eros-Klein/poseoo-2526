import { Routes } from '@angular/router';
import { CustomerList } from './customer-list/customer-list';
import { CustomerEdit } from './customer-edit/customer-edit';
import { CustomerListInlineEdit } from './customer-list-inline-edit/customer-list-inline-edit';

export const routes: Routes = [
  { path: '', redirectTo: '/customers', pathMatch: 'full' },
  { path: 'customers', component: CustomerList },
  { path: 'customers-inline', component: CustomerListInlineEdit},
  { path: 'customers/:id/edit', component: CustomerEdit }
];
