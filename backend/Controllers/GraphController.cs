using Microsoft.AspNetCore.Mvc;
using SkillGraph.Api.Models;
using SkillGraph.Api.Services;

namespace SkillGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GraphController : ControllerBase
{
    private readonly IGraphService _graph;
    private readonly ILogger<GraphController> _logger;

    public GraphController(IGraphService graph, ILogger<GraphController> logger)
    {
        _graph = graph;
        _logger = logger;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var ok = await _graph.IsHealthyAsync();
        if (!ok) return StatusCode(503, new { status = "unhealthy", message = "Cannot reach CognoDB" });
        return Ok(new { status = "healthy" });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        try
        {
            await _graph.SeedAsync();
            var stats = await _graph.GetStatsAsync();
            return Ok(new { message = "Seed completed", stats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seed failed");
            return StatusCode(503, new { error = "Database unreachable or seed failed", detail = ex.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<GraphStatsDto>> Stats()
    {
        try
        {
            return Ok(await _graph.GetStatsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stats failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }

    [HttpGet("people")]
    public async Task<ActionResult<IReadOnlyList<PersonDto>>> People()
    {
        try
        {
            return Ok(await _graph.GetPeopleAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPeople failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }

    [HttpGet("people/{id}")]
    public async Task<ActionResult<PersonDto>> Person(string id)
    {
        try
        {
            var person = await _graph.GetPersonAsync(id);
            if (person is null) return NotFound();
            return Ok(person);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPerson failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }

    [HttpGet("skills")]
    public async Task<ActionResult<IReadOnlyList<SkillDto>>> Skills()
    {
        try
        {
            return Ok(await _graph.GetSkillsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSkills failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }

    [HttpGet("projects")]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> Projects()
    {
        try
        {
            return Ok(await _graph.GetProjectsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProjects failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }

    [HttpGet("skills/{id}/prerequisites")]
    public async Task<ActionResult<IReadOnlyList<SkillDto>>> Prerequisites(string id)
    {
        try
        {
            return Ok(await _graph.GetSkillPrerequisitesAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Prerequisites failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }

    [HttpGet("path")]
    public async Task<ActionResult<IReadOnlyList<SkillPathDto>>> LearningPath(
        [FromQuery] string from, [FromQuery] string to)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            return BadRequest(new { error = "from and to skill ids are required" });
        try
        {
            return Ok(await _graph.FindLearningPathAsync(from, to));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LearningPath failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }

    [HttpGet("people/{id}/mentors")]
    public async Task<ActionResult<IReadOnlyList<RecommendationDto>>> Mentors(string id)
    {
        try
        {
            return Ok(await _graph.RecommendMentorsAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mentors failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }

    [HttpGet("projects/{id}/experts")]
    public async Task<ActionResult<IReadOnlyList<PersonDto>>> Experts(string id)
    {
        try
        {
            return Ok(await _graph.FindExpertsForProjectAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Experts failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }

    [HttpGet("people/{id}/suggest-skills")]
    public async Task<ActionResult<IReadOnlyList<SkillDto>>> SuggestSkills(string id)
    {
        try
        {
            return Ok(await _graph.SuggestNextSkillsAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SuggestSkills failed");
            return StatusCode(503, new { error = "Database unreachable", detail = ex.Message });
        }
    }
}