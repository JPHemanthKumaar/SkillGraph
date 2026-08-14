import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../api.service';
import { Person, Recommendation, Skill } from '../models';

@Component({
  selector: 'app-person-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './person-detail.component.html',
  styleUrl: './person-detail.component.scss'
})
export class PersonDetailComponent implements OnInit {
  person: Person | null = null;
  mentors: Recommendation[] = [];
  suggestions: Skill[] = [];
  loading = true;
  error: string | null = null;

  constructor(private route: ActivatedRoute, private api: ApiService) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.api.person(id).subscribe({
      next: p => {
        this.person = p;
        this.loading = false;
        this.api.mentors(id).subscribe(m => this.mentors = m);
        this.api.suggestSkills(id).subscribe(s => this.suggestions = s);
      },
      error: e => { this.error = e?.error?.detail || 'Not found'; this.loading = false; }
    });
  }
}
