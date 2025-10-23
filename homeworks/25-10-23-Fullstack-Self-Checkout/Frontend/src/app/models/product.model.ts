export interface Product {
  id: number;
  name: string;
  unitMeasure: string;
  unitPrice: number;
}

export interface ReceiptLine {
  productId: number;
  quantity: number;
}

export interface CheckoutRequest {
  receiptLines: ReceiptLine[];
}

export interface SelectedReceiptLine {
  product: Product;
  quantity: number;
  price: number;
}

