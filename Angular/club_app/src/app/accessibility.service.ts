import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AccessibilityService {

  token: string | undefined

  constructor() { }

  isIn() : boolean
  {
    return this.token !== undefined;
  }

  fuckOff()
  {
    this.token = undefined;
  }

  getIn(token: string)
  {
    this.token = token;
  }

}
