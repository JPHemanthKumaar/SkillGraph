import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from './api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  dbStatus: 'unknown' | 'healthy' | 'unhealthy' = 'unknown';

  constructor(private api: ApiService) {
    this.api.health().subscribe(r => {
      this.dbStatus = r.status === 'healthy' ? 'healthy' : 'unhealthy';
    });
  }
}
