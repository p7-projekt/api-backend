using Core.Sessions.Models;
using Core.Sessions.Validators;

namespace UnitTest.Core.Sessions;

public class JoinSessionDtoValidatorTest
{
    [Theory]
    [InlineData("DF1000")]
    [InlineData("AA0000")]
    [InlineData("ZZ9999")]
    [InlineData("KS4324")]
    public void JoinSessionValidator_ShouldReturn_Ok(string SessionCode)
    {
        var validator = new JoinSessionDtoValidator();
        var joinSessionDto = new JoinSessionDto(SessionCode, "lars");
        
        var result = validator.Validate(joinSessionDto);
        
        Assert.True(result.IsValid);
    }
    
    [Theory]
    [InlineData("ÆÆ")]
    [InlineData("Ks1000")]
    [InlineData("sK1000")]
    [InlineData("ss9213")]
    [InlineData("SSS109")]
    [InlineData("SSSS10")]
    [InlineData("SAAAA1")]
    [InlineData("AFAASD")]
    [InlineData("A12321")]
    [InlineData("134632")]
    [InlineData("  4632")]
    [InlineData("##4632")]
    [InlineData("KK4S32")]
    [InlineData("KK41S2")]
    [InlineData("KK419K")]
    public void JoinSessionValidator_ShouldReturn_Fail(string SessionCode)
    {
        var validator = new JoinSessionDtoValidator();
        var joinSessionDto = new JoinSessionDto(SessionCode, "lars");
        
        var result = validator.Validate(joinSessionDto);
        
        Assert.False(result.IsValid);
    }
}