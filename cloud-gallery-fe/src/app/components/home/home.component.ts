import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { IdentityService } from '../../services/identity.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  imports: [RouterLink, CommonModule]
})
export class HomeComponent {
  auth = inject(IdentityService);
  isAuthenticated = computed(() => this.auth.isAuthenticated());
  userEmail = computed(() => this.auth.userInfo()?.email ?? null);
}
