import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { OrderService } from '../order.service';
import { Order } from '../order.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-order-details',
  standalone: true,
  templateUrl: './order-details.component.html',
  styleUrls: ['./order-details.component.css'],
  imports: [CommonModule, RouterModule, FormsModule]
})
export class OrderDetailsComponent implements OnInit {
  order?: Order;
  newStatus = '';

  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.orderService.getById(id).subscribe(data => {
      this.order = data;
      this.newStatus = data.status;
    });
  }

  updateStatus(): void {
    if (!this.order) return;

    this.orderService.updateStatus(this.order.id, this.newStatus).subscribe(success => {
      if (success) {
        this.order!.status = this.newStatus;
        alert('Status updated successfully');
      } else {
        alert('Failed to update status');
      }
    });
  }
}
