import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestManagement } from './request-management';

describe('RequestManagement', () => {
  let component: RequestManagement;
  let fixture: ComponentFixture<RequestManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequestManagement],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestManagement);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
