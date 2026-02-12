import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from "@angular/router";

// TODO: Import routing modules (RouterOutlet, RouterLink, RouterLinkActive)
// Hint: import from '@angular/router'

@Component({
  selector: 'app-root',
  // TODO: Add router modules to imports array
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  // TODO: Create a signal to store the application title
  // Hint: Use signal<string>('Grammy Performance Planning')
}
