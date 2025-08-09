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
  allOrders: Order[] = [];

  activeOrders: Order[] = [];
  completedOrders: Order[] = [];
  canceledOrders: Order[] = [];

  archivedOrders: Order[] = [];
  archivedGrouped: { [date: string]: Order[] } = {};

  selectedTab: TabType = 'active';

  constructor(private orderService: OrderService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const tabParam = params['tab'];
      const newTab: TabType = this.isValidTab(tabParam) ? tabParam : 'active';

      console.log('[ngOnInit] Parametru tab:', tabParam);
      console.log('[ngOnInit] Tab validat:', newTab);

      this.selectedTab = newTab;

      this.loadOrders(newTab);
    });
  }

  async selectTab(tab: TabType) {
  console.log('[selectTab] Ai apăsat pe tab:', tab);
  this.selectedTab = tab;
  this.loadOrders(tab);
  
// await this.delay(50); 
  
  //this.loadOrders(tab);
}

// Helper function to create a delay
delay(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}


  private isValidTab(tab: any): tab is TabType {
    return ['active', 'completed', 'canceled', 'archive'].includes(tab);
  }

  loadOrders(selectedTabOverride?: TabType) {
    console.log('[loadOrders] Se încarcă comenzile...');

    this.orderService.getGroupedOrders().subscribe(grouped => {
      console.log('[loadOrders] Grupare primită din backend:', grouped);

      this.activeOrders = grouped.active || [];
      this.completedOrders = grouped.completed || [];
      this.canceledOrders = grouped.canceled || [];
      this.archivedGrouped = grouped.archivedGrouped || {};

      this.archivedOrders = Object.values(this.archivedGrouped).flat();

      if (selectedTabOverride) {
        this.selectedTab = selectedTabOverride;

        if (selectedTabOverride !== 'archive') {
          this.archivedGrouped = {};
          this.activeOrders = [];
        }
      }
    });
  }

  getCurrentTabOrders(): Order[] {
    switch (this.selectedTab) {
      case 'active': return this.activeOrders;
      case 'completed': return this.completedOrders;
      case 'canceled': return this.canceledOrders;
      default: return [];
    }
  }

  groupArchivedByDay() {
    this.archivedGrouped = {};
    this.archivedOrders.forEach(order => {
      const localDate = this.getLocalDay(order.placedAt);
      if (!this.archivedGrouped[localDate]) {
        this.archivedGrouped[localDate] = [];
      }
      this.archivedGrouped[localDate].push(order);
    });

    console.log('[groupArchivedByDay] Zile arhivate:', Object.keys(this.archivedGrouped));
  }

  getArchivedDates(): string[] {
    return Object.keys(this.archivedGrouped).sort((a, b) =>
      new Date(b).getTime() - new Date(a).getTime()
    );
  }

  getLocalDay(date: string | Date): string {
    const d = new Date(date);
    const local = new Date(d.getTime() + 3 * 60 * 60 * 1000);
    return local.toLocaleDateString('ro-RO', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  }

  getLocalDateString(date: string | Date): string {
    const d = new Date(date);
    const local = new Date(d.getTime() + 3 * 60 * 60 * 1000);
    return local.toLocaleDateString('ro-RO', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
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
