import { Component, inject, OnInit, signal } from '@angular/core';
import { TimeEntryListControlPanel } from "../time-entry-list-control-panel/time-entry-list-control-panel";
import { Employee, Project, TimeEntry } from '../api/models';
import { TimeEntryDataList } from "../time-entry-data-list/time-entry-data-list";
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';
import { getTimeentriesByOptionalFiltersEmployeeIdProjectId } from '../api/functions';

@Component({
  selector: 'app-time-entry-list',
  imports: [TimeEntryListControlPanel, TimeEntryDataList],
  templateUrl: './time-entry-list.html',
  styleUrl: './time-entry-list.css',
})
export class TimeEntryList implements OnInit {
  project = signal<string | "all">("all")
  employee = signal<string | "all">("all")
  description = signal<string>('')

  timeEntries = signal<TimeEntry[]>([])
  showTimeEntries = signal<TimeEntry[]>([])

  api = inject(Api)
  apiConfig = inject(ApiConfiguration)

  async ngOnInit() {
    this.apiConfig.rootUrl = environment.apiBaseUrl

    await this.onChange()
  }

  async onChange() {
    this.timeEntries.set(await this.api.invoke(getTimeentriesByOptionalFiltersEmployeeIdProjectId, {
      employeeId: this.employee() == "all"?"":this.employee(),
      projectId: this.project() == "all"?"": this.project()
    }))

    this.showTimeEntries.set(this.timeEntries().filter(te => te.description?.includes(this.description())))
  }

  onDescriptionChange() {
    this.showTimeEntries.set(this.timeEntries().filter(te => te.description?.includes(this.description())))
  }
}
