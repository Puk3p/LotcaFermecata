import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrdersArchivedComponent } from './orders-archived.component';

describe('OrdersArchivedComponent', () => {
  let component: OrdersArchivedComponent;
  let fixture: ComponentFixture<OrdersArchivedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrdersArchivedComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OrdersArchivedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
