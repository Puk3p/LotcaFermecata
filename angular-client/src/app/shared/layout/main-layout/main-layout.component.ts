import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { BreakpointObserver, Breakpoints, LayoutModule } from '@angular/cdk/layout';
import { NgIf } from '@angular/common';
import { Observable, map, shareReplay } from 'rxjs';
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
  showDropdown = false;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private push: PushService // <-- injectat
  ) {
    this.isHandset$ = this.breakpointObserver.observe([Breakpoints.Handset]).pipe(
      map(result => result.matches),
      shareReplay()
    );

    // citește rolul + userId
    const role = localStorage.getItem('role');
    if (role === 'BAR' || role === 'BUCATARIE') {
      this.currentRole = role;
    }

    const userId = localStorage.getItem('userId') || 'anon';

    // pornește Web Push (o singură dată)
    this.push.init(this.currentRole, userId);
  }

  toggleRole(): void {
    this.currentRole = this.currentRole === 'BAR' ? 'BUCATARIE' : 'BAR';
    localStorage.setItem('role', this.currentRole);
    this.showDropdown = false;

    // anunță backend-ul că subscription-ul aparține noului rol
    const userId = localStorage.getItem('userId') || 'anon';
    this.push.init(this.currentRole, userId);
    // alternativ, poți avea un endpoint separat de "update role for subscription"
  }

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }
}
