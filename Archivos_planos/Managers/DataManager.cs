using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Archivos_planos.Models;
using System.Globalization;

namespace Archivos_planos.Managers
{
    public static class DataManager
    {
        private const string PersonasFilePath = "Data/datos_personas.txt";
        private static List<Persona> _personas = new List<Persona>();

        public static List<Persona> CargarPersonas()
        {
            _personas = new List<Persona>();
            if (!File.Exists(PersonasFilePath))
            {
                Console.WriteLine($"Advertencia: Archivo de personas no encontrado en {PersonasFilePath}. Se creará al guardar.");
                return _personas;
            }

            var lines = File.ReadAllLines(PersonasFilePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 6) continue;

                if (int.TryParse(parts[0], out int id) &&
                    decimal.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal balance))
                {
                    _personas.Add(new Persona
                    {
                        Id = id,
                        Nombres = parts[1].Trim(),
                        Apellidos = parts[2].Trim(),
                        Telefono = parts[3].Trim(),
                        Ciudad = parts[4].Trim(),
                        Balance = balance
                    });
                }
                else
                {
                    Console.WriteLine($"Advertencia: Línea con formato inválido omitida: {line}");
                }
            }
            return _personas;
        }

        public static void GuardarPersonas(string usuarioEjecuta)
        {
            var lines = _personas.Select(p =>
                $"{p.Id},{p.Nombres},{p.Apellidos},{p.Telefono},{p.Ciudad},{p.Balance:0.00}");

            File.WriteAllLines(PersonasFilePath, lines);
            LogManager.EscribirLog(usuarioEjecuta, "Persistencia de datos: Cambios guardados en datos_personas.txt.");
            Console.WriteLine("\nCAMBIOS GUARDADOS exitosamente.");
        }

        public static void MostrarContenido(string usuarioEjecuta)
        {
            if (!_personas.Any())
            {
                Console.WriteLine("No hay personas registradas.");
                _personas = CargarPersonas();
                if (!_personas.Any()) return;
            }

            Console.WriteLine("\n========================================================");
            Console.WriteLine("ID\tNombres\t\tTeléfono\tCiudad\t\tBalance");
            Console.WriteLine("—\t———————\t\t———————\t\t———————\t\t———————");

            foreach (var p in _personas)
            {
                Console.WriteLine($"{p.Id}\t{p.Nombres} {p.Apellidos}\t{p.Telefono}\t{p.Ciudad}\t\t{p.Balance:N2}");
            }
            Console.WriteLine("========================================================");
            LogManager.EscribirLog(usuarioEjecuta, "Visualización de contenido de personas.");
        }


        private static bool ValidarTelefono(string telefono)
        {
            return Regex.IsMatch(telefono, @"^\d{7,15}$");
        }

        private static bool ValidarPersona(int id, string nombres, string apellidos, string telefono, decimal balance, bool esNuevo)
        {
            if (esNuevo && _personas.Any(p => p.Id == id))
            {
                Console.WriteLine("Error: El ID debe ser único y ya existe.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(apellidos))
            {
                Console.WriteLine("Error: Se deben ingresar nombres y apellidos.");
                return false;
            }

            if (!ValidarTelefono(telefono))
            {
                Console.WriteLine("Error: El teléfono no es válido. Debe contener solo números y tener una longitud razonable (ej. 7-15 dígitos).");
                return false;
            }

            if (balance <= 0)
            {
                Console.WriteLine("Error: El saldo (balance) debe ser un número positivo.");
                return false;
            }

            return true;
        }

        public static void AgregarPersona(string usuarioEjecuta)
        {
            int id = 0;
            decimal balance = 0;
            string nombres = string.Empty, apellidos = string.Empty, telefono = string.Empty, ciudad = string.Empty;
            bool idValido = false;

            Console.WriteLine("\n--- Agregar Nueva Persona ---");

            while (!idValido)
            {
                Console.Write("Ingrese ID (debe ser único y numérico): ");
                if (int.TryParse(Console.ReadLine(), out id))
                {
                    if (!_personas.Any(p => p.Id == id))
                    {
                        idValido = true;
                    }
                    else
                    {
                        Console.WriteLine("Error: El ID ya existe. Intente con otro.");
                    }
                }
                else
                {
                    Console.WriteLine("Error: El ID debe ser un número.");
                }
            }

            Console.Write("Ingrese Nombres: ");
            nombres = Console.ReadLine() ?? string.Empty;
            Console.Write("Ingrese Apellidos: ");
            apellidos = Console.ReadLine() ?? string.Empty;
            Console.Write("Ingrese Teléfono: ");
            telefono = Console.ReadLine() ?? string.Empty;
            Console.Write("Ingrese Ciudad: ");
            ciudad = Console.ReadLine() ?? string.Empty;

            bool balanceValido = false;
            while (!balanceValido)
            {
                Console.Write("Ingrese Saldo (Balance, ej: 1500.00): ");
                if (decimal.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out balance))
                {
                    balanceValido = true;
                }
                else
                {
                    Console.WriteLine("Error: El saldo debe ser un número válido.");
                }
            }

            if (ValidarPersona(id, nombres, apellidos, telefono, balance, true))
            {
                _personas.Add(new Persona
                {
                    Id = id,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Telefono = telefono,
                    Ciudad = ciudad,
                    Balance = balance
                });
                Console.WriteLine($"\nPersona ID {id} agregada temporalmente. ¡Recuerde Guardar Cambios!");
                LogManager.EscribirLog(usuarioEjecuta, $"Persona agregada: ID {id} - {nombres} {apellidos}.");
            }
            else
            {
                Console.WriteLine("\nNo se pudo agregar la persona debido a errores de validación.");
            }
        }

        public static void EditarPersona(string usuarioEjecuta)
        {
            Console.WriteLine("\n--- Editar Persona ---");
            Console.Write("Ingrese el ID de la persona a editar: ");

            if (!int.TryParse(Console.ReadLine(), out int idBuscado))
            {
                Console.WriteLine("Error: El ID debe ser un número.");
                return;
            }

            var personaAEditar = _personas.FirstOrDefault(p => p.Id == idBuscado);

            if (personaAEditar == null)
            {
                Console.WriteLine($"Error: No se encontró ninguna persona con el ID {idBuscado}.");
                return;
            }

            Console.WriteLine($"\nPersona encontrada: {personaAEditar.Nombres} {personaAEditar.Apellidos}");
            Console.WriteLine("Presione ENTER para mantener el valor actual o ingrese el nuevo valor.");

            // Nombres
            Console.Write($"Nombres ({personaAEditar.Nombres}): ");
            string? nuevoNombres = Console.ReadLine();
            if (!string.IsNullOrEmpty(nuevoNombres))
            {
                personaAEditar.Nombres = nuevoNombres;
            }

            // Apellidos
            Console.Write($"Apellidos ({personaAEditar.Apellidos}): ");
            string? nuevoApellidos = Console.ReadLine();
            if (!string.IsNullOrEmpty(nuevoApellidos))
            {
                personaAEditar.Apellidos = nuevoApellidos;
            }

            // Teléfono
            Console.Write($"Teléfono ({personaAEditar.Telefono}): ");
            string? nuevoTelefono = Console.ReadLine();
            if (!string.IsNullOrEmpty(nuevoTelefono))
            {
                if (ValidarTelefono(nuevoTelefono))
                {
                    personaAEditar.Telefono = nuevoTelefono;
                }
                else
                {
                    Console.WriteLine("Advertencia: Valor inválido. Se mantendrá el teléfono previo.");
                }
            }

            Console.Write($"Ciudad ({personaAEditar.Ciudad}): ");
            string? nuevaCiudad = Console.ReadLine();
            if (!string.IsNullOrEmpty(nuevaCiudad))
            {
                personaAEditar.Ciudad = nuevaCiudad;
            }

            // Balance (Saldo)
            Console.Write($"Saldo/Balance ({personaAEditar.Balance:N2}): ");
            string? nuevoBalanceStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(nuevoBalanceStr))
            {
                if (decimal.TryParse(nuevoBalanceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal nuevoBalance) && nuevoBalance > 0)
                {
                    personaAEditar.Balance = nuevoBalance;
                }
                else
                {
                    Console.WriteLine("Advertencia: Valor inválido. El saldo debe ser un número positivo. Se mantendrá el balance previo.");
                }
            }

            if (string.IsNullOrWhiteSpace(personaAEditar.Nombres) || string.IsNullOrWhiteSpace(personaAEditar.Apellidos))
            {
                Console.WriteLine("\nError crítico: Nombres y Apellidos son obligatorios y no pueden quedar vacíos.");
            }
            else
            {
                Console.WriteLine($"\nPersona ID {idBuscado} actualizada temporalmente. ¡Recuerde Guardar Cambios!");
                LogManager.EscribirLog(usuarioEjecuta, $"Persona editada: ID {idBuscado} - {personaAEditar.Nombres} {personaAEditar.Apellidos}.");
            }
        }

        public static void BorrarPersona(string usuarioEjecuta)
        {
            Console.WriteLine("\n--- Borrar Persona ---");
            Console.Write("Ingrese el ID de la persona a borrar: ");

            if (!int.TryParse(Console.ReadLine(), out int idBuscado))
            {
                Console.WriteLine("Error: El ID debe ser un número.");
                return;
            }

            var personaABorrar = _personas.FirstOrDefault(p => p.Id == idBuscado);

            if (personaABorrar == null)
            {
                Console.WriteLine($"Error: No se encontró ninguna persona con el ID {idBuscado}.");
                return;
            }

            Console.WriteLine("\nDatos de la persona a borrar:");
            Console.WriteLine($"ID: {personaABorrar.Id}");
            Console.WriteLine($"Nombres: {personaABorrar.Nombres} {personaABorrar.Apellidos}");
            Console.WriteLine($"Teléfono: {personaABorrar.Telefono}");
            Console.WriteLine($"Ciudad: {personaABorrar.Ciudad}");
            Console.WriteLine($"Balance: {personaABorrar.Balance:N2}");
            Console.WriteLine("-----------------------------------");

            Console.Write("¿Está seguro que desea borrar esta persona? (S/N): ");
            string? confirmacion = Console.ReadLine();

            if (confirmacion?.Trim().ToUpper() == "S")
            {
                _personas.Remove(personaABorrar);
                Console.WriteLine($"\nPersona ID {idBuscado} eliminada temporalmente. ¡Recuerde Guardar Cambios!");

                LogManager.EscribirLog(usuarioEjecuta, $"Persona eliminada: ID {idBuscado} - {personaABorrar.Nombres} {personaABorrar.Apellidos}.");
            }
            else
            {
                Console.WriteLine("\nOperación de borrado cancelada.");
            }
        }

        public static void MostrarInforme(string usuarioEjecuta)
        {
            if (!_personas.Any())
            {
                Console.WriteLine("No hay personas registradas para generar el informe.");
                return;
            }

            Console.WriteLine("\n--- INFORME DE BALANCES POR CIUDAD ---");

            var informePorCiudad = _personas
                .GroupBy(p => p.Ciudad)
                .Select(g => new
                {
                    Ciudad = g.Key,
                    Personas = g.ToList(),
                    TotalCiudad = g.Sum(p => p.Balance)
                })
                .OrderBy(i => i.Ciudad);

            decimal totalGeneral = 0;

            foreach (var grupo in informePorCiudad)
            {
                Console.WriteLine($"\nCiudad: {grupo.Ciudad}\n");

                Console.WriteLine("ID\tNombres\t\tApellidos\tSaldo");
                Console.WriteLine("—\t———————\t\t———————\t\t———————");

                foreach (var p in grupo.Personas.OrderBy(p => p.Id))
                {
                    Console.WriteLine($"{p.Id}\t{p.Nombres}\t\t{p.Apellidos}\t\t{p.Balance:N2}");
                }

                Console.WriteLine("\t\t\t\t\t=======");
                Console.WriteLine($"Total: {grupo.Ciudad}\t\t\t\t{grupo.TotalCiudad:N2}");

                totalGeneral += grupo.TotalCiudad;
            }

            Console.WriteLine("\n\t\t\t\t\t=======");
            Console.WriteLine($"Total General:\t\t\t\t{totalGeneral:N2}");
            Console.WriteLine("--------------------------------------");

            LogManager.EscribirLog(usuarioEjecuta, "Generación de informe con subtotales.");
        }
    }
}