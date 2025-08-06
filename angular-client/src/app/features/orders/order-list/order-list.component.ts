import { Component, OnInit } from '@angular/core';
import { Order } from '../order.model';
import { OrderService } from '../order.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms'; // ✅ adăugat

@Component({
  selector: 'app-order-list',
  standalone: true,
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.css'],
  imports: [
    CommonModule,
    RouterModule,
    FormsModule
  ]
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  selectedZone = 'Pool';

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.orderService.getAll(this.selectedZone).subscribe(data => {
      this.orders = data;
    });
  }

  onZoneChange(): void {
    this.loadOrders();
  }
}
