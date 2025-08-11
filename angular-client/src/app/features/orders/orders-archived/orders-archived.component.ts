import { Component, OnInit } from '@angular/core';
import { OrderService } from '../order.service';
import { Order } from '../order.model';
import { CommonModule } from '@angular/common';
import { OrderListComponent } from '../order-list/order-list.component';

type CloseLine = {
  productName: string;
  ordersCount: number;
  quantity: number;
  unitPrice?: number | null;
  totalPaid?: number | null;
};

type CloseTotals = {
  ordersPaid: number;
  itemsPaid: number;
  revenue?: number | null;
  paymentMethods: Record<string, number>;
};

type CloseOut = { lines: CloseLine[]; totals: CloseTotals };

@Component({
  selector: 'app-orders-archived',
  standalone: true,
  templateUrl: './orders-archived.component.html',
  styleUrls: ['./orders-archived.component.css'],
  imports: [CommonModule, OrderListComponent],
})
export class OrdersArchivedComponent implements OnInit {
  groupedOrders: { [date: string]: Order[] } = {};
  private closeCache = new Map<string, CloseOut>();

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.orderService.getArchivedOrders().subscribe((data) => {
      this.groupedOrders = data ?? {};
      this.closeCache.clear();
    });
  }

  getSortedDates(): string[] {
    return Object.keys(this.groupedOrders).sort(
      (a, b) => new Date(b).getTime() - new Date(a).getTime()
    );
  }

  // Alias ca să meargă și numele din template-ul vechi
  getCloseoutFor(date: string): CloseOut {
    return this.getCloseOut(date);
  }

  getCloseOut(date: string): CloseOut {
    if (this.closeCache.has(date)) return this.closeCache.get(date)!;

    const ordersInDay = this.groupedOrders[date] ?? [];
    const paid = ordersInDay.filter(
      (o) => this.isPaid(o) && this.isCompleted(o.status)
    );

    const map = new Map<string, CloseLine & { orderIds: Set<string> }>();
    for (const o of paid) {
      const items = (o.items ?? []) as any[];
      for (const it of items) {
        const name = (it.productName || it.name || 'Produs').trim();
        const qty = Number(it.quantity ?? 0) || 0;
        const unit = this.getUnitPrice(it);

        if (!map.has(name)) {
          map.set(name, {
            productName: name,
            ordersCount: 0,
            quantity: 0,
            unitPrice: unit ?? null,
            totalPaid: null,
            orderIds: new Set<string>(),
          });
        }

        const entry = map.get(name)!;
        entry.quantity += qty;
        entry.orderIds.add(String(o.id));

        if (unit != null) {
          entry.totalPaid = (entry.totalPaid ?? 0) + unit * qty;
          if (entry.unitPrice == null) entry.unitPrice = unit;
        }
      }
    }

    const lines: CloseLine[] = Array.from(map.values())
      .map((l) => ({
        productName: l.productName,
        ordersCount: l.orderIds.size,
        quantity: l.quantity,
        unitPrice: l.unitPrice ?? null,
        totalPaid: l.totalPaid ?? null,
      }))
      .sort(
        (a, b) => b.quantity - a.quantity || a.productName.localeCompare(b.productName)
      );

    const totals: CloseTotals = {
      ordersPaid: paid.length,
      itemsPaid: lines.reduce((s, l) => s + l.quantity, 0),
      revenue: lines.some((l) => l.totalPaid != null)
        ? lines.reduce((s, l) => s + (l.totalPaid ?? 0), 0)
        : null,
      paymentMethods: this.countPaymentMethods(paid),
    };

    const result: CloseOut = { lines, totals };
    this.closeCache.set(date, result);
    return result;
  }

  // Helpers pentru *ngIf (evităm arrow functions în template)
  hasAnyUnitPrice(close: CloseOut): boolean {
    return close.lines.some((l) => l.unitPrice != null);
  }
  hasAnyTotal(close: CloseOut): boolean {
    return close.lines.some((l) => l.totalPaid != null);
  }

  private isCompleted(status?: string): boolean {
    const s = (status ?? '').trim().toLowerCase();
    return s === 'done' || s === 'completed' || s === 'finalizata' || s === 'finalizată';
  }

  private isPaid(o: any): boolean {
    const ps = (o.paymentStatus || '').toString().trim().toLowerCase();
    if (ps === 'paid' || ps === 'plătit' || ps === 'platit') return true;
    if (o.isPaid === true) return true;
    return false; // sau: return this.isCompleted(o.status);
  }

  private getUnitPrice(item: any): number | null {
    const p = item.unitPrice ?? item.price ?? item.unit_price;
    const n = Number(p);
    return Number.isFinite(n) ? n : null;
  }

  private countPaymentMethods(orders: any[]): Record<string, number> {
    const dict: Record<string, number> = {};
    for (const o of orders) {
      const m = (o.paymentMethod ?? 'Necunoscut').toString();
      dict[m] = (dict[m] || 0) + 1;
    }
    return dict;
  }
}