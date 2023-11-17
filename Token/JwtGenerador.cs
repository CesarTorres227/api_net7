using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NetKubernetes.Models;

namespace NetKubernetes.Token;

public class JwtGenerador : IJwtGenerador
{
    public string CrearToken(Usuario usuario)
    {
        var claims = new List<Claim>{
            new Claim(JwtRegisteredClaimNames.NameId, usuario.UserName!),
            new Claim("userId", usuario.Id),
            new Claim("email", usuario.Email!)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("qkU3YOX{)/pa#>OVea;1=iBa>rv6}pz]G*!l'qv^.~<z-Z))0QuhZfK-g*@$-Wa9A4997AB5D38BBD4C82EA41533992"));

        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescripcion = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(30),
            SigningCredentials = credenciales
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescripcion);

        return tokenHandler.WriteToken(token);

    }
}