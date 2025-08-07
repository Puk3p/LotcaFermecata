import { Component } from '@angular/core';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})

export class LoginComponent {
  username = '';
  password = '';
  selectedRole: 'BAR' | 'BUCATARIE' = 'BAR'; // default

  constructor(private auth: AuthService, private router: Router) {}

  onLogin() {
    this.auth.login({ username: this.username, password: this.password }).subscribe({
      next: (res) => {
        this.auth.saveToken(res.token);
        localStorage.setItem('role', this.selectedRole);
        localStorage.setItem('userId', this.username); // salvăm username-ul introdus

        this.router.navigate(['/orders']);
      },
      error: () => alert('Login greșit!')
    });
  }
}
