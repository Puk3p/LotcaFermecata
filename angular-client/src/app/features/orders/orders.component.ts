import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Order, ArchivedGrouped, ArchivedDay } from './order.model';
import { OrderService } from './order.service';
import { OrderListComponent } from './order-list/order-list.component';

type TabType = 'active' | 'completed' | 'canceled' | 'archive';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterModule, OrderListComponent],
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.css']
})
export class OrdersComponent implements OnInit {
  currentOrders: Order[] = [];

  // Obiectul de la backend cu orders + summary + totals pe zi
  archivedGrouped: ArchivedGrouped = {};

  activeCount = 0;
  completedCount = 0;
  canceledCount = 0;
  archivedCount = 0;

  selectedTab: TabType = 'active';

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const tabParam = (params.get('tab') as TabType) || 'active';
      this.selectedTab = this.isValidTab(tabParam) ? tabParam : 'active';
      this.loadForTab(this.selectedTab);
    });

    this.prefetchCounts();
  }

  private isValidTab(tab: any): tab is TabType {
    return ['active', 'completed', 'canceled', 'archive'].includes(tab);
  }

  selectTab(tab: TabType) {
    this.router.navigate(['/orders', tab]);
  }

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
      this.archivedCount = Object.values(this.archivedGrouped)
        .reduce((sum, day) => sum + (day?.orders?.length || 0), 0);
    });
  }

  private prefetchCounts() {
    this.orderService.getActiveOrders().subscribe(l => this.activeCount = l.length);
    this.orderService.getCompletedOrders().subscribe(l => this.completedCount = l.length);
    this.orderService.getCancelledOrders().subscribe(l => this.canceledCount = l.length);
    this.orderService.getArchivedOrders().subscribe(g => {
      const total = Object.values(g || {}).reduce((s, day) => s + (day?.orders?.length || 0), 0);
      this.archivedCount = total;
    });
  }

  getArchivedDates(): string[] {
    return Object.keys(this.archivedGrouped).sort((a, b) =>
      new Date(b).getTime() - new Date(a).getTime()
    );
  }

  // Helpers pt. template (evităm expresii complicate în HTML)
  hasAnyUnitPrice(day: ArchivedDay | undefined | null): boolean {
    if (!day?.summary) return false;
    for (const l of day.summary) if (l.unitPrice != null) return true;
    return false;
  }

  hasAnyTotal(day: ArchivedDay | undefined | null): boolean {
    if (!day?.summary) return false;
    for (const l of day.summary) if (l.totalPaid != null) return true;
    return false;
  }
}
  