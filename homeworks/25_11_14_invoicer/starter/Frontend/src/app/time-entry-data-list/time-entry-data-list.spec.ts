import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeEntryDataList } from './time-entry-data-list';

describe('TimeEntryDataList', () => {
  let component: TimeEntryDataList;
  let fixture: ComponentFixture<TimeEntryDataList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TimeEntryDataList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TimeEntryDataList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
