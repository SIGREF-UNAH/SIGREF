import {
  getGetPatientListQueryKey,
  useDeletePatientById,
  useGetPatientList,
  useGetPatientById,
} from '@endpoints/patients/patients'
import type { TablePaginationConfig } from 'antd'
import { message } from 'antd'
import { createTablePagination } from '../../../shared/components/ui'
import type { AddressDto2, GetPatientListParams, PatientDto } from '@models'
import { useMemo, useState } from 'react'
import { useMessage, useUrlFilters } from '../../../shared/hooks'
import { PatientExtensionsUrls } from '../../../shared/constants'
import { useQueryClient } from '@tanstack/react-query'

export type PaginationDto = {
  currentPage: number
  pageSize: number
  totalItems: number
}

type PatientsResponse = {
  items?: PatientDto[]
  pagination?: PaginationDto
}

const defaultFilters = {
  search: '',
  pageNumber: 1,
  pageSize: 10,
  nombreCompleto: null as string | null,
  genero: null as string | null,
  estadoVital: null as string | null,
  tipoIdentificador: null as string | null,
  identificador: null as string | null,
  fechaNacimiento: null as string | null,
}

export function usePatientsInformation() {
  const queryClient = useQueryClient()
  const msg = useMessage()
  const [, contextHolder] = message.useMessage()
  // Estado local para el ID del paciente seleccionado
  const [selectedPatientId, setSelectedPatientId] = useState<string>('')

  // Detalle del paciente
  const {
    data,
    isLoading: loadingPatientDetail,
    error,
  } = useGetPatientById(selectedPatientId, {
    query: {
      enabled: !!selectedPatientId, // Solo hacer la petición si hay un ID
    },
  })

  // Filtros y paginación
  const { filters, setFilters, setFilter } = useUrlFilters({
    defaultValues: defaultFilters,
  })

  // Construcción de queryParams con filtros que el backend soporta
  const queryParams = useMemo(() => {
    const params: GetPatientListParams = {
      PageNumber: filters.pageNumber,
      PageSize: filters.pageSize,
    }

    // Nombre
    if (filters.nombreCompleto && filters.nombreCompleto.trim()) {
      params.Name = filters.nombreCompleto.trim()
    }

    // Género
    if (filters.genero) {
      params.Gender =
        filters.genero === 'Masculino'
          ? 'male'
          : filters.genero === 'Femenino'
            ? 'female'
            : filters.genero === 'Otro'
              ? 'other'
              : filters.genero === 'Desconocido'
                ? 'unknown'
                : undefined
    }

    // Estado vital
    if (filters.estadoVital) {
      params.Active =
        filters.estadoVital === 'Vivo'
          ? true
          : filters.estadoVital === 'Fallecido'
            ? false
            : undefined
    }

    // Tipo de identificador
    if (filters.tipoIdentificador) {
      params.IdentifierType = filters.tipoIdentificador
    }

    // Identificador
    if (filters.identificador && filters.identificador.trim()) {
      params.IdentifierValue = filters.identificador.trim()
    }

    // Fecha de nacimiento
    if (filters.fechaNacimiento) {
      let dateString = filters.fechaNacimiento

      // Si es string en formato DD/MM/YYYY, convertir a YYYY-MM-DD
      if (typeof dateString === 'string' && /^\d{2}\/\d{2}\/\d{4}$/.test(dateString)) {
        const [day, month, year] = dateString.split('/')
        dateString = `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`
      }
      // Si ya está en formato YYYY-MM-DD, usar directamente
      else if (typeof dateString === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(dateString)) {
        // No hacer nada, ya está en el formato correcto
      }

      params.BirthDate = dateString
    }

    if (filters.search && filters.search.trim()) {
      params.Name = filters.search.trim()
    }

    return params
  }, [filters])

  // Lista de pacientes con paginación
  const {
    data: response,
    isLoading: loadingPatients,
    refetch,
  } = useGetPatientList<PatientsResponse>(queryParams, {
    query: {
      placeholderData: (prev: PatientsResponse | undefined) => prev,
    },
  })

  const patient = data

  function formatAddress(a: AddressDto2) {
    return [a.line?.join(', '), a.city, a.state, a.country].filter(Boolean).join(', ')
  }

  // Datos del paciente seleccionado
  const selectedPatient = useMemo(() => {
    return {
      id: patient?.id ?? '',
      nombre: patient?.name?.[0]?.given?.join(' ') ?? 'Desconocido',
      apellidos: patient?.name?.[0]?.family ?? 'Desconocido',
      tipo: patient?.name?.[0]?.use,
      fechaNacimiento: patient?.birthDate
        ? new Date(patient.birthDate).toLocaleDateString('es-HN', {
            day: '2-digit',
            month: 'long',
            year: 'numeric',
          })
        : 'No especificada',
      edad: patient?.birthDate
        ? `${Math.floor(
            (Date.now() - new Date(patient.birthDate).getTime()) / (365.25 * 24 * 60 * 60 * 1000),
          )} años`
        : 'No especificada',
      genero:
        patient?.gender === 'male'
          ? 'Masculino'
          : patient?.gender === 'female'
            ? 'Femenino'
            : patient?.gender === 'other'
              ? 'Otro'
              : 'Desconocido',
      estadoCivil:
        typeof patient?.maritalStatus?.text === 'string'
          ? patient?.maritalStatus?.text === 'U'
            ? 'Soltero/a'
            : patient?.maritalStatus?.text === 'M'
              ? 'Casado/a'
              : patient?.maritalStatus?.text === 'D'
                ? 'Divorciado/a'
                : patient?.maritalStatus?.text === 'W'
                  ? 'Viudo/a'
                  : patient?.maritalStatus?.text === 'T'
                    ? 'Unión de hechos'
                    : patient?.maritalStatus?.text === 'UNK'
                      ? 'Desconocido'
                      : patient?.maritalStatus?.text
          : patient?.maritalStatus?.coding?.[0]?.display || 'Desconocido',
      nacionalidad:
        patient?.extension?.find((ext) => ext.url === PatientExtensionsUrls.nationality)
          ?.valueString || 'Desconocido',
      estadoVital: patient?.active ? 'Vivo' : 'Fallecido',
      idMaestro: patient?.id || 'Desconocido',
      identificadores:
        patient?.identifier?.map((id) => ({
          tipo: id.type?.coding?.[0]?.display || id.type?.text || 'Desconocido',
          codigo: id.type?.coding?.[0]?.code || id.type?.text || 'Desconocido',
          valor: id.value || 'Desconocido',
          emisor: id.system || null,
        })) || [],
      contactos:
        patient?.telecom?.map((t) => ({
          tipo:
            typeof t.system === 'string'
              ? t.system === 'phone'
                ? 'Teléfono'
                : t.system === 'email'
                  ? 'Correo electrónico'
                  : t.system === 'url'
                    ? 'Dirección web'
                    : t.system === 'pager'
                      ? 'Pager'
                      : t.system === 'fax'
                        ? 'Fax'
                        : t.system === 'sms'
                          ? 'Mensaje de texto'
                          : t.system === 'other'
                            ? 'Otro'
                            : t.system
              : t.system || 'Desconocido',
          uso:
            typeof t.use === 'string'
              ? t.use === 'mobile'
                ? 'Personal'
                : t.use === 'home'
                  ? 'Hogar'
                  : t.use === 'work'
                    ? 'Trabajo'
                    : t.use === 'temp'
                      ? 'Temporal'
                      : t.use === 'old'
                        ? 'Antiguo'
                        : t.use
              : t.use || 'Desconocido',
          valor: t.value,
        })) || [],
      direcciones:
        patient?.address?.map((a) => ({
          tipo:
            typeof a.use === 'string'
              ? a.use === 'home'
                ? 'Hogar'
                : a.use === 'work'
                  ? 'Trabajo'
                  : a.use === 'temp'
                    ? 'Temporal'
                    : a.use === 'old'
                      ? 'Antiguo'
                      : a.use
              : a.use || 'Desconocido',
          valor: formatAddress(a),
        })) || [],
    }
  }, [patient])

  // Mapeo de pacientes para tabla
  const patients = useMemo(() => {
    const items = response?.items ?? []
    return items.map((p, index) => ({
      id: p.id || String(index),
      key: p.id || String(index),
      nombre:
        `${p?.name?.[0]?.given?.join(' ') || ''} ${p?.name?.[0]?.family || ''}`.trim() ||
        'Desconocido',
      identificadorTipo: (() => {
        const code = p.identifier?.[0]?.type?.coding?.[0]?.code?.toUpperCase() ?? ''
        if (code === 'DNI') return 'DNI'
        if (code === 'PPN') return 'PPN'
        if (code === 'NI') return 'ID'
        return 'DSC'
      })(),
      identificador: p.identifier?.[0]?.value || 'Desconocido',
      contacto: p.telecom?.[0]?.value || '-',
      nacimiento: p.birthDate ? new Date(p.birthDate).toLocaleDateString() : '-',
      nacionalidad:
        p?.extension?.find((ext) => ext.url === PatientExtensionsUrls.nationality)?.valueString ||
        '-',
      genero:
        p.gender === 'male'
          ? 'Masculino'
          : p.gender === 'female'
            ? 'Femenino'
            : p.gender === 'other'
              ? 'Otro'
              : 'Desconocido',
      estadoVital: p.active ? 'Vivo' : 'Fallecido',
    }))
  }, [response])

  // Configuración de paginación
  const paginationConfig: TablePaginationConfig = createTablePagination({
    current: response?.pagination?.currentPage || filters.pageNumber || 1,
    pageSize: response?.pagination?.pageSize || filters.pageSize || 10,
    total: response?.pagination?.totalItems || 0,
    pageSizeOptions: ['10', '20', '50'],
    onChange: (page, pageSize) => {
      setFilters({ pageNumber: page, pageSize })
    },
  })

  // Eliminar paciente
  const { mutate: deletePatient } = useDeletePatientById({
    mutation: {
      onSuccess: () => {
        void queryClient.invalidateQueries({
          queryKey: getGetPatientListQueryKey(),
        })
        msg.success('Paciente eliminado correctamente')
        setSelectedPatientId('')
        void refetch()
      },
      onError: () => msg.error('Error al eliminar el paciente'),
    },
  })

  // Copiar datos del paciente
  const handleCopyData = () => {
    if (!selectedPatient || !selectedPatient.id) {
      msg.warning('No hay datos del paciente para copiar.')
      return
    }

    const info = `
ID: ${selectedPatient.id}
Nombre: ${selectedPatient.nombre} 
Apellido:${selectedPatient.apellidos}
Fecha de Nacimiento: ${selectedPatient.fechaNacimiento}
Edad: ${selectedPatient.edad}
Género: ${selectedPatient.genero}
Nacionalidad: ${selectedPatient.nacionalidad}
Estado Civil: ${selectedPatient.estadoCivil}
Estado Vital: ${selectedPatient.estadoVital}
${selectedPatient.identificadores.map((id) => `${id.tipo}: ${id.valor} (${id.emisor})`).join('\n')}
${selectedPatient.contactos.map((c) => `${c.tipo}(${c.uso}): ${c.valor}`).join('\n')}
${selectedPatient.direcciones.map((d) => `Dirección(${d.tipo}): ${d.valor}`).join('\n')}
`.trim()

    void navigator.clipboard.writeText(info)
    msg.success('Datos del paciente copiados al portapapeles.')
  }

  // Función para seleccionar un paciente
  const handleSelectPatient = (patientId: string) => {
    setSelectedPatientId(patientId)
  }

  // Función para limpiar todos los filtros
  const clearAllFilters = () => {
    setFilters({
      ...defaultFilters,
      pageNumber: 1,
      pageSize: filters.pageSize, // Mantener el tamaño de página actual
    })
  }

  // Color para tipo de identificador
  const getIdentificadorColor = (tipo: string) => {
    switch (tipo) {
      case 'DNI':
        return 'blue'
      case 'PPN':
        return 'purple'
      case 'ID':
        return 'red'
      default:
        return 'default'
    }
  }

  return {
    selectedPatientId,
    contextHolder,
    data,
    error,
    selectedPatient,
    patients,
    paginationConfig,
    filters,
    loadingPatients,
    isLoading: loadingPatientDetail || loadingPatients,
    setFilter,
    setFilters,
    clearAllFilters,
    handleCopyData,
    handleSelectPatient,
    deletePatient,
    getIdentificadorColor,
  }
}
