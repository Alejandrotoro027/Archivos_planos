using System;
using System.IO;

namespace Archivos_planos.Managers
{
    public static class LogManager
    {
        private const string LogFilePath = "Data/log.txt";

        public static void EscribirLog(string nombreUsuario, string operacion)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - USUARIO: {nombreUsuario} - OPERACIÓN: {operacion}";

            try
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir en el log: {ex.Message}");
            }
        }
    }
}