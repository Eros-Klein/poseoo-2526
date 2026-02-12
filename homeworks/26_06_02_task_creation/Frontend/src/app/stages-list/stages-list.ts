import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stages-list',
  imports: [CommonModule],
  templateUrl: './stages-list.html',
  styleUrl: './stages-list.css'
})
export class StagesList {
  // TODO: Create a signal to store stages array
  // Hint: Use signal<Stage[]>([])

  // TODO: Create signals for loading and error states
  // Hint: Use signal<boolean>(false) for loading
  // Hint: Use signal<string | null>(null) for error message

  // TODO: Inject the API service (or MockGrammyDataService for testing)
  // Hint: Use inject() function

  // TODO: Inject Router for navigation
  // Hint: Use inject(Router)

  // TODO: Implement ngOnInit lifecycle hook
  // 1. Set loading to true
  // 2. Fetch stages from API
  // 3. Update stages signal with fetched data
  // 4. Handle errors and update error signal
  // 5. Set loading to false when done

  // TODO: Implement viewStageDetails method
  // Takes stageId as parameter
  // Navigates to /stages/:id route
}
