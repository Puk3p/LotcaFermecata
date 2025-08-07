import { Component, OnInit } from '@angular/core';
import { Order } from '../order.model';
import { OrderService } from '../order.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../auth/auth.service';

@Component({
  selector: 'app-order-list',
  standalone: true,
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.css'],
  imports: [CommonModule, RouterModule, FormsModule]
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  currentUserId = '';
  currentRole: 'BAR' | 'BUCATARIE' = 'BAR';

  statusLabels: { [key: string]: string } = {
    Pending: 'În așteptare',
    InProgress: 'Se prepară',
    Done: 'Finalizată',
    Cancelled: 'Anulată'
  };


  constructor(
    private orderService: OrderService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const roleFromStorage = localStorage.getItem('role') as 'BAR' | 'BUCATARIE';
    this.currentRole = roleFromStorage || 'BAR';

    const zone = this.currentRole === 'BAR' ? 'Pool' : 'Kitchen';

    this.orderService.getOrdersForZone(zone).subscribe({
      next: (orders) => {
        this.orders = orders;
      },
      error: (err) => {
        console.error('Eroare la încărcarea comenzilor:', err);
      }
    });
  }

  getLocalTime(date: string | Date): string {
    const original = new Date(date);
    const localTime = new Date(original.getTime() + 3 * 60 * 60 * 1000);
    return localTime.toLocaleString('ro-RO', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
