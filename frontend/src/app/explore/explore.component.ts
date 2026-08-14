import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../api.service';
import { Skill, SkillPath } from '../models';

@Component({
  selector: 'app-explore',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './explore.component.html',
  styleUrl: './explore.component.scss'
})
export class ExploreComponent implements OnInit {
  skills: Skill[] = [];
  fromId = '';
  toId = '';
  paths: SkillPath[] = [];
  loading = false;
  error: string | null = null;
  searched = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.skills().subscribe({
      next: s => {
        this.skills = s;
        if (s.length >= 2) {
          this.fromId = s.find(x => x.name === 'TypeScript')?.id || s[0].id;
          this.toId = s.find(x => x.name === 'Kubernetes')?.id || s[1].id;
        }
      }
    });
  }

  find() {
    if (!this.fromId || !this.toId) return;
    this.loading = true;
    this.searched = true;
    this.error = null;
    this.paths = [];
    this.api.learningPath(this.fromId, this.toId).subscribe({
      next: p => { this.paths = p; this.loading = false; },
      error: e => {
        this.error = e?.error?.detail || 'Query failed';
        this.loading = false;
      }
    });
  }
}
