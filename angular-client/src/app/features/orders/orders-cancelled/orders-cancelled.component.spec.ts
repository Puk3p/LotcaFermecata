import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrdersCancelledComponent } from './orders-cancelled.component';

describe('OrdersCancelledComponent', () => {
  let component: OrdersCancelledComponent;
  let fixture: ComponentFixture<OrdersCancelledComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrdersCancelledComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OrdersCancelledComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
