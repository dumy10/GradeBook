import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { provideRouter } from '@angular/router';
import { TeacherAuthComponent } from './teacher-auth.component';

describe('TeacherAuthComponent', () => {
  let component: TeacherAuthComponent;
  let fixture: ComponentFixture<TeacherAuthComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeacherAuthComponent, FormsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TeacherAuthComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
