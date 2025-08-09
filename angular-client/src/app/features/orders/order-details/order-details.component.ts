import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrderService } from '../order.service';
import { Order } from '../order.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

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
    { value: 'Pending',    label: 'În așteptare' },
    { value: 'InProgress', label: 'În curs de preparare' },
    { value: 'Done',       label: 'Finalizată' },
    { value: 'Cancelled',  label: 'Anulată' }
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
  canEdit = false;                 // <- recalculăm în funcție de rol + conținut
  currentRole: 'BAR' | 'BUCATARIE' = 'BAR';

  menu: any[] = [];
  selectedCategory = '';
  currentCategory: any = null;
  cart: { name: string; quantity: number; price: number }[] = [];

  selectedTab = 'active';

  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.selectedTab = this.route.snapshot.queryParamMap.get('tab') || 'active';
    this.currentRole = (localStorage.getItem('role') as 'BAR' | 'BUCATARIE') || 'BAR';

    const id = this.route.snapshot.paramMap.get('id')!;
    const orderPromise = this.orderService.getById(id).toPromise();
    const menuPromise = fetch('/assets/menu.json').then(res => res.json());

    Promise.all([orderPromise, menuPromise]).then(([orderData, menuData]) => {
      if (!orderData || !menuData) return;

      this.order = orderData;
      this.newStatus = orderData.status;
      this.menu = menuData;

      // calculează permisiunile pe rol + conținut
      const hasFood = this.containsFood(orderData.items.map(i => i.productName));
      this.canEdit = (this.currentRole === 'BUCATARIE' && hasFood) || (this.currentRole === 'BAR' && !hasFood);

      if (this.menu.length > 0) this.changeCategory(this.menu[0].category);

      this.cart = orderData.items.map(i => ({
        name: i.productName,
        quantity: i.quantity,
        price: this.getPriceForProduct(i.productName)
      }));
    });
  }

  goBack(): void {
    this.router.navigate(['/orders', this.selectedTab]);
  }

  updateStatus(): void {
    if (!this.order || !this.newStatus) return;
    if (!this.canUpdateStatusForRole(this.order.items)) {
      alert('Nu ai permisiunea să actualizezi această comandă.');
      return;
    }

    this.orderService.updateStatus(this.order.id, this.newStatus).subscribe({
      next: (res) => {
        if (res.success) {
          this.order!.status = this.newStatus;
          // anunță BAR-ul (sau zona țintă) – backend-ul trimite push/SMS
          this.orderService.notifyStatusChange(this.order!.id, this.newStatus).subscribe({
            next: () => console.log('Notificare trimisă.'),
            error: (e) => console.warn('Notificare eșuată:', e)
          });
          alert('Status actualizat.');
        } else {
          alert('Actualizarea statusului a eșuat.');
        }
      },
      error: (err) => {
        console.error('Error updating status:', err);
        alert('Actualizarea statusului a eșuat.');
      }
    });
  }

  submitEdit(): void {
    if (!this.order) return;
    if (!this.canEdit) {
      alert('Nu ai permisiunea să editezi această comandă.');
      return;
    }

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
  }

  /** ———— Permisiuni pe conținut ———— */
  private containsFood(names: string[]): boolean {
    const foodKeywords = [
      'șnițel','snitel','pui','porc','aripioare','pizza','mici','ceafă',
      'cartofi','salată','salata','sos','chiflă','chifla','papanasi','gogoși','gogosi',
      'burger','paste','tocan','ciorb','fript','gratar'
    ];
    const lower = names.map(n => n?.toLowerCase() || '');
    return lower.some(n => foodKeywords.some(k => n.includes(k)));
  }

  /** dacă e BUCĂTĂRIE -> poate schimba status la comenzi cu mâncare;
   *  dacă e BAR -> poate schimba status la comenzi FĂRĂ mâncare
   */
  canUpdateStatusForRole(items: { productName: string; quantity: number }[]): boolean {
    const hasFood = this.containsFood(items.map(i => i.productName));
    return (this.currentRole === 'BUCATARIE' && hasFood) || (this.currentRole === 'BAR' && !hasFood);
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
    if (found) found.quantity++;
    else this.cart.push({ name: item.name, quantity: 1, price: item.price });
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
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }
}
