import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentAuthComponent } from './student-auth.component';
import { provideHttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

describe('StudentAuthComponent', () => {
  let component: StudentAuthComponent;
  let fixture: ComponentFixture<StudentAuthComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudentAuthComponent],
      providers: [provideHttpClient(), { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => {} } } } }],
      
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentAuthComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
