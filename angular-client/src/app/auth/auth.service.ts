import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../features/orders/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = '/api/auth';

  constructor(private http: HttpClient) {}

  login(data: { username: string, password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, data);
  }

  register(data: { username : string, password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  saveToken(token: string) {
    localStorage.setItem('token', token);
  }

  getCurrentUser(): User {
    const token = localStorage.getItem('token');
    if (!token) throw new Error('No token found');

    const payload = JSON.parse(atob(token.split('.')[1]));

    return {
      id: payload.sub,
      username: payload.username,
      role: payload.role
    };
  }

  getCurrentRole(): 'BAR' | 'BUCATARIE' {
    return localStorage.getItem('role') as 'BAR' | 'BUCATARIE';
  }
}
