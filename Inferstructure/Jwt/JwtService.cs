using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DominClass.Entities.User;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public interface IJwtService
{
    string CreateToken(User user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName)
            // Add other claims as needed
        };
        var jwtKey = _configuration["Jwt:Key"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(jwtIssuer) ||
            string.IsNullOrWhiteSpace(jwtAudience))
            // Handle missing or invalid configuration values here
            throw new InvalidOperationException("JWT configuration is missing or invalid.");

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(30), // Token expiration time
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}