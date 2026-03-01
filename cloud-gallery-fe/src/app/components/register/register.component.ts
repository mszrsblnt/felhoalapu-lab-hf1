import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { IdentityService } from '../../services/identity.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent implements OnInit {
  email = '';
  password = '';
  passwordConfirm = '';

  auth = inject(IdentityService);
  router = inject(Router);
  toast = inject(ToastService);

  onSubmit() {
    if (this.password !== this.passwordConfirm) {
      this.toast.show('A jelszavak nem egyeznek!', 'danger', 'Hiba');
      return;
    }

    this.auth.register(this.email, this.password).subscribe({
      next: () => {
        alert('Sikeres regisztráció!');
        this.toast.show('Sikeres regisztráció!', 'success', 'Rendszer');
        this.router.navigate(['/login']);
      },
      error: (err) => this.toast.show('Sikertelen regisztráció!', 'danger', 'Hiba')
    });
  }

    ngOnInit(): void {
      if (this.auth.isAuthenticated()) {
        this.router.navigate(['/home']);
      }
  }
}