import { NgModule            } from '@angular/core';
import { BrowserModule       } from '@angular/platform-browser';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { AppRoutingModule    } from './app-routing.module';
import { AppComponent        } from './app.component';
import { HttpClientModule    } from '@angular/common/http';
import { LoginComponent      } from './login/login.component';
import { RouterModule        } from '@angular/router';
import { MainComponent       } from './main/main.component';
import { RegisterComponent } from './register/register.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule.forRoot([
      { path: ''     , component: AppComponent  },
      { path: 'home' , component: MainComponent },
      { path: 'login', component: LoginComponent},

    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
