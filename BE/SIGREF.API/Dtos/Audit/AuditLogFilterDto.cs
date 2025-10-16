namespace SIGREF.API.Dtos.AuditLog
{
    public class AuditLogFilterDto
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? ActionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public void NormalizeDates()
        {
            // Si ambas fechas están presentes, validar que StartDate <= EndDate
            if (StartDate.HasValue && EndDate.HasValue)
            {
                if (StartDate.Value > EndDate.Value)
                {
                    throw new ArgumentException("La fecha inicial no puede ser mayor que la fecha final.");
                }
                return;
            }

            // Si solo una fecha está presente, lanzar excepción
            if (StartDate.HasValue || EndDate.HasValue)
            {
                throw new ArgumentException("Debe proporcionar ambas fechas (StartDate y EndDate) o ninguna para usar el rango por defecto del último mes.");
            }

            // Si no se proporcionan fechas, establecer el último mes por defecto
            EndDate = DateTime.UtcNow;
            StartDate = EndDate.Value.AddMonths(-1);
        }
    }
}