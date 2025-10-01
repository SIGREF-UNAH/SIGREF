import { AbilityBuilder, Ability } from '@casl/ability'

export type Actions = 'manage' | 'read' | 'create' | 'update' | 'delete'
export type Subjects = 'User' | 'Event' | 'Category' | 'all'

export const defineAbilitiesFor = (roles: string[]) => {
  const { can, cannot, build } = new AbilityBuilder(Ability)

  if (roles.includes('admin')) {
    can('manage', 'all')
  }

  if (roles.includes('editor')) {
    can('create', 'Event')
    can('update', 'Event')
    can('read', 'Event')
  }

  if (roles.includes('user')) {
    can('read', 'Event')
  }

  cannot('delete', 'User')

  return build()
}
