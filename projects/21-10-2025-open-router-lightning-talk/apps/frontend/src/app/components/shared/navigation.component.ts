import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './navigation.component.html',
  styles: []
})
export class NavigationComponent {
  isHomePage = false;
  isChatPage = false;
  private readonly API_URL = 'http://localhost:3001';
  constructor(private router: Router) {
    // Update active states based on current route
    this.router.events.subscribe(() => {
      const url = this.router.url;
      this.isHomePage = url === '/home' || url === '/';
      this.isChatPage = url === '/chat';
    });
  }

  navigateToHome() {
    this.router.navigate(['/home']);
  }

  navigateToChat() {
    this.router.navigate(['/chat']);
  }

  async clearMessages() {
    const response = await fetch(`${this.API_URL}/api/messages`, {
      method: 'DELETE',
    });

    if (response.ok) {
      location.reload();
    }
    else {
      console.error('Error clearing messages');
    }
  }
}
