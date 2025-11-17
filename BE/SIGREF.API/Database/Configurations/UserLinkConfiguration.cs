using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity;

namespace SIGREF.API.Database.Configurations;

    public class UserLinkConfiguration : IEntityTypeConfiguration<UserLink>
    {
        public void Configure(EntityTypeBuilder<UserLink> builder)
        {
            // ================================================================
            // Indices para busqueda rapida dentro de la tabla user_links
            // ================================================================

            //  Index unico: nos permite encontrar un usuario local por su ID en Keycloak.
            //    - Se usa cuando llega un JWT y queremos mapear el claim "sub" → UserLink
            //    - Garantiza que no existan dos filas con el mismo usuario de Keycloak
            builder.HasIndex(u => u.KeycloakUserId).IsUnique();

            //  Index para encontrar rapidamente un UserLink a partir del Practitioner
            //    - Sirve cuando en SIGREF seleccionas un empleado (practitioner)
            //      y necesitas saber si tiene usuario del sistema.
            builder.HasIndex(u => u.PractitionerId);

            //  Index combinado: Username + Email
            //    - Busquedas administrativas: “buscar por correo o por usuario”
            //    - Util cuando sincronizas datos desde Keycloak
            builder.HasIndex(u => new { u.Username, u.Email });

            //  Index para busquedas por DisplayName (nombre del Practitioner)
            //    - Consultas generales desde el panel administrativo:
            //        > buscar por nombre o apellido
            //        > autocompletado
            //        > listados rápidos
            builder.HasIndex(u => u.DisplayName);

            // Index para filtrar usuarios activos/inactivos
            //    - Súper útil para paginación:
            //        > usuarios activos
            //        > usuarios deshabilitados
            //    - Permite filtros instantaneos sin escanear la tabla completa
            builder.HasIndex(u => u.Active);

        }
    }