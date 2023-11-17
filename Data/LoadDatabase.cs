using Microsoft.AspNetCore.Identity;
using NetKubernetes.Models;

namespace NetKubernetes.Data;

public class LoadDatabase{
    public static async Task InsertarData(AppDbContext context, UserManager<Usuario> usuarioManager){

        if (!usuarioManager.Users.Any())
        {
            var usuario = new Usuario{
                Nombre = "Cesar",
                Apellido = "Torres",
                Email = "cesar@gmail.com",
                UserName = "cesar.torres",
                Telefono = "8115478569"
            };
            await usuarioManager.CreateAsync(usuario, "PasswordCesarTorres2023$");
        }

        if (!context.Inmuebles!.Any())
        {
            context.Inmuebles!.AddRange(
                new Inmueble{
                    Nombre = "Casa de playa",
                    Direccion = "Av. El sol 52",
                    Precio = 4500M,
                    FechaCreacion = DateTime.Now
                },
                new Inmueble{
                    Nombre = "Casa de invierno",
                    Direccion = "Av. El roca 82",
                    Precio = 3500M,
                    FechaCreacion = DateTime.Now
                }
            );
        }
        context.SaveChanges();
    }
}