import { Component, inject, signal } from '@angular/core';
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';
import { ToDo } from '../api/models';
import { toDoGet, toDoIdDelete, toDoIdPatch } from '../api/functions';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-to-do-list',
  imports: [RouterLink, FormsModule],
  templateUrl: './to-do-list.html',
  styleUrl: './to-do-list.css'
})
export class ToDoList {
  protected readonly todos = signal<ToDo[] | null>(null);
  protected readonly editingId = signal<number | null>(null);
  protected readonly editTitle = signal<string>('');
  protected readonly editAssignee = signal<string>('');
  protected readonly savePending = signal<boolean>(false);
  protected readonly deletePendingId = signal<number | null>(null);
  protected get editTitleModel(): string {
    return this.editTitle();
  }

  protected set editTitleModel(value: string) {
    this.editTitle.set(value);
  }

  protected get editAssigneeModel(): string {
    return this.editAssignee();
  }

  protected set editAssigneeModel(value: string) {
    this.editAssignee.set(value);
  }
  
  private api = inject(Api);
  private apiConfiguration = inject(ApiConfiguration);

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;

    this.todos.set(await this.api.invoke(toDoGet, {}));
  }

  protected readonly startEdit = (todo: ToDo) => {
    if (todo.id == null) {
      return;
    }

    this.editingId.set(todo.id);
    this.editTitle.set(todo.title ?? '');
    this.editAssignee.set(todo.assignee ?? '');
  };

  protected readonly cancelEdit = () => {
    this.editingId.set(null);
    this.editTitle.set('');
    this.editAssignee.set('');
    this.savePending.set(false);
  };

  protected readonly saveEdit = async () => {
    const id = this.editingId();
    if (id == null || this.savePending()) {
      return;
    }

    const title = this.editTitle().trim();
    const assignee = this.editAssignee().trim();
    if (!title || !assignee) {
      return;
    }

    this.savePending.set(true);

    try {
      this.apiConfiguration.rootUrl = environment.apiBaseUrl;
      const updated = await this.api.invoke(toDoIdPatch, {
        id,
        body: { title, assignee }
      });

      this.todos.update(list => {
        if (!list) {
          return list;
        }
        return list.map(item =>
          item.id === id ? { ...item, ...updated, title, assignee } : item
        );
      });
      this.cancelEdit();
    } catch (error) {
      console.error('Failed to update todo', error);
      this.savePending.set(false);
    }
  };

  protected readonly removeTodo = async (id: number | undefined) => {
    if (id == null || this.deletePendingId() === id) {
      return;
    }

    this.deletePendingId.set(id);

    try {
      this.apiConfiguration.rootUrl = environment.apiBaseUrl;
      await this.api.invoke(toDoIdDelete, { id });

      this.todos.update(list => list?.filter(todo => todo.id !== id) ?? list);

      if (this.editingId() === id) {
        this.cancelEdit();
      }
    } catch (error) {
      console.error('Failed to delete todo', error);
    } finally {
      this.deletePendingId.set(null);
    }
  };
}
