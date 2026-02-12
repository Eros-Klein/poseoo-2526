import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-statistics',
  imports: [CommonModule],
  templateUrl: './statistics.html',
  styleUrl: './statistics.css'
})
export class Statistics implements OnInit {
  // TODO: Create a signal to store statistics array
  // Hint: Use signal<ArtistStatistics[]>([])

  // TODO: Create signals for loading and error states
  // Hint: Use signal<boolean>(false) for loading
  // Hint: Use signal<string | null>(null) for error message

  // TODO: Create a computed signal or method to check if winners have been announced
  // Hint: hasWinners can be determined by checking if statistics array length > 0

  // TODO: Inject the API service (or MockGrammyDataService for testing)
  // Hint: Use inject() function

  ngOnInit() {
    // TODO: Implement initialization logic
    // 1. Set loading to true
    // 2. Fetch statistics from API
    // 3. Update statistics signal with fetched data
    // 4. Handle errors and update error signal
    // 5. Set loading to false when done
  }

  // TODO: Implement formatScore method
  // Takes score: number as parameter
  // Returns score formatted to 2 decimal places
  // Hint: Use toFixed(2)

  // TODO: Implement getScoreClass method
  // Takes score: number as parameter
  // Returns CSS class based on score value:
  //   - 'score-high' if score > 1
  //   - 'score-medium' if score > 0.5
  //   - 'score-low' otherwise

  // TODO: Implement getRankBadge method
  // Takes index: number as parameter (0-based)
  // Returns rank badge:
  //   - '🥇' for index 0 (1st place)
  //   - '🥈' for index 1 (2nd place)
  //   - '🥉' for index 2 (3rd place)
  //   - '#N' for others (e.g., '#4', '#5', etc.)
}
