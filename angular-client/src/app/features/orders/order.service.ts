import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateOrder, GroupedOrdersDto, Order } from './order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = '/api/Order';

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

  updateStatus(id: string, newStatus: string): Observable<{ success: boolean }> {
    const body = {
      OrderId: id,
      NewStatus: newStatus
    };

    return this.http.patch<{ success: boolean }>(`${this.apiUrl}/update-status`, body);
  }

  getOrdersForZone(zone: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/zone/${zone}`);
  }

  updateOrder(order: Order) {
    return this.http.put<{ success: boolean }>(`${this.apiUrl}/${order.id}`, order);
  }

  getAllOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}`);
  }

  getGroupedOrders() : Observable<GroupedOrdersDto> {
    return this.http.get<GroupedOrdersDto>(`${this.apiUrl}/grouped-by-date`);
  }

  getActiveOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/active`);
  }

  getCompletedOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/completed`);
  }

  getCancelledOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/cancelled`);
  }

  getArchivedOrders(): Observable<{ [key: string]: Order[] }> {
    return this.http.get<{ [key: string]: Order[] }>(`${this.apiUrl}/archived`);
  }


}
