import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobSchedulerComponent } from './job-scheduler.component';

describe('JobSchedulerComponent', () => {
  let component: JobSchedulerComponent;
  let fixture: ComponentFixture<JobSchedulerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobSchedulerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobSchedulerComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
