import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { RouterOutlet, provideRouter } from '@angular/router';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have the 'gradebook' title`, () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app.title).toEqual('gradebook');
  });

  it('should render router-outlet', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const routerOutlet = compiled.querySelector('router-outlet');
    expect(routerOutlet).not.toBeNull();
  });

  it('should have RouterOutlet imported', () => {
    // Verify component uses RouterOutlet by checking if it renders correctly
    const fixture = TestBed.createComponent(AppComponent);
    const routerOutletDebugEl = fixture.debugElement.query(
      By.directive(RouterOutlet)
    );
    expect(routerOutletDebugEl).toBeTruthy();
  });

  it('should be a standalone component', () => {
    // Check if component initializes correctly as a standalone component
    const fixture = TestBed.createComponent(AppComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
