import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../services/product.service';
import { Product, SelectedReceiptLine, CheckoutRequest } from '../models/product.model';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
  private productService = inject(ProductService);
  
  products = signal<Product[]>([]);
  selectedReceiptLines = signal<SelectedReceiptLine[]>([]);
  
  totalPrice = computed(() => {
    return this.selectedReceiptLines().reduce((sum, line) => sum + line.price, 0);
  });

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.productService.getAllProducts().subscribe({
      next: (products) => this.products.set(products),
      error: (error) => console.error('Error loading products:', error)
    });
  }

  addProduct(product: Product): void {
    const existingLine = this.selectedReceiptLines().find(
      line => line.product.id === product.id
    );

    if (existingLine) {
      this.selectedReceiptLines.update(lines =>
        lines.map(line =>
          line.product.id === product.id
            ? {
                ...line,
                quantity: line.quantity + 1,
                price: product.unitPrice * (line.quantity + 1)
              }
            : line
        )
      );
    } else {
      this.selectedReceiptLines.update(lines => [
        ...lines,
        {
          product,
          quantity: 1,
          price: product.unitPrice
        }
      ]);
    }
  }

  checkout(): void {
    const checkoutRequest: CheckoutRequest = {
      receiptLines: this.selectedReceiptLines().map(line => ({
        productId: line.product.id,
        quantity: line.quantity
      }))
    };

    this.productService.checkout(checkoutRequest).subscribe({
      next: () => {
        alert('Checkout successful!');
        this.selectedReceiptLines.set([]);
      },
      error: (error) => {
        console.error('Checkout error:', error);
        alert('Checkout failed. Please try again.');
      }
    });
  }
}

