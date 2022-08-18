import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CartPageComponent } from './cart-page/cart-page.component';
import { FurniturePageComponent } from './furniture-page/furniture-page.component';
import { HomeComponent } from './home/home.component';
import { LoginPageComponent } from './login-page/login-page.component';
import { SignupPageComponent } from './signup-page/signup-page.component';
import { UserPageComponent } from './user-page/user-page.component';

const routes: Routes = [
  { path: '', redirectTo:'login', pathMatch:'full'},
  {
    path: 'user', component:UserPageComponent,
    children: [
      { path: 'register', component:SignupPageComponent},
      { path: 'login', component:LoginPageComponent}
    ]  
  },
  { path: 'home', component: HomeComponent },
  { path: 'search/:searchItem', component: HomeComponent },
  { path: 'furniture/:id', component: FurniturePageComponent },
  { path: 'cart-page', component: CartPageComponent },
  
  
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}