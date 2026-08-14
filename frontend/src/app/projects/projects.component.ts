import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../api.service';
import { Project, Person } from '../models';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss'
})
export class ProjectsComponent implements OnInit {
  projects: Project[] = [];
  experts: Person[] = [];
  selected: Project | null = null;
  loading = true;
  loadingExp = false;
  error: string | null = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.projects().subscribe({
      next: p => { this.projects = p; this.loading = false; },
      error: e => { this.error = e?.error?.detail || 'Failed'; this.loading = false; }
    });
  }

  select(p: Project) {
    this.selected = p;
    this.loadingExp = true;
    this.experts = [];
    this.api.experts(p.id).subscribe({
      next: e => { this.experts = e; this.loadingExp = false; },
      error: () => { this.loadingExp = false; }
    });
  }
}
