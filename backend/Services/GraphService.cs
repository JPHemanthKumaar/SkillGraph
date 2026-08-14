using Neo4j.Driver;
using SkillGraph.Api.Models;

namespace SkillGraph.Api.Services;

public class GraphService : IGraphService, IDisposable
{
    private readonly ILogger<GraphService> _logger;
    private readonly string? _uri;
    private readonly string _user;
    private readonly string _password;
    private readonly string _database;
    private IDriver? _driver;
    private readonly object _lock = new();

    public GraphService(IConfiguration config, ILogger<GraphService> logger)
    {
        _logger = logger;

        _uri = FirstNonEmpty(
            config["CognoDB:Uri"],
            Environment.GetEnvironmentVariable("COGNODB_URI"));

        _user = FirstNonEmpty(
            config["CognoDB:User"],
            Environment.GetEnvironmentVariable("COGNODB_USER"),
            "cognodb")!;

        _password = FirstNonEmpty(
            config["CognoDB:Password"],
            Environment.GetEnvironmentVariable("COGNODB_PASSWORD")) ?? "";

        _database = FirstNonEmpty(
            config["CognoDB:Database"],
            Environment.GetEnvironmentVariable("COGNODB_DATABASE"),
            "neo4j")!;

        if (string.IsNullOrWhiteSpace(_uri))
        {
            _logger.LogWarning(
                "CognoDB URI is not set. Set COGNODB_URI (and COGNODB_PASSWORD) via environment variables, " +
                "a .env file in the project root, or CognoDB:Uri in appsettings.Local.json.");
        }
        else
        {
            _logger.LogInformation("CognoDB configured for {Uri} (user={User})", _uri, _user);
        }
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
            if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
        return null;
    }

