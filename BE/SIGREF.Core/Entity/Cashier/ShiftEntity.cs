using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Cashier;
// crearTurno
// ui nos envia  location_id y propiedades del turno
// vallidar  ue la location exista en fhir y esté habilitada (opcional)
// se manda a crear la entidad a nuestra db

public class ShiftEntity : BaseEntity
{
    // ======================================
    //             RELACIÓN
    // ======================================

    public string LocationId { get; set; }


    // ======================================
    //         INFORMACIÓN DEL TURNO
    // ======================================

    public string Name { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public DateTime CorrectionClosure { get; set; }
}