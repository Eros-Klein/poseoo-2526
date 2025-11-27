import { Component, inject, model, OnInit, signal } from '@angular/core';
import { Employee, Project } from '../api/models';
import { FormsModule } from '@angular/forms';
import { getAllEmployees, getAllProjects } from '../api/functions';
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-time-entry-list-control-panel',
  imports: [FormsModule],
  templateUrl: './time-entry-list-control-panel.html',
  styleUrl: './time-entry-list-control-panel.css',
})
export class TimeEntryListControlPanel implements OnInit {
  private api = inject(Api)
  private apiConfiguration = inject(ApiConfiguration)

  async ngOnInit() {
    this.apiConfiguration.rootUrl = environment.apiBaseUrl;

    this.allEmployees.set(await this.api.invoke(getAllEmployees));
    this.allProjects.set(await this.api.invoke(getAllProjects));
  }

  allEmployees = signal<Employee[]>([])
  allProjects = signal<Project[]>([])

  employees = model<string | "all">("all");
  projects = model<string | "all">("all");
  descriptions = model<string>('');


}
