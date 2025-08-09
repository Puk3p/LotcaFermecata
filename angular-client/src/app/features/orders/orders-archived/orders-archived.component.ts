import { Component, OnInit } from '@angular/core';
import { OrderService } from '../order.service';
import { Order } from '../order.model';
import { CommonModule } from '@angular/common';
import { OrderListComponent } from '../order-list/order-list.component';

@Component({
  selector: 'app-orders-archived',
  templateUrl: './orders-archived.component.html',
  styleUrls: ['./orders-archived.component.css'],
  standalone: true,
  imports: [CommonModule, OrderListComponent]
})
export class OrdersArchivedComponent implements OnInit {
  groupedOrders: { [date: string]: Order[] } = {};

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.orderService.getArchivedOrders().subscribe(data => {
      this.groupedOrders = data;
    });
  }

  getSortedDates(): string[] {
    return Object.keys(this.groupedOrders).sort((a, b) =>
      new Date(b).getTime() - new Date(a).getTime()
    );
  }
}
