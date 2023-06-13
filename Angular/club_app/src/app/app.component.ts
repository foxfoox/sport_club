import { Component, OnInit } from '@angular/core';
import { AccessibilityService } from './accessibility.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'club_app';

  constructor(private accessibility: AccessibilityService, private router: Router){}
  ngOnInit(): void {
    if(!this.accessibility.isIn())
    {
      this.router.navigate(['/login']);
    }
    else
    {
      this.router.navigate(['/home']);
    }
  }
}
