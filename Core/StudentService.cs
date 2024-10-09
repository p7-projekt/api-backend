using System.Text;

namespace Core;

public class StudentService
{
    
    readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }


    public Task HentNogleStudents()
    {
        var url = "http://127.0.0.1:8080/submit";
        using var client = new HttpClient();
        var testcase =
            """
            {
              "solution": "solution x =\n  if x < 0\n    then x \n    else x",
              "testCases": [
                {
                  "id": 0,
                  "inputParameters": [
                    {
                      "valueType": "integer",
                      "value": "-5"
                    }
                  ],
                  "outputParameters": [
                    {
                      "valueType": "integer",
                      "value": "5"
                    }
                  ]
                },
                {
                  "id": 1,
                  "inputParameters": [
                    {
                      "valueType": "integer",
                      "value": "5"
                    }
                  ],
                  "outputParameters": [
                    {
                      "valueType": "integer",
                      "value": "5"
                    }
                  ]
                }
              ]
            }
            """;
        var content = new StringContent(testcase, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        // Display the response
        Console.WriteLine("Response: " + responseBody);
        await _studentRepository.GetStudentsAsync();
    }
}