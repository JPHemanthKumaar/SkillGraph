import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../api.service';
import { Skill } from '../models';

@Component({
  selector: 'app-skills',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './skills.component.html',
  styleUrl: './skills.component.scss'
})
export class SkillsComponent implements OnInit {
  skills: Skill[] = [];
  prereqs: Skill[] = [];
  selected: Skill | null = null;
  loading = true;
  loadingPre = false;
  error: string | null = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.skills().subscribe({
      next: s => { this.skills = s; this.loading = false; },
      error: e => { this.error = e?.error?.detail || 'Failed'; this.loading = false; }
    });
  }

  select(s: Skill) {
    this.selected = s;
    this.loadingPre = true;
    this.prereqs = [];
    this.api.prerequisites(s.id).subscribe({
      next: p => { this.prereqs = p; this.loadingPre = false; },
      error: () => { this.loadingPre = false; }
    });
  }

  byCategory(): { category: string; skills: Skill[] }[] {
    const map = new Map<string, Skill[]>();
    for (const s of this.skills) {
      if (!map.has(s.category)) map.set(s.category, []);
      map.get(s.category)!.push(s);
    }
    return Array.from(map.entries()).map(([category, skills]) => ({ category, skills }));
  }
}
