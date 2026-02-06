import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MockGrammyDataService } from '../services/mock-grammy-data.service';
import { Stage } from '../models/grammy.models';

@Component({
  selector: 'app-stages-list',
  imports: [CommonModule],
  templateUrl: './stages-list.html',
  styleUrl: './stages-list.css'
})
export class StagesList {
  protected readonly stages = signal<Stage[]>([]);

  private mockDataService = inject(MockGrammyDataService);
  private router = inject(Router);

  ngOnInit() {
    this.stages.set(this.mockDataService.getStages());
  }

  viewStageDetails(stageId: string): void {
    this.router.navigate(['/stages', stageId]);
  }
}
