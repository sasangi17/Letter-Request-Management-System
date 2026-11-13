
import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-admin-header',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './admin-header.html',
  styleUrl: './admin-header.css'
})
export class AdminHeader {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
