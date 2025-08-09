// orders.component.ts
import { Component, OnInit } from '@angular/core';
import { Order } from './order.model';
import { OrderService } from './order.service';
import { CommonModule } from '@angular/common';
import { OrderListComponent } from './order-list/order-list.component';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

type TabType = 'active' | 'completed' | 'canceled' | 'archive';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterModule, OrderListComponent],
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.css']
})
export class OrdersComponent implements OnInit {
  // lista curentă pentru taburile non-archive
  currentOrders: Order[] = [];

  // pentru arhivă (grupată pe zile)
  archivedGrouped: { [date: string]: Order[] } = {};

  // pentru badge-uri/număr (opțional)
  activeCount = 0;
  completedCount = 0;
  canceledCount = 0;
  archivedCount = 0;

  selectedTab: TabType = 'active';

  constructor(private orderService: OrderService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const tabParam = (params.get('tab') as TabType) || 'active';
      this.selectedTab = this.isValidTab(tabParam) ? tabParam : 'active';
      this.loadForTab(this.selectedTab);
    });

    // (opțional) poți popula și contorii în background
    this.prefetchCounts();
  }

  private isValidTab(tab: any): tab is TabType {
    return ['active', 'completed', 'canceled', 'archive'].includes(tab);
  }

  selectTab(tab: TabType) {
    this.router.navigate(['/orders', tab]);
  }

  // AICI e magia: fiecare tab -> endpoint-ul lui din OrderService
  loadForTab(tab: TabType) {
    if (tab === 'active') {
      this.orderService.getActiveOrders().subscribe(list => this.currentOrders = list);
      return;
    }
    if (tab === 'completed') {
      this.orderService.getCompletedOrders().subscribe(list => this.currentOrders = list);
      return;
    }
    if (tab === 'canceled') {
      this.orderService.getCancelledOrders().subscribe(list => this.currentOrders = list);
      return;
    }
    // archive
    this.orderService.getArchivedOrders().subscribe(grouped => {
      this.archivedGrouped = grouped || {};
      this.archivedCount = Object.values(this.archivedGrouped).reduce((sum, arr) => sum + arr.length, 0);
    });
  }

  // (opțional) doar ca să afișezi cifrele pe tab-uri fără a depinde de tabul curent
  private prefetchCounts() {
    this.orderService.getActiveOrders().subscribe(l => this.activeCount = l.length);
    this.orderService.getCompletedOrders().subscribe(l => this.completedCount = l.length);
    this.orderService.getCancelledOrders().subscribe(l => this.canceledCount = l.length);
    this.orderService.getArchivedOrders().subscribe(g => {
      const total = Object.values(g || {}).reduce((s, arr) => s + arr.length, 0);
      this.archivedCount = total;
    });
  }

  getArchivedDates(): string[] {
    return Object.keys(this.archivedGrouped).sort((a, b) =>
      new Date(b).getTime() - new Date(a).getTime()
    );
  }
}
