namespace IssueTrackerApi.Services;

public class BusinessClockHttpService(HttpClient client)
{
    public async Task<SupportResponse> GetCurrentSupportInformationAsync()
    {
        var response = await client.GetAsync("/support-info");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<SupportResponse>();
        return body ?? new SupportResponse("asdf", "1234");
    }
}


public record SupportResponse(string Name, string Phone);