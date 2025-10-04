import { Can } from '@casl/react'
import { useAbility } from '../../../context/AbilityContext'
import { useKeycloak } from '@react-keycloak/web';


export const Example = () => {
  const ability = useAbility()

  const { keycloak } = useKeycloak();

  if (keycloak?.authenticated) {
    console.log("Token parseado:", keycloak.tokenParsed);
  }

  return (
    <div>
      <Can I="create" a="Event" ability={ability}>
        <button>Crear evento</button>
      </Can>

      <Can I="delete" a="User" ability={ability}>
        <button>Eliminar usuario</button>
      </Can>
    </div>
  )
}
