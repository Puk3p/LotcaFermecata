import { Component, OnInit } from '@angular/core';
import { AuthService } from '../auth.service';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { PushService } from '../../core/push.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  username = '';
  password = '';
  selectedRole: 'BAR' | 'BUCATARIE' = 'BAR';

  isLoggedIn = false;
  showSuccessScreen = false;

  constructor(
    private auth: AuthService,
    private router: Router,
    private push: PushService
  ) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (token) {
      this.isLoggedIn = true;
      this.showSuccessScreen = true;

      // 🔧 user e deja logat (refresh): resincronizează subscripția existentă
      const userId = localStorage.getItem('userId') || 'anon';
      const role = (localStorage.getItem('role') as 'BAR'|'BUCATARIE') || 'BAR';
      this.push.resyncPush(userId, role); // non-blocking, fără prompt
    }
  }

  onLogin() {
    this.auth.login({ username: this.username, password: this.password }).subscribe({
      next: (res) => {
        this.auth.saveToken(res.token);
        localStorage.setItem('role', this.selectedRole);
        localStorage.setItem('userId', this.username);

        // ✅ după login cerem permisiunea și facem subscribe
        this.push.init(this.selectedRole, this.username)
          .catch(err => console.warn('[Push] init after login failed', err));

        this.router.navigate(['/']);
      },
      error: () => alert('Login greșit!')
    });
  }

  goToDashboard() {
    this.router.navigate(['/']);
  }
}
