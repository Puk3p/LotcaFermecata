import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CreateOrder, OrderItem, MenuCategory, MenuItem } from '../order.model';
import { OrderService } from '../order.service';
import { MenuService } from '../menu.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-order-create',
  standalone: true,
  templateUrl: './order-create.component.html',
  styleUrls: ['./order-create.component.css'],
  imports: [CommonModule, FormsModule, HttpClientModule]
})
export class OrderCreateComponent implements OnInit {
  clientName = '';
  clientPhone = '';
  targetZone = 'Pool';

  menu: MenuCategory[] = [];
  selectedCategory: string = '';
  cart: { name: string; price: number; quantity: number }[] = [];

  constructor(
    private orderService: OrderService,
    private router: Router,
    private menuService: MenuService
  ) {}

  ngOnInit(): void {
    this.menuService.getMenu().subscribe(data => {
      this.menu = data;
      if (this.menu.length > 0) {
        this.selectedCategory = this.menu[0].category;
      }
    });
  }

  changeCategory(category: string) {
    this.selectedCategory = category;
  }

  get currentCategory(): MenuCategory | undefined {
    return this.menu.find(cat => cat.category === this.selectedCategory);
  }

  addToCart(item: MenuItem) {
    const found = this.cart.find(p => p.name === item.name);
    if (found) {
      found.quantity += 1;
    } else {
      this.cart.push({ ...item, quantity: 1 });
    }
  }

  removeFromCart(name: string) {
    this.cart = this.cart.filter(i => i.name !== name);
  }

  createOrder() {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Trebuie să fii autentificat pentru a trimite comanda!');
      return;
    }

    const items: OrderItem[] = this.cart.map(i => ({
      productName: i.name,
      quantity: i.quantity
    }));

    const payload: CreateOrder = {
      clientName: this.clientName,
      clientPhone: this.clientPhone,
      targetZone: this.targetZone,
      items: items,
      placedByUserId: localStorage.getItem('userId') || 'anonymous'
    };

    this.orderService.create(payload).subscribe({
      next: () => {
        this.router.navigate(['/orders']);
      },
      error: (err) => {
        console.error('Eroare la creare comandă:', err);
        alert('Comanda nu a putut fi trimisă. Verifică autentificarea sau reîncearcă.');
      }
    });
  }


  getTotal(): number {
    return this.cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  getCategoryIcon(category: string): string {
    switch (category) {
      case 'Cafea & Răcoritoare': return 'fas fa-coffee';
      case 'Energizante & Cidru': return 'fas fa-bolt';
      case 'Bere': return 'fas fa-beer';
      case 'Bere fără alcool': return 'fas fa-ban';
      case 'Tărie': return 'fas fa-glass-whiskey';
      case 'Vinuri & Spumant': return 'fas fa-wine-glass';
      case 'Mâncare': return 'fas fa-utensils';
      case 'Garnituri & Salate': return 'fas fa-leaf';
      case 'Sosuri & Condimente': return 'fas fa-pepper-hot';
      case 'Desert': return 'fas fa-ice-cream';
      default: return 'fas fa-question';
    }
  }

}
