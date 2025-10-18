import { AbilityBuilder, Ability } from '@casl/ability'

export type Actions = 'manage' | 'read' | 'create' | 'update' | 'delete'
export type Subjects = 'Fondos' | 'Servicios' | 'Pacientes' | 'Empleados' | 'Eventos' | 'Organizaciones' | 'Ubicaciones' | 'Reportes' | 'all'

export const defineAbilitiesFor = (roles: string[]) => {
  const { can, cannot, build } = new AbilityBuilder(Ability)

  // Admin total
  if (roles.includes('admin')) {
    can('manage', 'all')
  }

  // Cajero / asistente de fondos
  if (roles.includes('cashier')) {
    can(['create', 'read', 'update'], 'Fondos')
    can(['create', 'read', 'update'], 'Servicios')
    can(['create', 'read', 'update'], 'Pacientes')
  }

  // Auditor
  if (roles.includes('auditor')) {
    can('read', 'Servicios')
    can('read', 'Empleados')
    can('read', 'Eventos')
    can('read', 'Fondos')
  }

  // TI
  if (roles.includes('ti')) {
    can(['create', 'read', 'update'], 'Empleados')
    can(['create', 'read', 'update'], 'Eventos')
    can(['create', 'read', 'update'], 'Organizaciones')
    can(['create', 'read', 'update'], 'Ubicaciones')
  }

  // Restricciones generales
  cannot('delete', 'Empleados')
  cannot('delete', 'User')  

  return build()
}
