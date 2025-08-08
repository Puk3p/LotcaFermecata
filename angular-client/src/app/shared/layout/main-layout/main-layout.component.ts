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

  constructor(private breakpointObserver: BreakpointObserver) {
    this.isHandset$ = this.breakpointObserver.observe([Breakpoints.Handset]).pipe(
      map(result => result.matches),
      shareReplay()
    );

    const role = localStorage.getItem('role');
    if (role === 'BAR' || role === 'BUCATARIE') {
      this.currentRole = role;
    }
  }

  toggleRole(): void {
    this.currentRole = this.currentRole === 'BAR' ? 'BUCATARIE' : 'BAR';
    localStorage.setItem('role', this.currentRole);
    this.showDropdown = false;
  }

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }
}