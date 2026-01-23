import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Api } from '../api/api';
import { DatePipe, DecimalPipe } from '@angular/common';

import { ActivatedRoute } from '@angular/router';
import { getDetail } from '../api/functions';
import { TravelDetailsDto } from '../api/models';

@Component({
  selector: 'app-travel-details',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './travel-details.html',
  styleUrl: './travel-details.css'
})
export class TravelDetails implements OnInit {
  private readonly api = inject(Api);
  private readonly route = inject(ActivatedRoute);

  details = signal<TravelDetailsDto | null>(null);
  error = signal<string>("");

  public async ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam === null) {
      this.error.set("No Id in path");
    }
    const id = Number(idParam);

    this.details.set(await this.api.invoke(getDetail, {
      id: id
    }));
  }
}
