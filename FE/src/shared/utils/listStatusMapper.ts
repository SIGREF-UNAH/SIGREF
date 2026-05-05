/**
 * Mapper para el enum ListStatus de FHIR
 * Basado en: http://hl7.org/fhir/list-status
 */

/**
 * Enum de estados de lista según FHIR
 */
export const ListStatus = {
  /** La lista se considera una parte activa del registro del paciente */
  Current: 'Current',
  /** La lista es "antigua" y ya no debe considerarse precisa o relevante */
  Retired: 'Retired',
  /** La lista nunca fue precisa. Se conserva solo con fines médico-legales */
  EnteredInError: 'EnteredInError',
} as const;

export type ListStatus = typeof ListStatus[keyof typeof ListStatus];

/**
 * Traducciones de ListStatus al español
 */
const LIST_STATUS_TRANSLATIONS: Record<ListStatus, string> = {
  [ListStatus.Current]: 'Actual',
  [ListStatus.Retired]: 'Retirado',
  [ListStatus.EnteredInError]: 'Ingresado con Error',
};

/**
 * Convierte un ListStatus a su traducción en español
 * @param status - El estado de la lista
 * @returns La traducción en español del estado
 */
export const getListStatusLabel = (status: string | ListStatus | null | undefined): string => {
  if (!status) return '';

  const statusKey = status as ListStatus;
  return LIST_STATUS_TRANSLATIONS[statusKey] || status;
};

/**
 * Obtiene las opciones para un select de estado de lista
 * @returns Array de opciones con label en español y value en inglés
 */
export const getListStatusOptions = () => [
  { label: LIST_STATUS_TRANSLATIONS[ListStatus.Current], value: ListStatus.Current },
  { label: LIST_STATUS_TRANSLATIONS[ListStatus.Retired], value: ListStatus.Retired },
  { label: LIST_STATUS_TRANSLATIONS[ListStatus.EnteredInError], value: ListStatus.EnteredInError },
];

/**
 * Obtiene el color de badge/tag según el estado
 * @param status - El estado de la lista
 * @returns Color para el badge/tag de Ant Design
 */
export const getListStatusColor = (status: string | ListStatus | null | undefined): string => {
  if (!status) return 'default';

  const statusColors: Record<ListStatus, string> = {
    [ListStatus.Current]: 'success',
    [ListStatus.Retired]: 'warning',
    [ListStatus.EnteredInError]: 'error',
  };

  return statusColors[status as ListStatus] || 'default';
};