    private IDriver GetDriver()
    {
        if (_driver != null) return _driver;

        lock (_lock)
        {
            if (_driver != null) return _driver;

            if (string.IsNullOrWhiteSpace(_uri))
                throw new InvalidOperationException(
                    "CognoDB URI is empty. Set COGNODB_URI and COGNODB_PASSWORD " +
                    "(PowerShell: $env:COGNODB_URI='bolt+s://...'; $env:COGNODB_PASSWORD='...') " +
                    "or put them in a .env file next to the solution, then restart the API.");

            if (string.IsNullOrWhiteSpace(_password))
                throw new InvalidOperationException(
                    "CognoDB password is empty. Set COGNODB_PASSWORD and restart the API.");

            _driver = GraphDatabase.Driver(_uri, AuthTokens.Basic(_user, _password));
            return _driver;
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_uri) || string.IsNullOrWhiteSpace(_password))
                return false;
            await GetDriver().VerifyConnectivityAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed");
            return false;
        }
    }

    public async Task ClearAsync()
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync("MATCH (n) DETACH DELETE n");
        });
    }

    public async Task SeedAsync()
    {
        await ClearAsync();
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));

        // Constraints are best-effort — some managed graph engines differ slightly
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync("CREATE CONSTRAINT person_id IF NOT EXISTS FOR (p:Person) REQUIRE p.id IS UNIQUE");
                await tx.RunAsync("CREATE CONSTRAINT skill_id IF NOT EXISTS FOR (s:Skill) REQUIRE s.id IS UNIQUE");
                await tx.RunAsync("CREATE CONSTRAINT project_id IF NOT EXISTS FOR (p:Project) REQUIRE p.id IS UNIQUE");
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create constraints (continuing seed)");
        }

        var people = new[]
        {
            new { id = "p1", name = "Aisha Khan", title = "Senior Backend Engineer", bio = "Distributed systems & graph databases", avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Aisha" },
            new { id = "p2", name = "Marcus Chen", title = "Frontend Lead", bio = "Angular & design systems", avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Marcus" },
            new { id = "p3", name = "Priya Sharma", title = "Data Engineer", bio = "Pipelines, Spark, and knowledge graphs", avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Priya" },
            new { id = "p4", name = "Jordan Lee", title = "Full-Stack Developer", bio = "Node, .NET and cloud", avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Jordan" },
            new { id = "p5", name = "Sofia Rossi", title = "ML Engineer", bio = "NLP and recommendation systems", avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Sofia" },
            new { id = "p6", name = "Dev Patel", title = "DevOps Engineer", bio = "Kubernetes, observability", avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Dev" },
            new { id = "p7", name = "Emma Wilson", title = "Product Engineer", bio = "APIs and developer experience", avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Emma" },
            new { id = "p8", name = "Carlos Mendes", title = "Security Engineer", bio = "Auth, threat modeling", avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Carlos" }
        };

        foreach (var p in people)
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    "CREATE (p:Person {id: $id, name: $name, title: $title, bio: $bio, avatarUrl: $avatar})",
                    new { p.id, p.name, p.title, p.bio, p.avatar });
            });
        }

        var skills = new[]
        {
            new { id = "s1", name = "C#", category = "Language", description = "Modern .NET language" },
            new { id = "s2", name = "ASP.NET Core", category = "Backend", description = "Web framework for .NET" },
            new { id = "s3", name = "Angular", category = "Frontend", description = "TypeScript SPA framework" },
            new { id = "s4", name = "TypeScript", category = "Language", description = "Typed JavaScript" },
            new { id = "s5", name = "Cypher", category = "Query", description = "Graph query language" },
            new { id = "s6", name = "Neo4j / Graph DB", category = "Database", description = "Property graph databases" },
            new { id = "s7", name = "Docker", category = "DevOps", description = "Containers" },
            new { id = "s8", name = "Kubernetes", category = "DevOps", description = "Container orchestration" },
            new { id = "s9", name = "Python", category = "Language", description = "General-purpose language" },
            new { id = "s10", name = "Spark", category = "Data", description = "Distributed data processing" },
            new { id = "s11", name = "Machine Learning", category = "AI/ML", description = "Supervised & unsupervised models" },
            new { id = "s12", name = "REST APIs", category = "Backend", description = "HTTP API design" },
            new { id = "s13", name = "GraphQL", category = "Backend", description = "Flexible query APIs" },
            new { id = "s14", name = "OAuth / Auth", category = "Security", description = "Identity and access" },
            new { id = "s15", name = "System Design", category = "Architecture", description = "Scalable system thinking" }
        };

        foreach (var s in skills)
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    "CREATE (s:Skill {id: $id, name: $name, category: $category, description: $description})",
                    new { s.id, s.name, s.category, s.description });
            });
        }

        var projects = new[]
        {
            new { id = "pr1", name = "SkillGraph Platform", description = "Internal skill & mentor discovery", status = "Active" },
            new { id = "pr2", name = "Realtime Analytics Dashboard", description = "Streaming metrics for ops", status = "Active" },
            new { id = "pr3", name = "Knowledge Graph RAG", description = "LLM grounded on company graph", status = "Planning" },
            new { id = "pr4", name = "Auth Gateway", description = "Centralized OAuth and MFA", status = "Active" }
        };

        foreach (var pr in projects)
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    "CREATE (p:Project {id: $id, name: $name, description: $description, status: $status})",
                    new { pr.id, pr.name, pr.description, pr.status });
            });
        }

        var hasSkill = new (string person, string skill, string level, int years)[]
        {
            ("p1", "s1", "Expert", 8), ("p1", "s2", "Expert", 6), ("p1", "s5", "Advanced", 3),
            ("p1", "s6", "Advanced", 3), ("p1", "s12", "Expert", 7), ("p1", "s15", "Advanced", 5),
            ("p2", "s3", "Expert", 7), ("p2", "s4", "Expert", 8), ("p2", "s13", "Intermediate", 2),
            ("p3", "s9", "Expert", 9), ("p3", "s10", "Advanced", 5), ("p3", "s6", "Intermediate", 2),
            ("p3", "s5", "Intermediate", 2), ("p3", "s11", "Advanced", 4),
            ("p4", "s1", "Advanced", 4), ("p4", "s2", "Advanced", 3), ("p4", "s3", "Intermediate", 2),
            ("p4", "s4", "Advanced", 4), ("p4", "s7", "Advanced", 3), ("p4", "s12", "Advanced", 4),
            ("p5", "s9", "Expert", 6), ("p5", "s11", "Expert", 5), ("p5", "s4", "Intermediate", 2),
            ("p6", "s7", "Expert", 6), ("p6", "s8", "Expert", 5), ("p6", "s1", "Intermediate", 2),
            ("p7", "s2", "Advanced", 4), ("p7", "s12", "Expert", 5), ("p7", "s13", "Advanced", 3),
            ("p7", "s4", "Advanced", 4), ("p7", "s15", "Intermediate", 2),
            ("p8", "s14", "Expert", 7), ("p8", "s12", "Advanced", 4), ("p8", "s1", "Intermediate", 3)
        };

        foreach (var (person, skill, level, years) in hasSkill)
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (p:Person {id: $person}), (s:Skill {id: $skill})
                    CREATE (p)-[:HAS_SKILL {level: $level, years: $years}]->(s)",
                    new { person, skill, level, years });
            });
        }

        var requires = new (string project, string skill)[]
        {
            ("pr1", "s1"), ("pr1", "s2"), ("pr1", "s3"), ("pr1", "s5"), ("pr1", "s6"),
            ("pr2", "s9"), ("pr2", "s10"), ("pr2", "s3"), ("pr2", "s4"),
            ("pr3", "s6"), ("pr3", "s5"), ("pr3", "s11"), ("pr3", "s9"),
            ("pr4", "s14"), ("pr4", "s2"), ("pr4", "s12")
        };

        foreach (var (project, skill) in requires)
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (pr:Project {id: $project}), (s:Skill {id: $skill})
                    CREATE (pr)-[:REQUIRES_SKILL]->(s)",
                    new { project, skill });
            });
        }

        var prereqs = new (string skill, string prereq)[]
        {
            ("s2", "s1"), ("s3", "s4"), ("s6", "s5"), ("s8", "s7"),
            ("s10", "s9"), ("s11", "s9"), ("s13", "s12"), ("s15", "s12")
        };

        foreach (var (skill, prereq) in prereqs)
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (s:Skill {id: $skill}), (p:Skill {id: $prereq})
                    CREATE (s)-[:PREREQUISITE]->(p)",
                    new { skill, prereq });
            });
        }

        var related = new (string a, string b)[]
        {
            ("s1", "s2"), ("s3", "s4"), ("s5", "s6"), ("s7", "s8"),
            ("s9", "s10"), ("s9", "s11"), ("s12", "s13"), ("s14", "s12")
        };

        foreach (var (a, b) in related)
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (sa:Skill {id: $a}), (sb:Skill {id: $b})
                    CREATE (sa)-[:RELATED_TO]->(sb)
                    CREATE (sb)-[:RELATED_TO]->(sa)",
                    new { a, b });
            });
        }

        var mentors = new (string mentor, string mentee)[]
        {
            ("p1", "p4"), ("p1", "p7"), ("p2", "p4"), ("p3", "p5"), ("p6", "p4"), ("p8", "p7")
        };

        foreach (var (mentor, mentee) in mentors)
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (m:Person {id: $mentor}), (e:Person {id: $mentee})
                    CREATE (m)-[:MENTORS {since: 2023}]->(e)",
                    new { mentor, mentee });
            });
        }

        var worksOn = new (string person, string project)[]
        {
            ("p1", "pr1"), ("p2", "pr1"), ("p4", "pr1"),
            ("p3", "pr2"), ("p5", "pr2"),
            ("p1", "pr3"), ("p3", "pr3"), ("p5", "pr3"),
            ("p8", "pr4"), ("p7", "pr4")
        };

        foreach (var (person, project) in worksOn)
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (p:Person {id: $person}), (pr:Project {id: $project})
                    CREATE (p)-[:WORKS_ON]->(pr)",
                    new { person, project });
            });
        }

        _logger.LogInformation("Seed data loaded successfully");
    }

    public async Task<GraphStatsDto> GetStatsAsync()
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            // Always returns one row even on an empty graph (no SingleAsync crash)
            var cursor = await tx.RunAsync(@"
                OPTIONAL MATCH (p:Person)
                WITH count(p) AS people
                OPTIONAL MATCH (s:Skill)
                WITH people, count(s) AS skills
                OPTIONAL MATCH (pr:Project)
                WITH people, skills, count(pr) AS projects
                OPTIONAL MATCH ()-[r]->()
                RETURN people, skills, projects, count(r) AS relationships");
            var record = await cursor.SingleAsync();
            return new GraphStatsDto(
                record["people"].As<long>(),
                record["skills"].As<long>(),
                record["projects"].As<long>(),
                record["relationships"].As<long>());
        });
    }

    public async Task<IReadOnlyList<PersonDto>> GetPeopleAsync()
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Person)
                OPTIONAL MATCH (p)-[hs:HAS_SKILL]->(s:Skill)
                WITH p, collect({skillId: s.id, skillName: s.name, level: hs.level, years: hs.years}) AS skills
                RETURN p.id AS id, p.name AS name, p.title AS title, p.bio AS bio, p.avatarUrl AS avatarUrl, skills
                ORDER BY p.name");
            var list = new List<PersonDto>();
            await foreach (var r in cursor)
            {
                list.Add(MapPerson(r));
            }
            return list;
        });
    }

    public async Task<PersonDto?> GetPersonAsync(string id)
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Person {id: $id})
                OPTIONAL MATCH (p)-[hs:HAS_SKILL]->(s:Skill)
                WITH p, collect({skillId: s.id, skillName: s.name, level: hs.level, years: hs.years}) AS skills
                RETURN p.id AS id, p.name AS name, p.title AS title, p.bio AS bio, p.avatarUrl AS avatarUrl, skills",
                new { id });
            if (!await cursor.FetchAsync()) return null;
            return MapPerson(cursor.Current);
        });
    }

    public async Task<IReadOnlyList<SkillDto>> GetSkillsAsync()
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (s:Skill)
                RETURN s.id AS id, s.name AS name, s.category AS category, s.description AS description
                ORDER BY s.category, s.name");
            var list = new List<SkillDto>();
            await foreach (var r in cursor)
            {
                list.Add(new SkillDto(
                    r["id"].As<string>(),
                    r["name"].As<string>(),
                    r["category"].As<string>(),
                    r["description"].As<string?>()));
            }
            return list;
        });
    }

    public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync()
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Project)
                RETURN p.id AS id, p.name AS name, p.description AS description, p.status AS status
                ORDER BY p.name");
            var list = new List<ProjectDto>();
            await foreach (var r in cursor)
            {
                list.Add(new ProjectDto(
                    r["id"].As<string>(),
                    r["name"].As<string>(),
                    r["description"].As<string?>(),
                    r["status"].As<string>()));
            }
            return list;
        });
    }

    public async Task<IReadOnlyList<SkillDto>> GetSkillPrerequisitesAsync(string skillId)
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (s:Skill {id: $skillId})-[:PREREQUISITE*1..5]->(pre:Skill)
                RETURN DISTINCT pre.id AS id, pre.name AS name, pre.category AS category, pre.description AS description",
                new { skillId });
            var list = new List<SkillDto>();
            await foreach (var r in cursor)
            {
                list.Add(new SkillDto(
                    r["id"].As<string>(),
                    r["name"].As<string>(),
                    r["category"].As<string>(),
                    r["description"].As<string?>()));
            }
            return list;
        });
    }

    public async Task<IReadOnlyList<SkillPathDto>> FindLearningPathAsync(string fromSkillId, string toSkillId)
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH path = shortestPath(
                    (from:Skill {id: $fromSkillId})-[:PREREQUISITE|RELATED_TO*1..6]-(to:Skill {id: $toSkillId})
                )
                WITH nodes(path) AS nodes, length(path) AS len
                RETURN [n IN nodes | {skillId: n.id, skillName: n.name}] AS steps, len AS length
                LIMIT 5",
                new { fromSkillId, toSkillId });
            var list = new List<SkillPathDto>();
            await foreach (var r in cursor)
            {
                var stepsRaw = r["steps"].As<List<object>>();
                var steps = new List<PathStepDto>();
                int hop = 0;
                foreach (var item in stepsRaw)
                {
                    if (item is IDictionary<string, object> d)
                    {
                        steps.Add(new PathStepDto(
                            d["skillId"]?.ToString() ?? "",
                            d["skillName"]?.ToString() ?? "",
                            hop++));
                    }
                }
                list.Add(new SkillPathDto(steps, r["length"].As<int>()));
            }
            return list;
        });
    }

    public async Task<IReadOnlyList<RecommendationDto>> RecommendMentorsAsync(string personId)
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (me:Person {id: $personId})-[:HAS_SKILL]->(s:Skill)<-[:HAS_SKILL]-(other:Person)
                WHERE me <> other
                WITH other, s, count(DISTINCT s) AS shared
                ORDER BY shared DESC
                WITH other, collect(s.name)[0] AS topSkill, shared
                RETURN other.id AS personId, other.name AS personName, other.title AS title,
                       topSkill AS sharedSkill,
                       'Shares ' + toString(shared) + ' skill(s); strong overlap on ' + topSkill AS reason
                LIMIT 8",
                new { personId });
            var list = new List<RecommendationDto>();
            await foreach (var r in cursor)
            {
                list.Add(new RecommendationDto(
                    r["personId"].As<string>(),
                    r["personName"].As<string>(),
                    r["title"].As<string>(),
                    r["sharedSkill"].As<string>(),
                    r["reason"].As<string>()));
            }
            return list;
        });
    }

    public async Task<IReadOnlyList<PersonDto>> FindExpertsForProjectAsync(string projectId)
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (pr:Project {id: $projectId})-[:REQUIRES_SKILL]->(s:Skill)<-[hs:HAS_SKILL]-(p:Person)
                WITH p, collect({skillId: s.id, skillName: s.name, level: hs.level, years: hs.years}) AS matchedSkills,
                     count(s) AS matchCount
                ORDER BY matchCount DESC
                RETURN p.id AS id, p.name AS name, p.title AS title, p.bio AS bio, p.avatarUrl AS avatarUrl, matchedSkills AS skills
                LIMIT 10",
                new { projectId });
            var list = new List<PersonDto>();
            await foreach (var r in cursor)
            {
                list.Add(MapPerson(r));
            }
            return list;
        });
    }

    public async Task<IReadOnlyList<SkillDto>> SuggestNextSkillsAsync(string personId)
    {
        await using var session = GetDriver().AsyncSession(o => o.WithDatabase(_database));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Person {id: $personId})-[:HAS_SKILL]->(known:Skill)
                MATCH (known)-[:RELATED_TO|PREREQUISITE*1..2]-(candidate:Skill)
                WHERE NOT (p)-[:HAS_SKILL]->(candidate)
                WITH candidate, count(*) AS score
                ORDER BY score DESC
                RETURN candidate.id AS id, candidate.name AS name, candidate.category AS category,
                       candidate.description AS description
                LIMIT 8",
                new { personId });
            var list = new List<SkillDto>();
            await foreach (var r in cursor)
            {
                list.Add(new SkillDto(
                    r["id"].As<string>(),
                    r["name"].As<string>(),
                    r["category"].As<string>(),
                    r["description"].As<string?>()));
            }
            return list;
        });
    }

    private static PersonDto MapPerson(Neo4j.Driver.IRecord r)
    {
        var skillsRaw = r["skills"].As<List<object>>();
        var skills = new List<SkillLevelDto>();
        foreach (var item in skillsRaw)
        {
            if (item is IDictionary<string, object> d && d.ContainsKey("skillId") && d["skillId"] != null)
            {
                skills.Add(new SkillLevelDto(
                    d["skillId"].ToString()!,
                    d["skillName"]?.ToString() ?? "",
                    d["level"]?.ToString() ?? "",
                    Convert.ToInt32(d["years"] ?? 0)));
            }
        }
        return new PersonDto(
            r["id"].As<string>(),
            r["name"].As<string>(),
            r["title"].As<string>(),
            r["bio"].As<string?>(),
            r["avatarUrl"].As<string?>(),
            skills);
    }

    public void Dispose()
    {
        _driver?.Dispose();
    }
}
