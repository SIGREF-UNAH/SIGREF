namespace SIGREF.API.Utils;

/// <summary>
/// Códigos de escape ANSI para colorizar la salida de logs en terminales
/// y contenedores Docker que soporten secuencias ANSI (ej. Seq, Grafana Loki, kubectl logs).
/// </summary>
/// <remarks>
/// Centraliza las constantes de color para evitar duplicación de estos
/// No aplicar en entornos donde la salida sea redirigida a archivos planos
/// sin soporte ANSI, ya que los códigos aparecerán como caracteres literales.
/// </remarks>
internal static class AnsiColors
{
    internal const string Reset  = "\u001b[0m";
    internal const string Green  = "\u001b[32m";
    internal const string Blue   = "\u001b[34m";
    internal const string Red    = "\u001b[31m";
    internal const string Cyan   = "\u001b[36m";
    internal const string Yellow = "\u001b[33m";
    internal const string Gray   = "\u001b[90m";
}
