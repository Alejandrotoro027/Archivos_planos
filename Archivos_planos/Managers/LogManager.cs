using System;
using System.IO;

namespace Archivos_planos.Managers
{
    public static class LogManager
    {
        private static readonly string ProjectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
        private static readonly string LogFilePath = Path.Combine(ProjectRoot, "Data/log.txt");


        public static void EscribirLog(string nombreUsuario, string operacion)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - USUARIO: {nombreUsuario} - OPERACIÓN: {operacion}";

            try
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR DE LOG] Fallo al escribir en log.txt: {ex.Message}");
            }
        }
    }
}
