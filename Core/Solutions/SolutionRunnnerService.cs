using System.Text;
using System.Text.Json;
using Core.Contracts.Services;
using Core.Exercises.Models;

namespace Core.Solutions;

public class SolutionRunnnerService : ISolutionRunnerService
{
    public async Task SubmitSolutionAsync(ExerciseDto dto)
    {
        var url = "http://127.0.0.1:8080/submit";
        using var client = new HttpClient();

        var submission = JsonSerializer.Serialize(new Submission(dto));
        
        var content = new StringContent(submission, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        string responseBody = await response.Content.ReadAsStringAsync();
        //Managge potentiel responses from solution runner.
    }
}
