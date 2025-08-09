import { Component, OnInit } from '@angular/core';
import { OrderService } from '../order.service';
import { Order } from '../order.model';
import { CommonModule } from '@angular/common';
import { OrderListComponent } from '../order-list/order-list.component';

@Component({
  selector: 'app-orders-canceled',
  templateUrl: './orders-cancelled.component.html',
  styleUrls: ['./orders-cancelled.component.css'],
  standalone: true,
  imports: [CommonModule, OrderListComponent]
})
export class OrdersCanceledComponent implements OnInit {
  orders: Order[] = [];

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.orderService.getCancelledOrders().subscribe(data => {
      this.orders = data;
    });
  }
}
