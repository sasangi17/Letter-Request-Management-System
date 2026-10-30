import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmployeeLetter } from './employee-letter';

describe('EmployeeLetter', () => {
  let component: EmployeeLetter;
  let fixture: ComponentFixture<EmployeeLetter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmployeeLetter],
    }).compileComponents();

    fixture = TestBed.createComponent(EmployeeLetter);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
