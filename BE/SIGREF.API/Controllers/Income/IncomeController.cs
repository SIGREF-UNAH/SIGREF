using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Income;
using SIGREF.API.Services.Income;
using System.Threading.Tasks;

namespace SIGREF.API.Controllers.IncomeC
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeController : ControllerBase
    {
        private readonly IIncomeService _incomeService;

        public IncomeController(IIncomeService incomeService)
        {
            _incomeService = incomeService;
        }

        /// <summary>
        /// Obtiene todos los ingresos (cashier y admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = RolesConstants.cashier + "," + RolesConstants.admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            var incomes = await _incomeService.GetAllIncomesAsync();
            return Ok(incomes);
        }

        /// <summary>
        /// Obtiene un ingreso por ID (cashier y admin)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = RolesConstants.cashier + "," + RolesConstants.admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> GetById(string id)
        {
            var income = await _incomeService.GetIncomeByIdAsync(id);
            if (income == null)
                return NotFound($"Ingreso con id '{id}' no encontrado.");

            return Ok(income);
        }

        /// <summary>
        /// Registra un nuevo ingreso (cashier y admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = RolesConstants.cashier + "," + RolesConstants.admin)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> Create([FromBody] CreateIncomeDto createIncomeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var income = await _incomeService.CreateIncomeAsync(createIncomeDto);
            return CreatedAtAction(nameof(GetById), new { id = income.Id }, income);
        }

        /// <summary>
        /// Invalida un ingreso creando una contrapartida (solo admin)
        /// </summary>
        [HttpPost("counterpart")]
        [Authorize(Roles = RolesConstants.admin)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> CreateCounterpart([FromBody] CreateCounterpartIncomeDto createCounterpartDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var income = await _incomeService.CreateCounterpartAsync(createCounterpartDto);
                return CreatedAtAction(nameof(GetById), new { id = income.Id }, income);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}