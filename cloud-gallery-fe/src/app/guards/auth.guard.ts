import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { IdentityService } from '../services/identity.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private identity: IdentityService, private router: Router) {}

  canActivate(): boolean | UrlTree {
    if (this.identity.getToken()) {
      return true;
    }
    return this.router.parseUrl('/login');
  }
}
