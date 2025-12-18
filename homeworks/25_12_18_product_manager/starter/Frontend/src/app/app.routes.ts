import { Routes } from '@angular/router';
import { ProductList } from './product-list/product-list';
import { ProductEdit } from './product-edit/product-edit';

export const routes: Routes = [
  { path: 'products', component: ProductList },
  { path: 'products/:productId', component: ProductEdit},
  { path: '', pathMatch: 'full', redirectTo: 'products'}
];
