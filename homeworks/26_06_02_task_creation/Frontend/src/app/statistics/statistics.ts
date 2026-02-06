import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MockGrammyDataService } from '../services/mock-grammy-data.service';
import { ArtistStatistics } from '../models/grammy.models';

@Component({
  selector: 'app-statistics',
  imports: [CommonModule],
  templateUrl: './statistics.html',
  styleUrl: './statistics.css'
})
export class Statistics {
  protected readonly statistics = signal<ArtistStatistics[]>([]);
  protected readonly hasWinners = signal<boolean>(false);

  private mockDataService = inject(MockGrammyDataService);

  ngOnInit() {
    const stats = this.mockDataService.getStatistics();
    this.statistics.set(stats);
    this.hasWinners.set(stats.length > 0);
  }

  formatScore(score: number): string {
    return score.toFixed(2);
  }

  getScoreClass(score: number): string {
    if (score > 1) return 'score-high';
    if (score > 0.5) return 'score-medium';
    return 'score-low';
  }

  getRankBadge(index: number): string {
    switch(index) {
      case 0: return '🥇';
      case 1: return '🥈';
      case 2: return '🥉';
      default: return `#${index + 1}`;
    }
  }
}
