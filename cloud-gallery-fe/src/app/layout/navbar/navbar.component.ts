import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { IdentityService } from '../../services/identity.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html'
})
export class NavbarComponent {
  auth = inject(IdentityService);

  userEmail = computed(() => this.auth.userInfo()?.email || null);

  logout() {
    this.auth.logout();
  }
}