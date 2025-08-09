import { Component, OnInit } from '@angular/core';
import { OrderService } from '../order.service';
import { Order } from '../order.model';
import { CommonModule } from '@angular/common';
import { OrderListComponent } from '../order-list/order-list.component';

@Component({
  selector: 'app-orders-completed',
  templateUrl: './orders-completed.component.html',
  styleUrls: ['./orders-completed.component.css'],
  standalone: true,
  imports: [CommonModule, OrderListComponent]

})
export class OrdersCompletedComponent implements OnInit {
  orders: Order[] = [];

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.orderService.getCompletedOrders().subscribe(data => {
      this.orders = data;
    });
  }
}
