using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Cashier;

    public interface ICashierSessionService
    {
        // ======================================
        //     APERTURA DE SESIÓN DE CAJA
        // ======================================
        
        /// <summary>
        /// Abre una nueva sesión de caja para el usuario.
        /// </summary>
        Task<ResponseDto<CashierSessionMinimalDto>> OpenSessionAsync(CreateCashierSessionDto dto);


        /// <summary>
        /// Obtiene la sesión activa del usuario (si existe).
        /// </summary>
        Task<ResponseDto<CashierSessionDto?>> GetActiveSessionByUserAsync(Guid userId);


        // ======================================
        //           CIERRE DE SESIÓN
        // ======================================

        /// <summary>
        /// Cierra una sesión de caja y calcula automáticamente diferencia y estado.
        /// </summary>
        Task<ResponseDto<CashierSessionDto>> CloseSessionAsync(Guid sessionId, CloseCashierSessionDto dto);


        // ======================================
        //     SOLICITUD Y RESOLUCIÓN DE CORRECCIÓN
        // ======================================

        /// <summary>
        /// Cajero solicita corrección (solo notas).
        /// </summary>
        Task<ResponseDto<CashierSessionDto>> RequestCorrectionAsync(Guid sessionId, RequestCorrectionDto dto);


        /// <summary>
        /// Admin resuelve la corrección.
        /// </summary>
        Task<ResponseDto<CashierSessionDto>> ResolveCorrectionAsync(Guid sessionId, ResolveCorrectionDto dto);


        // ======================================
        //              OBTENER SESIONES
        // ======================================

        /// <summary>
        /// Lista sesiones de caja con filtros y paginación.
        /// Admin ve todas, cajero solo las suyas.
        /// </summary>
        Task<ResponseDto<PagedResultDto<CashierSessionDto>>> GetFilteredSessionsAsync(
            CashierSessionFilterDto filter
        );


        /// <summary>
        /// Obtiene una sesión específica por ID.
        /// </summary>
        Task<ResponseDto<CashierSessionDto?>> GetByIdAsync(Guid sessionId);
    }