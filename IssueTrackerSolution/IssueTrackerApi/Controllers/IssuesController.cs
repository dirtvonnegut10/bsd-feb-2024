using IssueTrackerApi.Services;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace IssueTrackerApi.Controllers;

public class IssuesController(IDocumentSession session, BusinessClockHttpService api) : ControllerBase
{
    [HttpPost("issues")]
    public async Task<ActionResult> AddIssueAsync([FromBody] IssueRequest request)
    {
        var response = new IssueResponse(Guid.NewGuid(), request.Software,
            request.Description, DateTimeOffset.Now, IssueStatus.Pending);
        session.Insert(response);
        await session.SaveChangesAsync();
        var support = await api.GetCurrentSupportInformationAsync();

        var model = new IssueResponseModel()
        {
            Id = response.Id,
            Software = response.Software,
            Description = response.Description,
            Logged = response.Logged,
            Status = response.Status,
            Support = new SupportResponse(support.Name, support.Phone)
        };

        return Ok(model);
    }

    [HttpGet("/issues")]
    public async Task<ActionResult> GetAllIssues()
    {
        var response = await session.Query<IssueResponse>().ToListAsync();
        return Ok(new IssuesResponseCollection(response));
    }

    [HttpGet("/issues/{id}")]
    public async Task<ActionResult> GetIssueByID(Guid id)
    {
        var response = await session.Query<IssueResponse>().Where(issue => issue.Id == id).SingleOrDefaultAsync();
        if (response is null)
        {
            return NotFound();
        }
        else
        {
            return Ok(response);
        }
    }
}


public record IssueRequest(string Software, string Description);
public record IssueResponse(Guid Id, string Software, string Description, DateTimeOffset Logged, IssueStatus Status);
public enum IssueStatus { Pending };
public record IssuesResponseCollection(IReadOnlyList<IssueResponse> Data);

public record IssueResponseSupportInformation(string Message, string Name, string Phone);

public record IssueResponseModel
{
    public Guid Id { get; set; }
    public string Software { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset Logged { get; set; }
    public IssueStatus Status { get; set; }
    public SupportResponse? Support { get; init; }
}