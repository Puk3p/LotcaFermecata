import { Component, OnInit } from '@angular/core';
import { OrderService } from '../order.service';
import { Order } from '../order.model';
import { CommonModule } from '@angular/common';
import { OrderListComponent } from '../order-list/order-list.component';

@Component({
  selector: 'app-orders-active',
  standalone: true,
  imports: [CommonModule, OrderListComponent],
  templateUrl: './orders-active.component.html',
  styleUrls: ['./orders-active.component.css']
})
export class OrdersActiveComponent implements OnInit {
  activeOrders: Order[] = [];

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadActiveOrders();
  }

  loadActiveOrders() {
    this.orderService.getActiveOrders().subscribe({
      next: (orders) => {
        this.activeOrders = orders;
      },
      error: (err) => {
        console.error('Eroare la încărcarea comenzilor active:', err);
      }
    });
  }
}
