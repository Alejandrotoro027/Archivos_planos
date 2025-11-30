using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archivos_planos.Models;

namespace Archivos_planos.Managers
{
    public static class AuthManager
    {

        private static readonly string ProjectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
        private static readonly string UsersFilePath = Path.Combine(ProjectRoot, "Data/Users.txt");


        private static List<Usuario> _usuarios = new List<Usuario>();

        public static List<Usuario> CargarUsuarios()
        {
            _usuarios = new List<Usuario>();
            if (!File.Exists(UsersFilePath))
            {
                Console.WriteLine($"Error: Archivo de usuarios no encontrado en {UsersFilePath}.");
                return _usuarios;
            }

            var lines = File.ReadAllLines(UsersFilePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                if (bool.TryParse(parts[2].Trim(), out var activo))
                {
                    var usuario = new Usuario
                    {
                        NombreUsuario = parts[0].Trim(),
                        Contrasena = parts[1].Trim(),
                        Activo = activo
                    };

                    if (parts.Length >= 4 && int.TryParse(parts[3].Trim(), out int intentos))
                    {
                        usuario.IntentosFallidos = intentos;
                    }
                    else
                    {
                        usuario.IntentosFallidos = 0;
                    }
                    _usuarios.Add(usuario);
                }
            }
            return _usuarios;
        }

        public static void GuardarUsuarios()
        {
            try
            {
                var lines = _usuarios.Select(u =>
                    $"{u.NombreUsuario},{u.Contrasena},{u.Activo.ToString().ToLower()},{u.IntentosFallidos}");

                File.WriteAllLines(UsersFilePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR CRÍTICO DE ARCHIVO] No se pudo guardar Users.txt: {ex.Message}");
            }
        }

        public static Usuario? Autenticar()
        {
            if (!_usuarios.Any())
            {
                _usuarios = CargarUsuarios();
            }

            Console.WriteLine("===============================");
            Console.WriteLine("    Sistema de Autenticación");
            Console.WriteLine("===============================");

            while (true)
            {
                Console.Write("Ingrese usuario: ");
                string? user = Console.ReadLine();
                Console.Write("Ingrese contraseña: ");
                string? pass = Console.ReadLine();

                var usuarioEncontrado = _usuarios.FirstOrDefault(u => u.NombreUsuario.Equals(user, StringComparison.OrdinalIgnoreCase));

                if (usuarioEncontrado != null)
                {
                    if (!usuarioEncontrado.Activo)
                    {
                        LogManager.EscribirLog(usuarioEncontrado.NombreUsuario, "Intento de acceso denegado (usuario bloqueado).");
                        Console.WriteLine("USUARIO BLOQUEADO. No se permite la entrada.");
                        continue;
                    }

                    if (usuarioEncontrado.Contrasena == pass)
                    {
                        LogManager.EscribirLog(usuarioEncontrado.NombreUsuario, "Autenticación exitosa.");
                        Console.WriteLine($"¡Bienvenido, {usuarioEncontrado.NombreUsuario}!");
                        usuarioEncontrado.IntentosFallidos = 0;
                        GuardarUsuarios();
                        return usuarioEncontrado;
                    }
                    else
                    {
                        usuarioEncontrado.IntentosFallidos++;
                        LogManager.EscribirLog(usuarioEncontrado.NombreUsuario, $"Intento de contraseña fallido ({usuarioEncontrado.IntentosFallidos} de 3).");
                        Console.WriteLine($"Contraseña incorrecta. Intento {usuarioEncontrado.IntentosFallidos} de 3.");

                        if (usuarioEncontrado.IntentosFallidos >= 3)
                        {
                            usuarioEncontrado.Activo = false;
                            GuardarUsuarios();
                            LogManager.EscribirLog(usuarioEncontrado.NombreUsuario, "USUARIO BLOQUEADO por fallos de contraseña.");
                            Console.WriteLine("ATENCIÓN: Ha fallado 3 veces. El usuario ha sido BLOQUEADO.");
                        }
                        else
                        {
                            GuardarUsuarios();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Usuario o contraseña incorrectos.");
                }

                Console.WriteLine("-------------------------------");
            }
        }
    }
}
