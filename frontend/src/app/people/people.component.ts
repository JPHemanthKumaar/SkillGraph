import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../api.service';
import { Person } from '../models';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './people.component.html',
  styleUrl: './people.component.scss'
})
export class PeopleComponent implements OnInit {
  people: Person[] = [];
  loading = true;
  error: string | null = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.people().subscribe({
      next: p => { this.people = p; this.loading = false; },
      error: e => { this.error = e?.error?.detail || 'Failed to load'; this.loading = false; }
    });
  }
}
