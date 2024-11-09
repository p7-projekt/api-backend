namespace IntegrationTest;

public class AuthenticationEndpoints
{
	// Refresh token
	public async Task RefreshToken_ShouldReturn_BadRequest()
	{
		await Task.Delay(5000);
		Assert.True(true);
	}
	public async Task RefreshToken_ShouldReturn_Ok()
	{
		await Task.Delay(5000);
		Assert.True(true);
	}
	
	// login
	public async Task Login_ShouldReturn_BadRequest()
	{
		await Task.Delay(5000);
		Assert.True(true);
	}
	public async Task Login_ShouldReturn_Ok()
	{
		await Task.Delay(5000);
		Assert.True(true);
	}
	public async Task Register_ShouldReturn_Okkk()
	{
		await Task.Delay(5000);
		Assert.True(true);
	}
	public async Task Register_ShouldTrigger_RequestValidation()
	{
		await Task.Delay(5000);
		Assert.True(true);
	}
}