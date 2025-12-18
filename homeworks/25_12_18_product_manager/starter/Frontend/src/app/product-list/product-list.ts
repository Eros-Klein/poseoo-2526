import { deleteSpecificProduct } from './../api/fn/products-endpoints/delete-specific-product';
import { Component, inject, signal } from '@angular/core';
import { Product } from '../api/models';
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment.development';
import { getCategories, getProducts } from '../api/functions';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-list',
  imports: [FormsModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css',
})
export class ProductList {
protected readonly products = signal<Product[]>([]);
  protected readonly categories = signal<string[]>([]);

  protected readonly selectedCategory = signal<string>('');
  protected readonly maxUnitPrice = signal<number | null>(null);

  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);
  private router = inject(Router);

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadData();
  }

  private async loadData() {
    let filter = {};
    if (this.selectedCategory()) {
      filter = { ...filter, category: this.selectedCategory() };
    }
    if (this.maxUnitPrice() !== null) {
      filter = { ...filter, maxPrice: this.maxUnitPrice() };
    }
    const products = await this.api.invoke(getProducts, filter);

    const categories = await this.api.invoke(getCategories, {});

    this.products.set(products);
    this.categories.set(categories.categories);
  }

  protected onSearch() {
    this.loadData();
  }

  protected async deleteProduct(id: number) {
    if (confirm('Are you sure you want to delete this product?')) {
      await this.api.invoke(deleteSpecificProduct, { id });
      await this.loadData();
    }
  }

  protected editProduct(id: number) {
    this.router.navigate(['/products', id]);
  }
}
