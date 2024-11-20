using Core.Sessions.Models;
using Core.Sessions.Validators;

namespace UnitTest.Core.Sessions;

public class CreateSessionDtoValidatorTest
{
	[Theory]
	[InlineData("")]
	[InlineData("jdhsjekdjasnfjjdhsjekdjasnfjjdhsjekdjasnfjjdhsjekdjasnfjjdhsjekdjasnfjjdhsjekdjasnfjjdhsjekdjasnfjjdhsjekdjasnfjjdhsjekdjasnfjjdhsjekdjasnfj")]
	[InlineData("       ")]
	[InlineData(" ")]
	[InlineData("#(/&==(")]
	public void ValidateTitle_ShouldReturn_Fail(string title)
	{
		var validator = new CreateSessionDtoValidator();
		var dto = new CreateSessionDto(title, null, 1, new List<int>{1,2}, new List<int> { 1 });
		
		var result = validator.Validate(dto);
		
		Assert.False(result.IsValid);
		
	}
	[Theory]
	[InlineData("3 Words Count")]
	[InlineData("Reverse list")]
	[InlineData("GET ALL ITEMS")]
	[InlineData("TWO-Sum")]
	[InlineData("5")]
	public void ValidateTitle_ShouldReturn_Ok(string title)
	{
		var validator = new CreateSessionDtoValidator();
		var dto = new CreateSessionDto(title, null, 1, new List<int>{1,2}, new List<int> { 1 });
		
		var result = validator.Validate(dto);
		
		Assert.True(result.IsValid);
	}

	[Fact]
	public void ValidateDescription_ShouldReturn_Fail()
	{
		var validator = new CreateSessionDtoValidator();
		var desc = "";
		for (int i = 0; i < 1001; i++)
		{
			desc += "a";
		}
		var dto = new CreateSessionDto("MyTitle", desc, 1, new List<int>{1,2}, new List<int> { 1 });
		
		var result = validator.Validate(dto);
		
		Assert.False(result.IsValid);
	}
	[Fact]
	public void ValidateDescription_ShouldReturn_OkMaxCapacity()
	{
		var validator = new CreateSessionDtoValidator();
		var desc = "";
		for (int i = 0; i < 1000; i++)
		{
			desc += "a";
		}
		var dto = new CreateSessionDto("MyTitle", desc, 1, new List<int>{1,2}, new List<int> { 1 });
		
		var result = validator.Validate(dto);
		
		Assert.True(result.IsValid);
	}
	[Fact]
	public void ValidateDescription_ShouldReturn_OkNull()
	{
		var validator = new CreateSessionDtoValidator();
		var dto = new CreateSessionDto("MyTitle", null, 1, new List<int>{1,2}, new List<int> { 1 });
		
		var result = validator.Validate(dto);
		
		Assert.True(result.IsValid);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(11)]
	[InlineData(-1)]
	[InlineData(12)]
	[InlineData(200)]
	public void ValidateExpiresInHours_ShouldReturn_Fail(int expiresInHours)
	{
		var validator = new CreateSessionDtoValidator();
		var dto = new CreateSessionDto("MyTitle", null, expiresInHours, new List<int>{1,2}, new List<int> { 1 });
		
		var result = validator.Validate(dto);
		
		Assert.False(result.IsValid);
	}
	
	[Theory]
	[InlineData(1)]
	[InlineData(10)]
	[InlineData(4)]
	[InlineData(5)]
	public void ValidateExpiresInHours_ShouldReturn_OK(int expiresInHours)
	{
		var validator = new CreateSessionDtoValidator();
		var dto = new CreateSessionDto("MyTitle", null, expiresInHours, new List<int>{1,2}, new List<int> { 1 });
		
		var result = validator.Validate(dto);
		
		Assert.True(result.IsValid);
	}

	[Fact]
	public void ValidateExerciseIds_ShouldReturn_Fail()
	{
		var validator = new CreateSessionDtoValidator();
		var dto = new CreateSessionDto("MyTitle", null, 1, new List<int>{}, new List<int> { 1 });
		
		var result = validator.Validate(dto);
		
		Assert.False(result.IsValid);
	}
	
	[Fact]
	public void ValidateExerciseIds_ShouldReturn_Ok()
	{
		var validator = new CreateSessionDtoValidator();
		var dto = new CreateSessionDto("MyTitle", null, 1, new List<int>{1}, new List<int> { 1 });
		
		var result = validator.Validate(dto);
		
		Assert.True(result.IsValid);
	}
}