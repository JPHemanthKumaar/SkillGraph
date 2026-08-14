import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of, throwError } from 'rxjs';
import {
  Person, Skill, Project, Recommendation, SkillPath, GraphStats
} from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private base = '/api/graph';

  constructor(private http: HttpClient) {}

  health(): Observable<{ status: string }> {
    return this.http.get<{ status: string }>(`${this.base}/health`).pipe(
      catchError(() => of({ status: 'unhealthy' }))
    );
  }

  seed(): Observable<any> {
    return this.http.post(`${this.base}/seed`, {});
  }

  stats(): Observable<GraphStats> {
    return this.http.get<GraphStats>(`${this.base}/stats`).pipe(
      catchError(err => throwError(() => err))
    );
  }

  people(): Observable<Person[]> {
    return this.http.get<Person[]>(`${this.base}/people`);
  }

  person(id: string): Observable<Person> {
    return this.http.get<Person>(`${this.base}/people/${id}`);
  }

  skills(): Observable<Skill[]> {
    return this.http.get<Skill[]>(`${this.base}/skills`);
  }

  projects(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.base}/projects`);
  }

  prerequisites(skillId: string): Observable<Skill[]> {
    return this.http.get<Skill[]>(`${this.base}/skills/${skillId}/prerequisites`);
  }

  learningPath(from: string, to: string): Observable<SkillPath[]> {
    return this.http.get<SkillPath[]>(`${this.base}/path`, {
      params: { from, to }
    });
  }

  mentors(personId: string): Observable<Recommendation[]> {
    return this.http.get<Recommendation[]>(`${this.base}/people/${personId}/mentors`);
  }

  experts(projectId: string): Observable<Person[]> {
    return this.http.get<Person[]>(`${this.base}/projects/${projectId}/experts`);
  }

  suggestSkills(personId: string): Observable<Skill[]> {
    return this.http.get<Skill[]>(`${this.base}/people/${personId}/suggest-skills`);
  }
}