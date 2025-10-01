import { BrowserRouter } from "react-router";
import { AppRouter } from "./routers";
import { AntdConfig } from "./config";
import { useKeycloak } from "@react-keycloak/web";
import { AbilityProvider } from "./context/AbilityContext";
import { getRolesFromToken } from "./utils/keycloakRoles";

function App() {

  const { initialized, keycloak } = useKeycloak();
  if (!initialized) {
    return <div>Cargando...</div>;
  }

  if (!keycloak.authenticated) {
    keycloak.login();
    return <div>Redirigiendo a la página de inicio de sesión...</div>;
  }
  const roles = getRolesFromToken(keycloak);

  return (
    <AntdConfig>
      <BrowserRouter>
        <AbilityProvider roles={roles}>
          <AppRouter />
        </AbilityProvider>
      </BrowserRouter>
    </AntdConfig>
  );
}

export default App;
