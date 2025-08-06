import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrderListComponent } from './order-list/order-list.component';
import { OrderDetailsComponent } from './order-details/order-details.component';
import { OrderCreateComponent } from './order-create/order-create.component';

const routes: Routes = [
  { path: '', component: OrderListComponent },
  { path: 'new', component: OrderCreateComponent },
  { path: ':id', component: OrderDetailsComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrdersRoutingModule {}
