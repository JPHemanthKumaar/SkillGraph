# SkillGraph

A small full-stack application backed by **CognoDB** (Neo4j-compatible graph database).  
Stack: **ASP.NET Core 8** + **Angular 18** + official **Neo4j .NET driver**.

## Use case

**SkillGraph** models people, skills, projects and the relationships between them:

- People have skills at different levels  
- Skills have prerequisites and related skills  
- Projects require skills  
- People mentor other people and work on projects  

The interesting questions are about **connections**: learning paths, mentor recommendations, “who can staff this project?”, “what should I learn next?”.

### Why a graph database?

| Question | Graph | Relational |
|----------|-------|------------|
| Shortest learning path between two skills | `shortestPath` over `PREREQUISITE \| RELATED_TO` | Recursive CTE or many self-joins |
| Mentor recommendations by shared skills | Multi-hop + aggregation on `HAS_SKILL` | Wide joins + grouping |
| Experts for a project | `Project-[:REQUIRES]->Skill<-[:HAS_SKILL]-Person` | Classic many-to-many join explosion |
| Next skills 1–2 hops away | Variable-length path `*1..2` | Recursive CTE |

A graph model stores relationships as first-class citizens, so multi-hop traversals stay readable and efficient. A relational schema would bury the same logic in recursive CTEs and intermediate join tables.

## Data model

```
(:Person {id, name, title, bio, avatarUrl})
(:Skill  {id, name, category, description})
(:Project {id, name, description, status})

(Person)-[:HAS_SKILL {level, years}]->(Skill)
(Person)-[:MENTORS {since}]->(Person)
(Person)-[:WORKS_ON]->(Project)
(Project)-[:REQUIRES_SKILL]->(Skill)
(Skill)-[:PREREQUISITE]->(Skill)      // "needs to know X first"
(Skill)-[:RELATED_TO]->(Skill)        // bidirectional relatedness
```

```
                    ┌─────────┐
         MENTORS    │ Person  │──HAS_SKILL──▶│ Skill │◀──REQUIRES_SKILL──│ Project │
              │     └────┬────┘              └──┬───┘                     └────────┘
              └──────────┘                      │
                                           PREREQUISITE
                                           RELATED_TO
                                                │
                                           ┌────▼───┐
                                           │ Skill  │
                                           └────────┘
```

## Main Cypher queries (all parameterised)

1. **Prerequisites (multi-hop)**  
   `MATCH (s:Skill {id:$id})-[:PREREQUISITE*1..5]->(pre) RETURN pre`

2. **Learning path (shortestPath — awkward in SQL)**  
   `MATCH path = shortestPath((a:Skill {id:$from})-[:PREREQUISITE|RELATED_TO*1..6]-(b:Skill {id:$to}))`

3. **Mentor recommendations**  
   Shared skills between people, ordered by overlap.

4. **Project experts**  
   People who possess skills required by a project.

5. **Suggested next skills**  
   Skills 1–2 hops away that the person does not already have.

## Setup

### 1. CognoDB Cloud

1. Sign up at [console.cognodb.com/signup](https://console.cognodb.com/signup) (free tier, no card).  
2. Create a free **c0** instance.  
3. Copy the `bolt+s://…` URI and the password for user `cognodb` (shown once).

### 2. Environment

```bash
cp .env.example .env
# edit .env with your URI and password
```

Or export:

```bash
export COGNODB_URI=bolt+s://xxxx.databases.cognodb.cloud
export COGNODB_USER=cognodb
export COGNODB_PASSWORD=your_password
```

### 3. Backend

```bash
cd backend
dotnet restore
dotnet run --urls http://localhost:5080
```

Swagger: http://localhost:5080/swagger

### 4. Frontend

```bash
cd frontend
npm install
npm start
```

App: http://localhost:4200  
API calls are proxied to the backend.

### 5. Seed data

Open the app → **Home** → **Load sample data**,  
or `POST http://localhost:5080/api/graph/seed`.

## Project structure

```
skill-graph/
├── backend/                 # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Models/
│   ├── Services/            # GraphService (Neo4j driver)
│   └── Program.cs
├── frontend/                # Angular 18 standalone components
│   └── src/app/
├── .env.example
└── README.md
```

## Engineering notes

- Connection URI / password come from **environment variables** (or `CognoDB:*` config); nothing sensitive is committed.  
- All Cypher uses **parameters** via the official Neo4j driver — no string concatenation.  
- Controllers return **503** with a clear message when the database is unreachable.  
- UI has loading, empty and error states throughout.

## Deliverables checklist

- [x] Thoughtful graph model + diagram  
- [x] Seed script (API endpoint) with realistic data  
- [x] Multi-hop + relationally awkward queries  
- [x] Parameterised queries via official driver  
- [x] Functional Angular web app  
- [x] Clean UI (dark theme, navigation, states)  
- [x] Env-based secrets  
- [x] Graceful DB error handling  

## License

MIT — assignment submission for Wexa AI / CognoDB.
