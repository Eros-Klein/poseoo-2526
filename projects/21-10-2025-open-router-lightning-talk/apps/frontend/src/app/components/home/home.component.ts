import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styles: []
})
export class HomeComponent {
  constructor(private router: Router) {}

  navigateToChat() {
    this.router.navigate(['/chat']);
  }
}
