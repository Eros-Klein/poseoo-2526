import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeEntryListControlPanel } from './time-entry-list-control-panel';

describe('TimeEntryListControlPanel', () => {
  let component: TimeEntryListControlPanel;
  let fixture: ComponentFixture<TimeEntryListControlPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TimeEntryListControlPanel]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TimeEntryListControlPanel);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
