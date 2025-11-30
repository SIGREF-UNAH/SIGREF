using SIGREF.API.Database.Entity.Cashier;
using SIGREF.API.Dtos.Cashier;

namespace SIGREF.API.Extensions;

public static class ShiftExtensions
{
    public static ShiftDto ToDto(this ShiftEntity entity)
    {
        if (entity == null)
            return null;

        return new ShiftDto
        {
            Id = entity.Id,
            LocationId = entity.LocationId,
            Name = entity.Name,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            IsActive = entity.IsActive,
            
        };
    }
    public static ShiftEntity ApplyUpdate(this ShiftEntity entity, UpdateShiftDto dto , Guid cashierId)
    {
        if (dto.LocationId.HasValue)
            entity.LocationId = dto.LocationId.Value;
        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;
        
        if (dto.StartTime.HasValue)
            entity.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue)
            entity.EndTime = dto.EndTime.Value;

        // ================================
        //   Auditoría
        // ================================
        entity.UpdatedDate = DateTime.UtcNow;
        entity.UpdatedById = cashierId; 

        return entity;
    }
}