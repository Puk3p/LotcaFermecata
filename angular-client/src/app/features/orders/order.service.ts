import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateOrder, Order } from './order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = 'http://localhost:7227/api/orders'; // endpoint real backend

  constructor(private http: HttpClient) {}

  getAll(zone: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/zone/${zone}`);
  }

  getById(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${id}`);
  }

  create(order: CreateOrder): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}`, order);
  }

  updateStatus(id: string, newStatus: string): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/${id}/status`, { status: newStatus });
  }
}
