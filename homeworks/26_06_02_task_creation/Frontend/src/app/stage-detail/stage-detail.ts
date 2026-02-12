import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stage-detail',
  imports: [CommonModule],
  templateUrl: './stage-detail.html',
  styleUrl: './stage-detail.css'
})
export class StageDetail implements OnInit {
  // TODO: Create a signal to store the stage data
  // Hint: Use signal<Stage | null>(null)

  // TODO: Create signals for loading and error states
  // Hint: Use signal<boolean>(true) for loading (start as true)
  // Hint: Use signal<string | null>(null) for error message

  // TODO: Expose PriorityLevel enum to template
  // Hint: protected readonly PriorityLevel = PriorityLevel;

  // TODO: Inject ActivatedRoute to get route parameter (stage ID)
  // Hint: Use inject(ActivatedRoute)

  // TODO: Inject Router for navigation
  // Hint: Use inject(Router)

  // TODO: Inject the API service (or MockGrammyDataService for testing)
  // Hint: Use inject() function

  ngOnInit() {
    // TODO: Implement initialization logic
    // 1. Get the stage ID from route parameters
    //    Hint: this.route.snapshot.paramMap.get('id')
    // 2. If ID exists, fetch stage data from API
    // 3. Update stage signal with fetched data
    // 4. If stage not found, set error message and/or redirect to stages list
    // 5. Handle errors and update error signal
    // 6. Set loading to false when done
  }

  // TODO: Implement goBack method
  // Navigates back to /stages route

  // TODO: Implement getPriorityLabel method
  // Takes priority: PriorityLevel as parameter
  // Returns 'Across Genres' for AcrossGenres priority
  // Returns 'Genre Specific' for GenreSpecific priority

  // TODO: Implement formatCurrency method
  // Takes amount: number as parameter
  // Returns formatted currency string (USD format)
  // Hint: Use Intl.NumberFormat with style: 'currency', currency: 'USD'

  // TODO: Implement formatDateTime method
  // Takes dateTime: string as parameter
  // Returns formatted date/time string
  // Hint: Use Date.toLocaleString() with appropriate options
}
