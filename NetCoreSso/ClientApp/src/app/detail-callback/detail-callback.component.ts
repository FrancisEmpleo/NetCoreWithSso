import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';

@Component({
  templateUrl: './detail-callback.component.html',
  styleUrls: ['./detail-callback.component.scss']
})
export class DetailCallbackComponent implements OnInit {

  constructor(private activatedRoute: ActivatedRoute, private router: Router) { }

  ngOnInit() {
    this.activatedRoute.queryParamMap.subscribe(x =>{
      if(x.get('at') && x.get('un') && x.get('it'))
      {
        window.localStorage.setItem('at', x.get('at'));
        window.localStorage.setItem('un', x.get('un'));
        window.localStorage.setItem('it', x.get('it'));
        this.router.navigate(['/']);
      }
    });
  }

}
