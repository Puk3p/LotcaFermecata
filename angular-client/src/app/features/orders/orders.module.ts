import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrdersRoutingModule } from './orders-routing.module';
import { OrderListComponent } from './order-list/order-list.component';
import { OrderDetailsComponent } from './order-details/order-details.component';
import { OrderCreateComponent } from './order-create/order-create.component';

@NgModule({
  imports: [
    CommonModule,
    OrdersRoutingModule,
    OrderListComponent,
    OrderDetailsComponent,
    OrderCreateComponent
  ]
})
export class OrdersModule {}
