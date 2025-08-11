import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// ---- DTO-uri exact ca în backend ----
export interface KpiDto {
  label: string;
  value: number;
  trend: number; // procent +/-
  icon: string;
}

export interface ProductTrendDto {
  name: string;
  count: number;
  trendPercent: number;
}

export type ActivityType = 'pending' | 'done' | 'cancelled';

export interface ActivityDto {
  title: string;
  badge: string;
  type: ActivityType;
  when: string; // ora sau data+ora formatată
  zone: string;
}

export interface ZoneStatDto {
  zone: string;
  orders: number;
  percentOfTotal: number;
  peakHour?: number | null;
  peakHourOrders?: number | null;
}

export interface DashboardDto {
  kpis: KpiDto[];
  productOfDay?: ProductTrendDto | null;
  recent: ActivityDto[];
  peakZones: ZoneStatDto[];
  peakHours: HourStatDto[];
}

export interface HourStatDto {
  hour: number;
  orders: number;
  percentOfTotal: number;
}

export interface DailyCloseTotalsDto {
  ordersPaid: number;
  itemsPaid: number;
  revenue?: number | null;
  paymentMethods: Record<string, number>;
}
export interface DailyCloseLineDto {
  productName: string;
  ordersCount: number;
  quantity: number;
  unitPrice?: number | null;
  totalPaid?: number | null;
}
export interface DailyCloseDto {
  date: string;
  lines: DailyCloseLineDto[];
  totals: DailyCloseTotalsDto;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private baseUrl = '/api/dashboard'; // setat prin proxy la backend

  constructor(private http: HttpClient) {}

  /**
   * Returnează dashboard-ul zilei curente cu datele filtrate pe rol
   * @param role BAR | BUCATARIE
   */
  getTodayDashboard(role: 'BAR' | 'BUCATARIE'): Observable<DashboardDto> {
    return this.http.get<DashboardDto>(`${this.baseUrl}/today`);
  }

  getDailyCloseout(date: string) {
    return this.http.get<DailyCloseDto>(`/api/archive/closeout?date=${date}`);
  }
  getCloseoutRange(days = 7) {
    return this.http.get<DailyCloseDto[]>(`/api/archive/closeout-range?days=${days}`);
  }
}
