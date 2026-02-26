import { Component, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface AppointmentServiceView {
  id?: number;
  name?: string;
  styleReference?: number;
  appointmentId?: number;
}

export interface AppointmentView {
  id: number;
  date: string;
  startTime: string;
  duration: string;
  customerName: string;
  services: AppointmentServiceView[];
  barberName: string | null;
  beverageChoice: string | null;
  isVip: boolean;
  calculatedPrice: number | null;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.html',
})
export class Dashboard {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl ?? '';

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly appointments = signal<AppointmentView[]>([]);

  constructor() {
    this.loadAppointments();
  }

  protected loadAppointments(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<AppointmentView[]>(`${this.baseUrl}/appointments`).subscribe({
      next: (data) => {
        this.appointments.set(Array.isArray(data) ? data : []);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.message ?? 'Failed to load appointments');
        this.appointments.set([]);
        this.loading.set(false);
      },
    });
  }

  protected deleteAppointment(id: number): void {
    this.http.delete(`${this.baseUrl}/appointments/${id}`).subscribe({
      next: () => this.loadAppointments(),
      error: (err) => this.error.set(err?.error?.message ?? err?.message ?? 'Delete failed'),
    });
  }

  protected formatServices(services: AppointmentServiceView[] | undefined): string {
    if (!services?.length) return '—';
    return services.map((s) => s.name ?? `Style ${s.styleReference ?? ''}`).join(', ');
  }

  protected formatPrice(price: number | null | undefined): string {
    if (price == null) return '—';
    return typeof price === 'number' ? price.toFixed(2) : '—';
  }

  protected formatDate(dateStr: string | undefined): string {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return isNaN(d.getTime()) ? dateStr : d.toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' });
  }

  protected formatTime(timeStr: string | undefined): string {
    if (!timeStr) return '—';
    const [h, m] = timeStr.split(':');
    return `${h ?? '00'}:${m ?? '00'}`;
  }
}
