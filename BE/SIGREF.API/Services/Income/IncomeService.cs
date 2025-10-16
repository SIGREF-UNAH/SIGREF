using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Income;
using SIGREF.API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIGREF.API.Services.Income
{
    public class IncomeService : IIncomeService
    {
        private readonly FhirClient _fhirClient;
        private const string ResourceType = "ChargeItem";

        public IncomeService(FhirClient fhirClient)
        {
            _fhirClient = fhirClient ?? throw new ArgumentNullException(nameof(fhirClient));
        }

        public async Task<IEnumerable<IncomeDto>> GetAllIncomesAsync()
        {
            var searchParams = new SearchParams()
                .Add("code", "income")
                .OrderBy("_lastUpdated", SortOrder.Descending);

            var bundle = await _fhirClient.SearchAsync<ChargeItem>(searchParams);

            return bundle.Entry
                .Select(entry => (entry.Resource as ChargeItem)?.ToIncomeDto())
                .Where(dto => dto != null)
                .ToList()!;
        }

        public async Task<IncomeDto?> GetIncomeByIdAsync(string id)
        {
            try
            {
                var income = await _fhirClient.ReadAsync<ChargeItem>($"{ResourceType}/{id}");
                return income?.ToIncomeDto();
            }
            catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IncomeDto> CreateIncomeAsync(CreateIncomeDto createIncomeDto)
        {
            var income = createIncomeDto.ToFhirChargeItem();
            var result = await _fhirClient.CreateAsync(income);
            return result.ToIncomeDto();
        }

        public async Task<IncomeDto> CreateCounterpartAsync(CreateCounterpartIncomeDto createCounterpartDto)
        {
            // Verificar que el ingreso original existe
            var originalIncome = await GetIncomeByIdAsync(createCounterpartDto.OriginalIncomeId);
            if (originalIncome == null)
            {
                throw new ArgumentException($"El ingreso original con ID {createCounterpartDto.OriginalIncomeId} no existe.");
            }

            // Verificar que el ingreso original no sea ya una contrapartida
            if (originalIncome.IsCounterpart)
            {
                throw new InvalidOperationException("No se puede crear una contrapartida de otra contrapartida.");
            }

            // Crear la contrapartida
            var counterpart = createCounterpartDto.ToFhirChargeItem();
            var result = await _fhirClient.CreateAsync(counterpart);
            return result.ToIncomeDto();
        }
    }
}