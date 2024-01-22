using System.IdentityModel.Tokens.Jwt;

namespace Boostan.Application.Models.Jwt;

public class AccessToken
{
    public AccessToken(JwtSecurityToken securityToken, string refreshToken = "")
    {
        access_token = new JwtSecurityTokenHandler().WriteToken(securityToken);
        token_type = "Bearer";
        expires_in = (int)(securityToken.ValidTo - DateTime.UtcNow).TotalSeconds;
    }

    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
}