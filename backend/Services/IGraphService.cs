using SkillGraph.Api.Models;

namespace SkillGraph.Api.Services;

public interface IGraphService
{
    Task<bool> IsHealthyAsync();
    Task SeedAsync();
    Task ClearAsync();
    Task<GraphStatsDto> GetStatsAsync();
    Task<IReadOnlyList<PersonDto>> GetPeopleAsync();
    Task<PersonDto?> GetPersonAsync(string id);
    Task<IReadOnlyList<SkillDto>> GetSkillsAsync();
    Task<IReadOnlyList<ProjectDto>> GetProjectsAsync();
    Task<IReadOnlyList<SkillDto>> GetSkillPrerequisitesAsync(string skillId);
    Task<IReadOnlyList<SkillPathDto>> FindLearningPathAsync(string fromSkillId, string toSkillId);
    Task<IReadOnlyList<RecommendationDto>> RecommendMentorsAsync(string personId);
    Task<IReadOnlyList<PersonDto>> FindExpertsForProjectAsync(string projectId);
    Task<IReadOnlyList<SkillDto>> SuggestNextSkillsAsync(string personId);
}