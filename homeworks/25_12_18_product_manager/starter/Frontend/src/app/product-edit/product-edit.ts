import { getCategories } from './../api/fn/products-endpoints/get-categories';
import { Component, inject, input, signal } from '@angular/core';
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment.development';
import { getSpecificProduct, updateSpecificProduct } from '../api/functions';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Field, form, max, maxLength, min, required } from '@angular/forms/signals';
import { CommonModule } from '@angular/common';
import { ProductUpdateReq } from '../api/models';
import { firstValueFrom } from 'rxjs';

interface ProductUpdateReqAdjusted {
    productName: string,
    productCode: string,
    productDescription: string,
    pricePerUnit: number,
    category: string
}

@Component({
  selector: 'app-product-edit',
  imports: [Field],
  templateUrl: './product-edit.html',
  styleUrl: './product-edit.css',
})
export class ProductEdit {
protected readonly loading = signal<boolean>(true);
  protected readonly saving = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);

  protected readonly product = signal<ProductUpdateReqAdjusted>({
    productName: "",
    productCode: "",
    productDescription: "",
    pricePerUnit: 0,
    category: ""
  });

  protected readonly categories = signal<string[]>([]);

  protected readonly productForm = form(this.product, (schemaPath) => {
    required(schemaPath.productName, { message: 'ProductName is required' });
    required(schemaPath.pricePerUnit, { message: 'Price per Unit is required' });
    required(schemaPath.productCode, { message: 'ProductCode is required' });

    maxLength(schemaPath.productCode, 10, { message: 'ProductCode must be at most 10 characters' });
    maxLength(schemaPath.productName, 100, { message: 'ProductName must be at most 100 characters' });
    maxLength(schemaPath.category, 50, { message: 'Category must be at most 50 characters' });
    maxLength(schemaPath.productDescription, 255, { message: 'ProductDescription must be at most 255 characters' })
  });

  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);
  private router = inject(Router);
  public productId = input.required<number>();

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    await this.loadData();
  }

  private async loadData() {
    try {
      console.log(this.productId())
      const product = await this.api.invoke(getSpecificProduct, { id: this.productId()! });

      const categories = await this.api.invoke(getCategories);

      if (categories) {
        this.categories.set(categories.categories);
      }

      if (product) {
        this.product.set({
          productName: product.productName??"",
          productCode: product.productCode??"",
          productDescription: product.productDescription??"",
          pricePerUnit: product.pricePerUnit??0,
          category: product.category??""
        })
      } else {
        this.error.set('Product not found');
      }
    } catch (err) {
      this.error.set('Failed to load product');
      console.error(err);
    } finally {
      this.loading.set(false);
    }
  }

  protected async onSubmit(event: Event) {
    event.preventDefault();

    if (this.productForm.productName().invalid() ||
        this.productForm.pricePerUnit().invalid() ||
        this.productForm.productCode().invalid() ||
        this.productForm.productDescription().invalid()) {
      this.error.set('Please correct the errors in the form.');
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    try {
      const formData = this.product();
      const patchDto: ProductUpdateReq = {
        productName: formData.productName,
        pricePerUnit: formData.pricePerUnit,
        productCode: formData.productCode,
        productDescription: formData.productDescription,
        category: formData.category
      };

      await this.api.invoke(updateSpecificProduct, {
        id: this.productId(),
        body: patchDto
      });

      this.router.navigate(['/products']);
    } catch (error: any) {
      this.error.set('Error saving: ' + (error.message || JSON.stringify(error)));
    } finally {
      this.saving.set(false);
    }
  }

  protected cancel() {
    this.router.navigate(['/products']);
  }
}
