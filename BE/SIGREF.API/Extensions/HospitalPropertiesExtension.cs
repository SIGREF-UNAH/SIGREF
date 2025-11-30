using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Dtos.Administration;

namespace SIGREF.API.Extensions;

public static class HospitalPropertiesExtension
{
    public static HospitalAdminDto ToDto(this HospitalPropertiesEntity h)
    {
        return new HospitalAdminDto
        {
            Name = h.Name,
            Director = h.Director,
            Subdirector = h.Subdirector,
            Ubication = h.Ubication,
            PhoneNumber = h.PhoneNumber,
            Email = h.Email,
            HospitalCode = h.HospitalCode,
            RTN = h.RTN,
            Website = h.Website,
            Currency = h.Currency,
           // ExchangeVersion = h.ExchangeVersion,
            UrlLogo = h.UrlLogo,
            UrlLogoHealth = h.UrlLogoHealth
        };
    }

    public static void ApplyUpdate(this HospitalPropertiesEntity h, UpdateHospitalPropertiesDto dto)
    {
        if (dto.Name != null) h.Name = dto.Name;
        if (dto.Director != null) h.Director = dto.Director;
        if (dto.Subdirector != null) h.Subdirector = dto.Subdirector;
        if (dto.Ubication != null) h.Ubication = dto.Ubication;
        if (dto.PhoneNumber != null) h.PhoneNumber = dto.PhoneNumber;
        if (dto.Email != null) h.Email = dto.Email;
        if (dto.HospitalCode != null) h.HospitalCode = dto.HospitalCode;
        if (dto.RTN != null) h.RTN = dto.RTN;
        if (dto.Website != null) h.Website = dto.Website;
        if (dto.Currency != null) h.Currency = dto.Currency;
       // if (dto.ExchangeVersion != null) h.ExchangeVersion = dto.ExchangeVersion;
    }

}