using System;
using Archivos_planos.Managers;
using Archivos_planos.Models;

namespace Archivos_planos
{
    class Program
    {
        static void Main(string[] args)
        {
            Usuario? usuarioActual = AuthManager.Autenticar();

            if (usuarioActual != null)
            {
                Console.Clear();
                Console.WriteLine($"Acceso concedido para {usuarioActual.NombreUsuario}.");

                DataManager.CargarPersonas();

                EjecutarMenuPrincipal(usuarioActual);
            }
            else
            {
                Console.WriteLine("Acceso denegado. Saliendo del programa.");
            }
        }

        static void EjecutarMenuPrincipal(Usuario usuario)
        {
            string? opcion = "";
            while (opcion != "0")
            {
                Console.WriteLine("\n==========================================");
                Console.WriteLine("1. Show content");
                Console.WriteLine("2. Add person");
                Console.WriteLine("3. Save changes");
                Console.WriteLine("4. Edit person");
                Console.WriteLine("5. Delete person");
                Console.WriteLine("6. Mostrar Informe");
                Console.WriteLine("0. Exit");
                Console.WriteLine("==========================================");
                Console.Write("Choose an option: ");
                opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        DataManager.MostrarContenido(usuario.NombreUsuario);
                        break;
                    case "2":
                        DataManager.AgregarPersona(usuario.NombreUsuario);
                        break;
                    case "3":
                        DataManager.GuardarPersonas(usuario.NombreUsuario);
                        break;
                    case "4":
                        DataManager.EditarPersona(usuario.NombreUsuario);
                        break;
                    case "5":
                        DataManager.BorrarPersona(usuario.NombreUsuario);
                        break;
                    case "6":
                        DataManager.MostrarInforme(usuario.NombreUsuario);
                        break;
                    case "0":
                        Console.WriteLine("Saliendo del programa. ¡Adiós!");
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }
    }
}