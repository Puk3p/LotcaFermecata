import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrdersComponent } from './orders.component'; // adaugă asta
import { OrderListComponent } from './order-list/order-list.component';
import { OrderDetailsComponent } from './order-details/order-details.component';
import { OrderCreateComponent } from './order-create/order-create.component';

// orders-routing.module.ts
const routes: Routes = [
  { path: 'new', component: OrderCreateComponent },                 // mai specific
  { path: 'view/:id', component: OrderDetailsComponent },           // mutăm detaliile pe /orders/view/:id ca să nu bată cu :tab
  { path: '', component: OrdersComponent },                         // /orders
  { path: ':tab', component: OrdersComponent },                     // /orders/active etc.
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrdersRoutingModule {}
