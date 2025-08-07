// order.model.ts
export interface OrderItem {
  productName: string;
  quantity: number;
  price?: number;
}

export interface Order {
  id: string;
  placedByUserId: string;
  clientName: string;
  clientPhone: string;
  targetZone: string;
  placedAt: string;
  status: string;
  items: OrderItem[];
}

export interface CreateOrder {
  clientName: string;
  clientPhone: string;
  targetZone: string;
  items: OrderItem[];
  placedByUserId: string;
}

export interface MenuItem {
  name: string;
  price: number;
}

export interface MenuCategory {
  category: string;
  items: MenuItem[];
}

