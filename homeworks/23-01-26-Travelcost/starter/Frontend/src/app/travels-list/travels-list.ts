import { Component, inject, signal, WritableSignal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Api } from '../api/api';
import { ApiConfiguration } from '../api/api-configuration';
import { TravelDetails } from '../travel-details/travel-details';
import { listAllTravels } from '../api/functions';
import { TravelListItemDto } from '../api/models';

@Component({
  selector: 'app-travels-list',
  imports: [RouterLink],
  templateUrl: './travels-list.html',
  styleUrl: './travels-list.css'
})
export class TravelsList {
  private readonly api = inject(Api);

  travelEntries : WritableSignal<TravelListItemDto[]> = signal([]);

  isLoading = signal<boolean>(false);

  error = signal<string>("");

  async ngOnInit() {
    this.isLoading.set(true);
    
    this.travelEntries.set(await this.api.invoke(listAllTravels, (e : any) => {
      this.error.set(e);
    }));

    this.isLoading.set(false);
  }
}
