using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Dtos.Administration;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Extensions;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.AdministrationHospital;

public class HospitalPropertiesService : IHospitalPropertiesService
{
    private readonly SIGREFContext _context;

    public HospitalPropertiesService(SIGREFContext context)
    {
        _context = context;
    }

    // ============================================================
    //                 GET PUBLICO
    // ============================================================
    // Todos pueden acceder a este y manda los datos minimo
    public async Task<ResponseDto<HospitalPublicDto>> GetPublicAsync()
    {
        var hospital = await _context.HospitalProperties
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsSingleton);

        if (hospital == null)
        {
            return new ResponseDto<HospitalPublicDto>
            {
                Status = false,
                Message = "Hospital no configurado.",
                StatusCode = 404,
                Data = null
            };
        }

        return new ResponseDto<HospitalPublicDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Datos públicos obtenidos correctamente.",
            Data = new HospitalPublicDto
            {
                Name = hospital.Name,
                UrlLogo = hospital.UrlLogo,
                UrlLogoHealth = hospital.UrlLogoHealth
            }
        };
    }

    public async Task<ResponseDto<HospitalDetailsDto>> GetAllDetailsAsync()
    {
        var hospital = await _context.HospitalProperties
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsSingleton);

        if (hospital == null)
        {
            return new ResponseDto<HospitalDetailsDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "Hospital no configurado.",
                Data = null
            };
        }

        return new ResponseDto<HospitalDetailsDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Detalles del hospital obtenidos correctamente.",
            Data = hospital.ToDto()
        };
    }


    // ============================================================
    //                 CREATE (solo se usa 1 vez)
    // ============================================================
    public async Task<ResponseDto<HospitalDetailsDto>> CreateAsync(CreateHospitalPropertiesDto dto)
    {
        // Verificar si ya existe un singleton
        var existing = await _context.HospitalProperties
            .FirstOrDefaultAsync(x => x.IsSingleton);

        if (existing != null)
        {
            return new ResponseDto<HospitalDetailsDto>
            {
                Status = false,
                Message = "Ya existe una configuración del hospital.",
                StatusCode = 400,
                Data = null
            };
        }

        var entity = new HospitalPropertiesEntity
        {
            Name = dto.Name,
            Director = dto.Director,
            Subdirector = dto.Subdirector,
            Ubication = dto.Ubication,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            HospitalCode = dto.HospitalCode,
            RTN = dto.RTN,
            Website = dto.Website,
            Currency = dto.Currency,
            IsSingleton = true
        };

        _context.HospitalProperties.Add(entity);
        await _context.SaveChangesAsync();

        return new ResponseDto<HospitalDetailsDto>
        {
            Status = true,
            StatusCode = 201,
            Message = "Hospital creado correctamente.",
            Data = entity.ToDto()
        };
    }


    // ============================================================
    //                 UPDATE
    // ============================================================
    public async Task<ResponseDto<HospitalDetailsDto>> UpdateAsync(UpdateHospitalPropertiesDto dto)
    {
        var hospital = await _context.HospitalProperties
            .FirstOrDefaultAsync(x => x.IsSingleton);

        if (hospital == null)
        {
            return new ResponseDto<HospitalDetailsDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "No existe una configuración del hospital.",
                Data = null
            };
        }

        hospital.ApplyUpdate(dto);

        await _context.SaveChangesAsync();

        return new ResponseDto<HospitalDetailsDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Hospital actualizado correctamente.",
            Data = hospital.ToDto()
        };
    }
}