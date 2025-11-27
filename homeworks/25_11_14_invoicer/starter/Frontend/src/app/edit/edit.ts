import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';
import { Employee, Project, TimeEntry, TimeEntryUpdateReq } from '../api/models';
import { getAllEmployees, getAllProjects, getTimeentryById, updateTimeentryById } from '../api/functions';
import { FormsModule } from "@angular/forms";

@Component({
  selector: 'app-edit',
  imports: [FormsModule],
  templateUrl: './edit.html',
  styleUrl: './edit.css',
})
export class Edit implements OnInit {
  route = inject(ActivatedRoute)
  router = inject(Router)

  api = inject(Api)
  apiConfig = inject(ApiConfiguration)

  timeEntry = signal<TimeEntry|undefined>(undefined)

  description = signal<string>("")
  startTime = signal<string>("")
  endTime = signal<string>("")
  date = signal<string>("")

  employee = signal<number>(0)
  project = signal<number>(0)

  allEmployees = signal<Employee[]>([])
  allProjects = signal<Project[]>([])

  async ngOnInit() {
    this.apiConfig.rootUrl=environment.apiBaseUrl

    const editId = this.route.snapshot.params['id']

    this.timeEntry.set(await this.api.invoke(getTimeentryById, {
      id: editId
    }))

    if(!this.timeEntry()){
      this.router.navigate(["/"])
    }

    this.allEmployees.set((await this.api.invoke(getAllEmployees)).sort(e => e.id!))
    this.allProjects.set((await this.api.invoke(getAllProjects)).sort(p => p.id!))

    this.description.set(this.timeEntry()!.description!)
    this.startTime.set(this.timeEntry()!.startTime!)
    this.endTime.set(this.timeEntry()!.endTime!)
    this.employee.set(this.timeEntry()!.employeeId!)
    this.project.set(this.timeEntry()!.projectId!)
    this.date.set(this.timeEntry()!.date!)
  }

  async onSave(){
    console.log(this.employee(), this.allEmployees())
    const updateReq : TimeEntryUpdateReq = {
      date: this.date(),
      description: this.description(),
      employee: {
        employeeId: this.allEmployees().find(e => e.id == this.employee())?.emplyeeId ?? "",
        employeeName: this.allEmployees().find(e => e.id == this.employee())?.employeeName ?? ""
      },
      project: {
        projectCode: this.allProjects().find(p =>p.id == this.project())?.projectCode ?? ""
      },
      startTime: this.startTime().slice(0, 5),
      endTime: this.endTime().slice(0, 5)
    }

    await this.api.invoke(updateTimeentryById, {
      id: this.timeEntry()?.id!,
      body: updateReq
    })

    this.router.navigate(["/"])
  }

  navigateHome(){
    this.router.navigate(["/"])
  }
}
