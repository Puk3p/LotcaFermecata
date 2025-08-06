import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface MenuItem {
  name: string;
  price: number;
}

interface MenuCategory {
  category: string;
  items: MenuItem[];
}

@Component({
  selector: 'app-menu-page',
  standalone: true,
  templateUrl: './menu-page.component.html',
  styleUrls: ['./menu-page.component.css'],
  imports: [CommonModule, FormsModule]
})
export class MenuPageComponent implements OnInit {
  menu: MenuCategory[] = [];
  selectedCategory: string = '';
  currentCategoryItems: MenuItem[] = [];

  cart: { name: string; price: number; quantity: number }[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.http.get<MenuCategory[]>('assets/menu.json').subscribe(data => {
      this.menu = data;
      if (data.length > 0) {
        this.selectedCategory = data[0].category;
        this.changeCategory(this.selectedCategory);
      }
    });
  }

  changeCategory(category: string) {
    this.selectedCategory = category;
    const found = this.menu.find(c => c.category === category);
    this.currentCategoryItems = found ? found.items : [];
  }

  addToCart(item: MenuItem) {
    const existing = this.cart.find(i => i.name === item.name);
    if (existing) {
      existing.quantity += 1;
    } else {
      this.cart.push({ name: item.name, price: item.price, quantity: 1 });
    }
  }

  removeItem(name: string) {
    this.cart = this.cart.filter(i => i.name !== name);
  }
}
