import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../api.service';
import { GraphStats } from '../models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  stats: GraphStats | null = null;
  loading = true;
  seeding = false;
  error: string | null = null;
  seedMsg: string | null = null;

  constructor(private api: ApiService) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.error = null;
    this.api.stats().subscribe({
      next: s => { this.stats = s; this.loading = false; },
      error: err => {
        this.loading = false;
        this.error = err?.error?.detail || err?.message || 'Cannot reach database';
      }
    });
  }

  seed() {
    this.seeding = true;
    this.seedMsg = null;
    this.api.seed().subscribe({
      next: res => {
        this.seeding = false;
        this.seedMsg = 'Seed completed successfully';
        this.stats = res.stats;
      },
      error: err => {
        this.seeding = false;
        this.error = err?.error?.detail || 'Seed failed — check CognoDB credentials';
      }
    });
  }
}
