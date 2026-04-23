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
    internal const string Reset  = "[0m";
    internal const string Green  = "[32m";
    internal const string Blue   = "[34m";
    internal const string Red    = "[31m";
    internal const string Cyan   = "[36m";
    internal const string Yellow = "[33m";
    internal const string Gray   = "[90m";
}
