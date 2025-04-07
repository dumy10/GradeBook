import { provideLocationMocks } from '@angular/common/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { WelcomePageComponent } from './welcome-page.component';

describe('WelcomePageComponent', () => {
  let component: WelcomePageComponent;
  let fixture: ComponentFixture<WelcomePageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideRouter([]), provideLocationMocks()],
    }).compileComponents();

    fixture = TestBed.createComponent(WelcomePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have the correct title', () => {
    const titleElement = fixture.debugElement.query(By.css('h1'));
    expect(titleElement.nativeElement.textContent).toBe('GradeTrack');
  });

  it('should have the correct subtitle', () => {
    const subtitleElement = fixture.debugElement.query(By.css('.lead'));
    expect(subtitleElement.nativeElement.textContent).toBe(
      ' Simple and effective grade management '
    );
  });

  it('should have two cards for teachers and students', () => {
    const cards = fixture.debugElement.queryAll(By.css('.card'));
    expect(cards.length).toBe(2);

    const teacherCardTitle = cards[0].query(By.css('h3')).nativeElement
      .textContent;
    const studentCardTitle = cards[1].query(By.css('h3')).nativeElement
      .textContent;

    expect(teacherCardTitle).toBe('For Teachers');
    expect(studentCardTitle).toBe('For Students');
  });

  it('should have teacher login and signup links', () => {
    const teacherCard = fixture.debugElement.query(
      By.css('.card:first-of-type')
    );
    const teacherLinks = teacherCard.queryAll(By.css('a'));

    expect(teacherLinks.length).toBe(2);
    expect(teacherLinks[0].nativeElement.textContent).toBe('Login as Teacher');
    expect(teacherLinks[0].attributes['routerLink']).toBe('/teacher');

    expect(teacherLinks[1].nativeElement.textContent).toBe(
      'Sign Up as Teacher'
    );
    expect(teacherLinks[1].attributes['routerLink']).toBe('/teacher');
  });

  it('should have student login and signup links', () => {
    const studentCard = fixture.debugElement.query(
      By.css('.card:last-of-type')
    );
    const studentLinks = studentCard.queryAll(By.css('a'));

    expect(studentLinks.length).toBe(2);
    expect(studentLinks[0].nativeElement.textContent).toBe('Login as Student');
    expect(studentLinks[0].attributes['routerLink']).toBe('/student');

    expect(studentLinks[1].nativeElement.textContent).toBe(
      'Sign Up as Student'
    );
    expect(studentLinks[1].attributes['routerLink']).toBe('/student');
  });

  it('should have teacher features list with 4 items', () => {
    const teacherFeatures = fixture.debugElement.queryAll(
      By.css('.card:first-of-type li')
    );
    expect(teacherFeatures.length).toBe(4);
    expect(teacherFeatures[0].nativeElement.textContent).toContain(
      'Create and manage multiple classes'
    );
    expect(teacherFeatures[1].nativeElement.textContent).toContain(
      'Add assignments and grade students'
    );
    expect(teacherFeatures[2].nativeElement.textContent).toContain(
      'Track student performance over time'
    );
    expect(teacherFeatures[3].nativeElement.textContent).toContain(
      'Generate reports and statistics'
    );
  });

  it('should have student features list with 4 items', () => {
    const studentFeatures = fixture.debugElement.queryAll(
      By.css('.card:last-of-type li')
    );
    expect(studentFeatures.length).toBe(4);
    expect(studentFeatures[0].nativeElement.textContent).toContain(
      'See grades for all your classes'
    );
    expect(studentFeatures[1].nativeElement.textContent).toContain(
      'Track assignment completion'
    );
    expect(studentFeatures[2].nativeElement.textContent).toContain(
      'Monitor your overall GPA'
    );
    expect(studentFeatures[3].nativeElement.textContent).toContain(
      'Receive notifications for new grades'
    );
  });
});
