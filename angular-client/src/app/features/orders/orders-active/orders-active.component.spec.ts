import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrdersActiveComponent } from './orders-active.component';

describe('OrdersActiveComponent', () => {
  let component: OrdersActiveComponent;
  let fixture: ComponentFixture<OrdersActiveComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrdersActiveComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OrdersActiveComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
