import { Component, OnInit } from '@angular/core';
import { Router} from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit{
  access_token: string;
  id_token: string;
  username: string;

  constructor(private router: Router) { }
  
  ngOnInit()
  {
    if(!window.localStorage.getItem('at') && !window.localStorage.getItem('un') && !window.localStorage.getItem('it'))
    {
      this.login();
    }
    else
    {
      this.access_token = window.localStorage.getItem('at');
      this.id_token = window.localStorage.getItem('it');
      this.username = window.localStorage.getItem('un');
    }
  }

  login()
  {
    window.location.href = 'https://localhost:5001/external/challenge?scheme=AAD&returnUrl=/detail-callback';
  }
}
