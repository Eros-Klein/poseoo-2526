import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ConnectionStatusComponent } from '../shared/connection-status.component';
import { MessageItemComponent, ChatMessage } from '../shared/message-item.component';
import { ErrorAlertComponent } from '../shared/error-alert.component';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    ConnectionStatusComponent, 
    MessageItemComponent, 
    ErrorAlertComponent
  ],
  templateUrl: './chat.component.html',
  styles: []
})
export class ChatComponent implements OnInit, OnDestroy {
  protected messages = signal<ChatMessage[]>([]);
  protected newMessage = signal('');
  protected senderName = signal('User');
  protected isConnected = signal(false);
  protected builtMessage = signal<ChatMessage>({ id: '', content: '', created_at: '', sender: '', role: 'user' });
  protected error = signal('');
  protected isThinking = signal(false);
  protected model = signal('');
  
  private eventSource: EventSource | null = null;
  private readonly API_URL = 'http://localhost:3001';

  constructor(private router: Router) {}

  async ngOnInit() {
    await this.getMessages();
    await this.getModel();
    this.connectToSSE();
  }

  ngOnDestroy() {
    this.disconnectSSE();
  }

  private async getMessages() {
    const response = await fetch(`${this.API_URL}/api/messages`);
    if (response.ok) {
      const data = await response.json();
      if (data.success) {
        this.messages.set(data.messages);
        console.log('messages: ', this.messages());
      }
    }
    else {
      this.error.set('Error getting messages');
    }
  }

  private async getModel() {
    const response = await fetch(`${this.API_URL}/api/model`);
    if (response.ok) {
      const data = await response.json();
      this.model.set(data.model);
    }
  }

  private connectToSSE() {
    this.eventSource = new EventSource(`${this.API_URL}/api/sse`);

    this.eventSource.onopen = () => {
      console.log('SSE connection opened');
      this.isConnected.set(true);
    };

    this.eventSource.onmessage = (event) => {
      const data = JSON.parse(event.data);
      
      if (data.type === 'connected') {
        console.log(data.message);
      } else if (data.type === 'message_start') {
        setTimeout(() => this.scrollToBottom(), 100);
        this.builtMessage.set({ ...this.builtMessage(), content: 'Thinking...', created_at: new Date().toISOString(), sender: this.model(), role: 'assistant' });
        this.isThinking.set(true);
      } else if (data.type === 'message_chunk') {
        if (this.isThinking()) {
          this.builtMessage.set({ id: Date.now().toString(), content: '', created_at: new Date().toISOString(), sender: this.model(), role: 'assistant' });
          this.isThinking.set(false);
        }
        this.builtMessage.set({ id: Date.now().toString(), content: this.builtMessage().content + data.message, created_at: new Date().toISOString(), sender: this.model(), role: 'assistant' });
      } else if (data.type === 'message_end') {
        if (this.builtMessage().content !== 'Thinking...') {
          this.messages.update(msgs => [...msgs, this.builtMessage()]);
        }
        this.builtMessage.set({ id: '', content: '', created_at: '', sender: this.model(), role: 'assistant' });
        setTimeout(() => this.scrollToBottom(), 100);
      } else if (data.type === 'error') {
        this.error.set(data.message);
      }
    };

    this.eventSource.onerror = (error) => {
      console.error('SSE error:', error);
      this.isConnected.set(false);
    };
  }

  private disconnectSSE() {
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = null;
    }
  }

  async sendMessage() {
    const text = this.newMessage().trim();
    if (!text) return;

    try {
      this.messages.update(msgs => [...msgs, { id: Date.now().toString(), content: text, created_at: new Date().toISOString(), sender: this.senderName() || 'Anonymous', role: 'user' }]);

      this.newMessage.set('');

      const response = await fetch(`${this.API_URL}/api/messages`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          text,
          sender: this.senderName() || 'Anonymous'
        }),
      });

      if (response.ok) {
        this.newMessage.set('');
      }
    } catch (error) {
      console.error('Error sending message:', error);
    }
  }

  handleKeyPress(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom() {
    const messagesContainer = document.getElementById('messages-container');
    if (messagesContainer) {
      messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }
  }
}
