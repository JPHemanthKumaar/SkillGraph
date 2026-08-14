namespace SkillGraph.Api.Models;

public record PersonDto(
    string Id,
    string Name,
    string Title,
    string? Bio,
    string? AvatarUrl,
    List<SkillLevelDto>? Skills = null
);

public record SkillLevelDto(string SkillId, string SkillName, string Level, int Years);

public record SkillDto(
    string Id,
    string Name,
    string Category,
    string? Description
);

public record ProjectDto(
    string Id,
    string Name,
    string? Description,
    string Status
);

public record PathStepDto(string SkillId, string SkillName, int Hop);

public record RecommendationDto(
    string PersonId,
    string PersonName,
    string Title,
    string SharedSkill,
    string Reason
);

public record GraphStatsDto(
    long People,
    long Skills,
    long Projects,
    long Relationships
);

public record SkillPathDto(
    List<PathStepDto> Path,
    int Length
);