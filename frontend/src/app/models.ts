export interface Person {
  id: string;
  name: string;
  title: string;
  bio?: string;
  avatarUrl?: string;
  skills?: SkillLevel[];
}

export interface SkillLevel {
  skillId: string;
  skillName: string;
  level: string;
  years: number;
}

export interface Skill {
  id: string;
  name: string;
  category: string;
  description?: string;
}

export interface Project {
  id: string;
  name: string;
  description?: string;
  status: string;
}

export interface Recommendation {
  personId: string;
  personName: string;
  title: string;
  sharedSkill: string;
  reason: string;
}

export interface PathStep {
  skillId: string;
  skillName: string;
  hop: number;
}

export interface SkillPath {
  path: PathStep[];
  length: number;
}

export interface GraphStats {
  people: number;
  skills: number;
  projects: number;
  relationships: number;
}