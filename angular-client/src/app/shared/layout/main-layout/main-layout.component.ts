import { Component } from '@angular/core';
import { NavigationEnd, Router, RouterModule, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { BreakpointObserver, Breakpoints, LayoutModule } from '@angular/cdk/layout';
import { Observable, map, shareReplay, filter, startWith } from 'rxjs';
import { ClickOutsideDirective } from '../../directives/click-outside.directive';

import {
  trigger,
  state,
  style,
  transition,
  animate
} from '@angular/animations';
import { PushService } from '../../../core/push.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    LayoutModule,
    ClickOutsideDirective
  ],
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.css'],
  animations: [
    trigger('dropdownAnimation', [
      state('void', style({ opacity: 0, transform: 'translateY(-10px)' })),
      state('*', style({ opacity: 1, transform: 'translateY(0)' })),
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-10px)' }),
        animate('200ms ease-out')
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ opacity: 0, transform: 'translateY(-10px)' }))
      ])
    ])
  ]
})
export class MainLayoutComponent {
  isHandset$!: Observable<boolean>;
  currentRole: 'BAR' | 'BUCATARIE' = 'BAR';
  isOnDashboard$!: Observable<boolean>;
  isAuthRoute$!: Observable<boolean>;
  isOrdersPage$!: Observable<boolean>;
  showDropdown = false;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private push: PushService,
    private router: Router
  ) {
    this.isHandset$ = this.breakpointObserver.observe([Breakpoints.Handset]).pipe(
      map(result => result.matches),
      shareReplay(1)
    );

    this.isOnDashboard$ = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map(() => this.router.url === '/' || this.router.url.startsWith('/dashboard')),
      startWith(this.router.url === '/' || this.router.url.startsWith('/dashboard'))
    );

    // 🔹 detectăm dacă suntem pe login/register
    this.isAuthRoute$ = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map(() => this.router.url.startsWith('/auth')),
      startWith(this.router.url.startsWith('/auth'))
    );

    this.isOrdersPage$ = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map(() => this.router.url.startsWith('/orders')),
      startWith(this.router.url.startsWith('/orders'))
    );
    // citește rolul + userId
    const role = localStorage.getItem('role');
    if (role === 'BAR' || role === 'BUCATARIE') this.currentRole = role;

    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId') || '';

    if (token && userId) {
      this.push.resyncPush(userId, this.currentRole);
    }
  }

  toggleRole(): void {
    this.currentRole = this.currentRole === 'BAR' ? 'BUCATARIE' : 'BAR';
    localStorage.setItem('role', this.currentRole);
    this.showDropdown = false;

    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId') || '';
    if (token && userId) {
      this.push.resyncPush(userId, this.currentRole);
    }
  }

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }
}
