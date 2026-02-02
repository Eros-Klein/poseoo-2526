import { Component, EventEmitter, inject, Input, model, OnInit, Output, signal, WritableSignal } from '@angular/core';
import { Customer } from '../api/models';
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';
import { Field, form, max, maxLength, min, required } from "@angular/forms/signals";

interface CustomerFormModel {
  name: string;
  dateOfBirth: string;
  revenue: number;
  customerValue: number;
  isActive: boolean;
}

@Component({
  selector: 'app-customer-list-element',
  imports: [Field],
  templateUrl: './customer-list-element.html',
  styleUrl: './customer-list-element.css',
})
export class CustomerListElement implements OnInit {
  customerFormModel: WritableSignal<CustomerFormModel> = signal<CustomerFormModel>({
    name: "",
    dateOfBirth: "1920-01-01",
    revenue: 0,
    customerValue: 0,
    isActive: false
  })

  isSelected = model<boolean>(false)

  @Input({ required: true })
  customer!: Customer

  @Input({ required: true })
  toggleSelection!: (customerId: number) => void

  @Output()
  successfulEdit = new EventEmitter<Customer>()

  inEdit = signal<boolean>(false)

  private api = inject(Api)
  private apiConfig = inject(ApiConfiguration)

  ngOnInit(): void {
    this.apiConfig.rootUrl = environment.apiBaseUrl

    this.customerFormModel.set({
      name: this.customer.name,
      dateOfBirth: this.customer.dateOfBirth,
      revenue: this.customer.revenue,
      customerValue: this.customer.customerValue,
      isActive: this.customer.isActive
    })
  }

  deleteCustomer(customerId: number) {

  }

  protected readonly customerForm = form(this.customerFormModel, (schemaPath) => {
    required(schemaPath.name, { message: 'Name is required' });
    required(schemaPath.dateOfBirth, { message: 'Date of birth is required' });
    required(schemaPath.revenue, { message: 'Revenue is required' });
    required(schemaPath.customerValue, { message: 'Customer value is required' });

    maxLength(schemaPath.name, 50, { message: 'Name must be at most 50 characters' });

    min(schemaPath.revenue, 0, { message: 'Revenue must be at least 0' });
    min(schemaPath.customerValue, 0, { message: 'Customer value must be at least 0' });
    max(schemaPath.customerValue, 10, { message: 'Customer value must be at most 10' });
  });
}
