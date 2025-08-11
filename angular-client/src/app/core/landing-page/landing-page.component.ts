import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import {
  ActivityDto,
  DashboardDto,
  DashboardService,
  KpiDto,
  ProductTrendDto,
  ZoneStatDto,
  HourStatDto
} from '../../features/dashboard/dashboard.service';
import { CommonModule } from '@angular/common';
import { toRoStatus } from '../../shared/status.map';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.scss'],
  imports: [CommonModule]
})
export class LandingPageComponent implements OnInit {
  todayString = '';
  displayName = '';
  roleLabel = '';

  kpis: KpiDto[] = [];
  recent: ActivityDto[] = [];

  // 🔧 adăugate: folosite de template
  productOfDay: ProductTrendDto | null | undefined;
  peakZones: ZoneStatDto[] = [];
  peakHours: HourStatDto[] = [];

  quickActions = [
    { title: 'Comenzi active',  desc: 'Vezi toate comenzile în desfășurare', icon: 'fa-bolt', href: '/orders/active' },
    { title: 'Finalizate azi',  desc: 'Vezi comenzile închise',             icon: 'fa-circle-check', href: '/orders/completed' },
    { title: 'Anulate',         desc: 'Vezi comenzile anulate',              icon: 'fa-ban', href: '/orders/cancelled' } // <- 'cancelled'
  ];

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    const now = new Date();
    this.todayString = now.toLocaleDateString('ro-RO', {
      weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
    });

    const user = this.authService.getCurrentUser();
    this.displayName = user?.username || 'Utilizator';
    this.roleLabel   = (user?.role === 'BAR') ? 'BAR' : 'BUCĂTĂRIE';

    this.loadDashboard();
  }

  private clamp(v: number, lim = 300) { return Math.max(-lim, Math.min(lim, v)); }
  private fmtHour(h?: number | null): string {
    if (h == null || h === undefined) return '-';
    return `${h.toString().padStart(2, '0')}:00`;
  }

  loadDashboard() {
    const role = (this.authService.getCurrentUser()?.role || 'BAR') as 'BAR' | 'BUCATARIE';
    this.dashboardService.getTodayDashboard(role).subscribe({
      next: (data: DashboardDto) => {
        // opțional: limitează trendurile pentru UI
        this.kpis = (data.kpis || []).map(k => ({ ...k, trend: this.clamp(k.trend) }));
        this.recent = (data.recent || []).map(r => {
          const ro = toRoStatus(r.badge);
          const newTitle = r.title.replace(/\s-\s[^-]+$/, ` - ${ro}`);
          return { ...r, badge: ro, title: newTitle };
        });

        this.productOfDay = data.productOfDay
          ? { ...data.productOfDay, trendPercent: this.clamp(data.productOfDay.trendPercent) }
          : null;
        
          this.peakZones = (data.peakZones || []).map(z => ({
            ...z,
            percentOfTotal: Math.max(0, Math.min(100, z.percentOfTotal ?? 0))
          }));
          
          this.peakHours = (data.peakHours || []).map(h => ({
            ...h,
            hour: (h.hour + 3) % 24,
            percentOfTotal: Math.max(0, Math.min(100, h.percentOfTotal ?? 0))
          }));

          const order = ['Comenzi azi', 'Active acum', 'Finalizate', 'Anulate'];
          this.kpis.sort((a,b) =>  order.indexOf(a.label) - order.indexOf(b.label));

        console.log('📊 Dashboard data:', data);
      },
      error: (err) => console.error('Eroare dashboard:', err)
    });
  }

  refresh() { this.loadDashboard(); }

  goTo(path: string) { this.router.navigateByUrl(path); }

  fmt(h?: number | null) { return this.fmtHour(h); }
}
