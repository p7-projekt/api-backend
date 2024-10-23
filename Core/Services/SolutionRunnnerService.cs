using Core.Contracts.Services;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;
using Core.Models.DTOs;

namespace Core.Services;

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
