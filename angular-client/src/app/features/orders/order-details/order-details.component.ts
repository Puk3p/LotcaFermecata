import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrderService } from '../order.service';
import { Order } from '../order.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Location } from '@angular/common';

@Component({
  selector: 'app-order-details',
  standalone: true,
  templateUrl: './order-details.component.html',
  styleUrls: ['./order-details.component.css'],
  imports: [CommonModule, RouterModule, FormsModule]
})
export class OrderDetailsComponent implements OnInit {
  order?: Order;
  newStatus = '';

  statusOptions = [
    { value: 'Pending', label: 'În așteptare' },
    { value: 'InProgress', label: 'În curs de preparare' },
    { value: 'Done', label: 'Finalizată' },
    { value: 'Cancelled', label: 'Anulată' }
  ];

  statusLabels: { [key: string]: string } = {
    Pending: 'În așteptare',
    InProgress: 'Se prepară',
    Done: 'Finalizată',
    Cancelled: 'Anulată'
  };

  statusClasses: { [key: string]: string } = {
    Pending: 'waiting',
    InProgress: 'cooking',
    Done: 'done',
    Cancelled: 'cancelled'
  };

  isEditing = false;
  canEdit = true;

  menu: any[] = [];
  selectedCategory: string = '';
  currentCategory: any = null;
  cart: { name: string, quantity: number, price: number }[] = [];

  selectedTab: string = 'active';

  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService,
    private location: Location,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.selectedTab = this.route.snapshot.queryParamMap.get('tab') || 'active';

    const id = this.route.snapshot.paramMap.get('id')!;

    const orderPromise = this.orderService.getById(id).toPromise();
    const menuPromise = fetch('/assets/menu.json').then(res => res.json());

    Promise.all([orderPromise, menuPromise]).then(([orderData, menuData]) => {
      if (!orderData || !menuData) {
        console.error("Datele nu au fost încărcate corect.");
        return;
      }

      this.order = orderData;
      this.newStatus = orderData.status;
      this.menu = menuData;

      if (this.menu.length > 0) {
        this.changeCategory(this.menu[0].category);
      }

      this.cart = this.order.items.map(i => ({
        name: i.productName,
        quantity: i.quantity,
        price: this.getPriceForProduct(i.productName)
      }));
    });
  }



  goBack(): void {
    this.router.navigate(['/orders'], {
      queryParams: { tab: this.selectedTab }
    });
  }

  updateStatus(): void {
    if (!this.order || !this.newStatus) return;

    this.orderService.updateStatus(this.order.id, this.newStatus).subscribe({
      next: (res) => {
        if (res.success) {
          this.order!.status = this.newStatus;
          alert('Status updated successfully');
        } else {
          alert('Failed to update status');
        }
      },
      error: (err) => {
        console.error('Error updating status:', err);
        alert('Failed to update status');
      }
    });
  }

  submitEdit(): void {
    if (!this.order) return;

    const updatedOrder: Order = {
      ...this.order,
      items: this.cart.map(i => ({ productName: i.name, quantity: i.quantity, price: i.price }))
    };

    this.orderService.updateOrder(updatedOrder).subscribe({
      next: () => {
        this.isEditing = false;
        alert('Comanda a fost actualizată');
      },
      error: () => {
        alert('Eroare la actualizarea comenzii');
      }
    });
    this.order.clientName = this.order.clientName.trim();
    this.order.clientPhone = this.order.clientPhone.trim();
    this.order.targetZone = this.order.targetZone;
    this.order.items = this.cart.map(item => ({
      productName: item.name,
      quantity: item.quantity
    }));
    this.isEditing = false;

  }

  canUpdateStatus(items: any[]): boolean {
    const restrictedKeywords = [
      'șnițel', 'pui', 'porc', 'aripioare', 'pizza', 'mici', 'ceafă',
      'cartofi', 'salată', 'sos', 'chiflă', 'papanasi', 'gogoși'
    ];

    return items.every(item =>
      !restrictedKeywords.some(keyword =>
        item.productName?.toLowerCase().includes(keyword)
      )
    );
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending': return 'select-pending';
      case 'InProgress': return 'select-cooking';
      case 'Done': return 'select-done';
      case 'Cancelled': return 'select-cancelled';
      default: return '';
    }
  }

  changeCategory(category: string) {
    this.selectedCategory = category;
    this.currentCategory = this.menu.find(cat => cat.category === category);
  }

  getCategoryIcon(category: string): string {
    if (category.toLowerCase().includes('cafea')) return 'fas fa-mug-hot';
    if (category.toLowerCase().includes('baut')) return 'fas fa-glass-martini';
    if (category.toLowerCase().includes('alcool')) return 'fas fa-wine-bottle';
    return 'fas fa-utensils';
  }

  addToCart(item: any) {
    const found = this.cart.find(i => i.name === item.name);
    if (found) {
      found.quantity++;
    } else {
      this.cart.push({ name: item.name, quantity: 1, price: item.price });
    }
  }

  removeFromCart(name: string) {
    const idx = this.cart.findIndex(i => i.name === name);
    if (idx !== -1) this.cart.splice(idx, 1);
  }

  cancelEditing(): void {
    this.isEditing = false;
    this.newStatus = this.order?.status || '';
  }

  getPriceForProduct(productName: string): number {
    for (const category of this.menu) {
      const item = category.items.find((i: any) => i.name === productName);
      if (item) return item.price;
    }
    return 0;
  }

  getTotal(): number {
    return this.cart.reduce((total, item) => total + item.price * item.quantity, 0);
  }

  getLocalTime(date: string | Date): string {
    const original = new Date(date);
    const localTime = new Date(original.getTime() + 3 * 60 * 60 * 1000);
    return localTime.toLocaleString('ro-RO', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

}
