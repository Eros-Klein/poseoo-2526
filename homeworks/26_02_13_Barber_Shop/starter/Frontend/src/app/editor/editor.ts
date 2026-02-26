import { Component, signal, inject, computed } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { environment } from '../../environments/environment';

interface ServiceOption {
  styleReference: number;
  name: string;
  basePrice: number;
}

// StyleReference enum and base prices from spec (hardcoded per README)
const SERVICE_OPTIONS: ServiceOption[] = [
  { styleReference: 0, name: 'Short', basePrice: 25 },
  { styleReference: 1, name: 'Medium', basePrice: 30 },
  { styleReference: 2, name: 'Long', basePrice: 35 },
  { styleReference: 3, name: 'Faded', basePrice: 40 },
  { styleReference: 4, name: 'Tapered', basePrice: 38 },
  { styleReference: 5, name: 'Undercut', basePrice: 42 },
  { styleReference: 6, name: 'Layered', basePrice: 45 },
  { styleReference: 7, name: 'Textured', basePrice: 48 },
  { styleReference: 8, name: 'Slicked Back', basePrice: 35 },
  { styleReference: 9, name: 'Side Parted', basePrice: 32 },
  { styleReference: 10, name: 'Forward Crop', basePrice: 38 },
  { styleReference: 11, name: 'Voluminous', basePrice: 50 },
  { styleReference: 12, name: 'Natural', basePrice: 28 },
  { styleReference: 13, name: 'Mullet Style', basePrice: 60 },
  { styleReference: 14, name: 'Mohawk Style', basePrice: 65 },
  { styleReference: 15, name: 'Beard Shaped', basePrice: 15 },
  { styleReference: 16, name: 'Clean Shaven', basePrice: 12 },
  { styleReference: 17, name: 'Hot Towel Shave', basePrice: 18 },
];

@Component({
  selector: 'app-editor',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './editor.html',
})
export class Editor {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly baseUrl = environment.apiBaseUrl ?? '';

  protected readonly serviceOptions = SERVICE_OPTIONS;
  protected readonly selectedServices = signal<number[]>([]);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly submitting = signal(false);
  protected readonly estimatePrice = signal<number | null>(null);
  protected readonly estimateDisplay = computed(() => {
    const p = this.estimatePrice();
    return p != null ? p.toFixed(2) : '--';
  });

  protected form: FormGroup = this.fb.group({
    customerName: ['', Validators.required],
    date: ['', Validators.required],
    startTime: ['09:00', Validators.required],
    durationMinutes: [60, [Validators.required, Validators.min(15)]],
    barberName: ['Todd', Validators.required],
    beverageChoice: [''],
    isVip: [false],
  });

  constructor() {
    this.form.valueChanges.subscribe(() => this.updateEstimate());
  }

  protected isServiceSelected(styleRef: number): boolean {
    return this.selectedServices().includes(styleRef);
  }

  protected toggleService(opt: ServiceOption): void {
    const current = this.selectedServices();
    if (current.includes(opt.styleReference)) {
      this.selectedServices.set(current.filter((s) => s !== opt.styleReference));
    } else {
      this.selectedServices.set([...current, opt.styleReference]);
    }
    this.updateEstimate();
  }

  private buildPayload(): {
    customerName: string;
    date: string;
    startTime: string;
    duration: string;
    barberName: string;
    beverageChoice: string | null;
    isVip: boolean;
    services: { name: string; styleReference: number }[];
  } | null {
    const d = this.form.value;
    const mins = Number(d.durationMinutes) || 60;
    const hours = Math.floor(mins / 60);
    const remainderMins = mins % 60;
    const durationStr = `${hours.toString().padStart(2, '0')}:${remainderMins.toString().padStart(2, '0')}:00`;
    const services = this.selectedServices().map((styleRef) => {
      const opt = SERVICE_OPTIONS.find((o) => o.styleReference === styleRef);
      return { name: opt?.name ?? `Style ${styleRef}`, styleReference: styleRef };
    });
    if (services.length === 0) return null;
    return {
      customerName: d.customerName ?? '',
      date: d.date ?? '',
      startTime: (d.startTime ?? '09:00').slice(0, 5),
      duration: durationStr,
      barberName: d.barberName ?? 'Todd',
      beverageChoice: d.beverageChoice && d.beverageChoice.trim() ? d.beverageChoice.trim() : null,
      isVip: !!d.isVip,
      services,
    };
  }

  private updateEstimate(): void {
    const payload = this.buildPayload();
    if (!payload || !payload.date || !payload.customerName) {
      this.estimatePrice.set(null);
      return;
    }
    this.http.post<{ calculatedPrice?: number; message?: string }>(`${this.baseUrl}/appointments/estimate`, payload).subscribe({
      next: (res) => this.estimatePrice.set(res.calculatedPrice ?? null),
      error: () => this.estimatePrice.set(null),
    });
  }

  protected onSubmit(): void {
    this.errorMessage.set(null);
    const payload = this.buildPayload();
    if (!payload) {
      this.errorMessage.set('Please select at least one service.');
      return;
    }
    if (!this.form.valid) {
      this.errorMessage.set('Please fill in all required fields.');
      return;
    }
    this.submitting.set(true);
    this.http.post<{ id: number }>(`${this.baseUrl}/appointments`, payload).subscribe({
      next: () => {
        this.submitting.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.submitting.set(false);
        const msg = err?.error?.message ?? err?.message ?? 'Failed to create appointment';
        this.errorMessage.set(msg);
      },
    });
  }
}
