import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginForm = new FormGroup({
    Email:    new FormControl('',[
      Validators.required,
      Validators.pattern("^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$")
    ]),
    Password: new FormControl('',[
      Validators.required,
      Validators.pattern(/^(?=\D*\d)(?=[^a-z]*[a-z])(?=.*[$@$!%*?&])(?=[^A-Z]*[A-Z]).{8,30}$/)
    ])
  });
  
  inPassword()
  {
    return this.loginForm.get('Password');
  }

  inEmail()
  {
    return this.loginForm.get('Email');
  }

  submit()
  {
    this.http.post("http://localhost:5281/login",this.loginForm.value)
  }

  constructor(private http: HttpClient){}
}
