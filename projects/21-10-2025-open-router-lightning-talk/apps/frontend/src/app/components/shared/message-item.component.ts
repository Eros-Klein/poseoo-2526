import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ChatMessage {
  id: string;
  content: string;
  created_at: string;
  sender: string;
  role: 'system' | 'user' | 'assistant';
}

@Component({
  selector: 'app-message-item',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './message-item.component.html',
  styles: []
})
export class MessageItemComponent {
  message = input.required<ChatMessage>();

  formatTime(timestamp: string): string {
    const date = new Date(timestamp);
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  }
}
