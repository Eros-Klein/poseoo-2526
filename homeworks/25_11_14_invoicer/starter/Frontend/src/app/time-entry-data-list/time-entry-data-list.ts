import { Component, EventEmitter, inject, Input, OnInit, Output, signal, WritableSignal } from '@angular/core';
import { TimeEntry } from '../api/models';
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';
import { deleteTimeentryById } from '../api/functions';
import { Router } from '@angular/router';

@Component({
  selector: 'app-time-entry-data-list',
  imports: [],
  templateUrl: './time-entry-data-list.html',
  styleUrl: './time-entry-data-list.css',
})
export class TimeEntryDataList implements OnInit {
  @Input()
  timeEntries: WritableSignal<TimeEntry[]> = signal([])

  @Output()
  onDelete = new EventEmitter()

  router = inject(Router)
  api = inject(Api)
  apiConfig = inject(ApiConfiguration)

  async ngOnInit() {
    this.apiConfig.rootUrl = environment.apiBaseUrl
  }


  async deleteEntry(id: number){
    await this.api.invoke(deleteTimeentryById, {
      id: id
    });

    this.onDelete.emit()
  }

  editEntry(id: number) {
    this.router.navigate(["edit", id])
  }
}
