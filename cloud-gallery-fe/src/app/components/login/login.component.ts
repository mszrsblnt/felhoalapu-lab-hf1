import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { IdentityService } from '../../services/identity.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  email = '';
  password = '';

  auth = inject(IdentityService);
  router = inject(Router);
  toast = inject(ToastService);

  onSubmit() {

    const loginData = {
      email: this.email,
      password: this.password
    };
    this.auth.login(loginData).subscribe({
      next: () => {
      this.toast.show('Sikeres bejelentkezés!', 'success', 'Rendszer');
      this.router.navigate(['/home']);
    },
      error: (err) => this.toast.show('Sikertelen bejelentkezés!', 'danger', 'Hiba')
    });
  }
  ngOnInit(): void {
      if (this.auth.isAuthenticated()) {
        this.router.navigate(['/home']);
      }
  }
}