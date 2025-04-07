import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';

interface NgLetContext<T> {
  $implicit: T;
  ngLet: T;
}

/**
 * NgLet directive - similar to *ngIf but doesn't have a condition
 * It just assigns the result of an expression to a variable
 */
@Directive({
  selector: '[ngLet]',
  standalone: true,
})
export class NgLetDirective<T> {
  private _context: NgLetContext<T> = {
    $implicit: null as any,
    ngLet: null as any,
  };

  @Input()
  set ngLet(value: T) {
    this._context.$implicit = this._context.ngLet = value;
    this.viewContainerRef.clear();
    this.viewContainerRef.createEmbeddedView(this.templateRef, this._context);
  }

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<NgLetContext<T>>
  ) {}
}
