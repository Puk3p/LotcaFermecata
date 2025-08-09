import { Component, Input, OnInit } from '@angular/core';
import { Order } from '../order.model';
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
  @Input() orders: Order[] = [];
  @Input() tab: 'active' | 'completed' | 'canceled' | 'archive' = 'active';
  
  currentRole: 'BAR' | 'BUCATARIE' = 'BAR';

  statusLabels: { [key: string]: string } = {
    Pending: 'În așteptare',
    InProgress: 'Se prepară',
    Done: 'Finalizată',
    Cancelled: 'Anulată'
  };

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    const roleFromStorage = localStorage.getItem('role') as 'BAR' | 'BUCATARIE';
    this.currentRole = roleFromStorage || 'BAR';
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

  formatPhone(phone: string): string {
    if (!phone.startsWith('+40')) {
      if (phone.startsWith('0')) {
        phone = phone.substring(1);
      }
      return '+40 ' + phone;
    }
    return phone;
  }

}
