using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetKubernetes.Dtos.UsuarioDto;
using NetKubernetes.Middleware;
using NetKubernetes.Models;
using NetKubernetes.Token;

namespace NetKubernetes.Data.Usuarios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _contexto;
    private readonly IUsuarioSesion _usuarioSesion;
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IJwtGenerador _jwtGenerador;

    public UsuarioRepository(AppDbContext contexto,
                              IUsuarioSesion usuarioSesion,
                              UserManager<Usuario> userManager,
                              SignInManager<Usuario> signInManager,
                              IJwtGenerador jwtGenerador)
    {
        _contexto = contexto;
        _usuarioSesion = usuarioSesion;
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtGenerador = jwtGenerador;
    }

    private UsuarioResponseDto TransformerUserToUserDto(Usuario usuario)
    {
        return new UsuarioResponseDto 
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Telefono = usuario.Telefono,
            Email = usuario.Email,
            UserName = usuario.UserName,
            Token = _jwtGenerador.CrearToken(usuario)
        };
    }

    public async Task<UsuarioResponseDto> GetUsuario()
    {
        var usuario = await _userManager.FindByNameAsync(_usuarioSesion.ObtenerUsuarioSesion());

        if (usuario is null)
        {
            throw new MiddlewareException(
                HttpStatusCode.Unauthorized, 
                new {mensaje = "El usuario del token no existe en la base de datos"}
            );
        }

        return TransformerUserToUserDto(usuario!);
    }

    public async Task<UsuarioResponseDto> Login(UsuarioLoginRequestDto request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email!);

        if (usuario is null)
        {
            throw new MiddlewareException(
                HttpStatusCode.Unauthorized, 
                new {mensaje = "El email del usuario no existe en la base de datos"}
            );
        }

        var resultado = await _signInManager.CheckPasswordSignInAsync(usuario!, request.Password!, false);

        if (resultado.Succeeded)
        {
            return TransformerUserToUserDto(usuario!); 
        }

        throw new MiddlewareException(
            HttpStatusCode.Unauthorized,
            new {mesaje = "Las credenciales son incorrectas"}
        );
    }

    public async Task<UsuarioResponseDto> RegistroUsuario(UsuarioRegistroRequestDto request)
    {
        var existeEmail = await _contexto.Users.Where(x => x.Email == request.Email).AnyAsync();

        if (existeEmail)
        {
            throw new MiddlewareException(
                HttpStatusCode.BadRequest,
                new {mesaje = "El email del usuario ya existe en la base de datos"}
            );
        }

        var existeUserName = await _contexto.Users.Where(x => x.UserName == request.UserName).AnyAsync();

        if (existeUserName)
        {
            throw new MiddlewareException(
                HttpStatusCode.BadRequest,
                new {mesaje = "El username del usuario ya existe en la base de datos"}
            );
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Telefono = request.Telefono,
            Email = request.Email,
            UserName = request.UserName
        };

        var resultado = await _userManager.CreateAsync(usuario!, request.Password!);

        if (resultado.Succeeded)
        {
            return TransformerUserToUserDto(usuario);
        }

        throw new Exception("No se pudo registrar el usuario");
    }
}