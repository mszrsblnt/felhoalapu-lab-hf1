import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { IdentityService } from '../../services/identity.service';

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

  onSubmit() {
    if (this.password !== this.passwordConfirm) {
      alert('A jelszavak nem egyeznek!');
      return;
    }

    this.auth.register(this.email, this.password).subscribe({
      next: () => {
        alert('Sikeres regisztráció!');
        this.router.navigate(['/login']);
      },
      error: (err) => alert('Hiba a regisztráció során!')
    });
  }

    ngOnInit(): void {
      if (this.auth.isAuthenticated()) {
        this.router.navigate(['/home']);
      }
  }
}