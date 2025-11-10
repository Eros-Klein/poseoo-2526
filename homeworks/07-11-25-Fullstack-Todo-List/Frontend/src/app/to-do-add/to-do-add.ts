import { Component, inject, signal } from '@angular/core';
import { toDoPost } from '../api/functions';
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-to-do-add',
  imports: [FormsModule, RouterLink],
  templateUrl: './to-do-add.html',
  styleUrl: './to-do-add.css',
})
export class ToDoAdd {
  protected title = signal<string>('');
  protected assignee = signal<string>('');

  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);
  private router = inject(Router);
  
  protected readonly addTodo = async () => {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;
    
    await this.api.invoke(toDoPost, { body: { title: this.title(), assignee: this.assignee() } });

    this.router.navigate(['/list'], { replaceUrl: true });
  };
}
