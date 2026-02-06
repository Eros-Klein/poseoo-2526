import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MockGrammyDataService } from '../services/mock-grammy-data.service';
import { Stage, PriorityLevel } from '../models/grammy.models';

@Component({
  selector: 'app-stage-detail',
  imports: [CommonModule],
  templateUrl: './stage-detail.html',
  styleUrl: './stage-detail.css'
})
export class StageDetail implements OnInit {
  protected readonly stage = signal<Stage | null>(null);
  protected readonly PriorityLevel = PriorityLevel;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private mockDataService = inject(MockGrammyDataService);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      const foundStage = this.mockDataService.getStageById(id);
      if (foundStage) {
        this.stage.set(foundStage);
      } else {
        // Stage not found, redirect to stages list
        this.router.navigate(['/stages']);
      }
    }
  }

  goBack(): void {
    this.router.navigate(['/stages']);
  }

  getPriorityLabel(priority: PriorityLevel): string {
    return priority === PriorityLevel.AcrossGenres ? 'Across Genres' : 'Genre Specific';
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0
    }).format(amount);
  }

  formatDateTime(dateTime: string): string {
    return new Date(dateTime).toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      timeZoneName: 'short'
    });
  }
}
