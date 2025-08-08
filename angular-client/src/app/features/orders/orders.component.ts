import { Component, OnInit } from '@angular/core';
import { Order } from './order.model';
import { OrderService } from './order.service';
import { CommonModule } from '@angular/common';
import { OrderListComponent } from './order-list/order-list.component';
import { ActivatedRoute, Router } from '@angular/router';

type TabType = 'active' | 'completed' | 'canceled' | 'archive';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, OrderListComponent],
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.css']
})
export class OrdersComponent implements OnInit {
  selectedTab: TabType = 'active';

  activeOrders: Order[] = [];
  completedOrders: Order[] = [];
  canceledOrders: Order[] = [];
  archivedGrouped: { [date: string]: Order[] } = {};

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const tabParam = params['tab'];
      const validatedTab: TabType = this.isValidTab(tabParam) ? tabParam : 'active';

      console.log('[ngOnInit] Parametru tab:', tabParam);
      console.log('[ngOnInit] Tab validat:', validatedTab);

      this.selectTab(validatedTab);
    });
  }

  selectTab(tab: TabType) {
    console.log('[selectTab] Ai apăsat pe tab:', tab);
    this.selectedTab = tab;
    this.loadOrders();
  }

  loadOrders() {
    console.log('[loadOrders] Se încarcă comenzile pentru tab:', this.selectedTab);

    switch (this.selectedTab) {
      case 'active':
        this.orderService.getActiveOrders().subscribe(data => {
          this.activeOrders = data;
        });
        break;
      case 'completed':
        this.orderService.getCompletedOrders().subscribe(data => {
          this.completedOrders = data;
        });
        break;
      case 'canceled':
        this.orderService.getCancelledOrders().subscribe(data => {
          this.canceledOrders = data;
        });
        break;
      case 'archive':
        this.orderService.getArchivedOrders().subscribe(data => {
          this.archivedGrouped = data;
        });
        break;
    }
  }

  isValidTab(tab: any): tab is TabType {
    return ['active', 'completed', 'canceled', 'archive'].includes(tab);
  }

  getCurrentTabOrders(): Order[] {
    switch (this.selectedTab) {
      case 'active': return this.activeOrders;
      case 'completed': return this.completedOrders;
      case 'canceled': return this.canceledOrders;
      default: return [];
    }
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

  getArchivedDates(): string[] {
    return Object.keys(this.archivedGrouped).sort((a, b) =>
      new Date(b).getTime() - new Date(a).getTime()
    );
  }
  
  getArchivedOrdersCount(): number {
    return Object.values(this.archivedGrouped)
      .reduce((total, orders) => total + orders.length, 0);
  }

}
