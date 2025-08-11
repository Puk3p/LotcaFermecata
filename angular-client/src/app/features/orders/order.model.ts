export type PaymentMethods = Record<string, number>;

export enum OrderStatus {
  Pending = 0,
  InProgress = 1,
  Done = 2,
  Cancelled = 3
}

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

export interface GroupedOrdersDto {
  active: Order[];
  completed: Order[];
  canceled: Order[];
  archivedGrouped: {
    [date: string]: {
      orders: Order[];
      summary: { product: string; quantity: number; total: number }[];
    }
  }
}

export interface ProductSummary {
  productName: string;
  ordersCount: number;
  quantity: number;
  unitPrice?: number | null;
  totalPaid?: number | null;
}

export interface DayTotals {
  ordersPaid: number;
  itemsPaid: number;
  revenue?: number | null;
  paymentMethods: PaymentMethods;
}

export interface ArchivedDay {
  orders: Order[];
  summary: ProductSummary[];
  totals: DayTotals;
}

export type ArchivedGrouped = {
  [date: string]: ArchivedDay;
};
