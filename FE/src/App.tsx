import { BrowserRouter } from "react-router";
import { AppRouter } from "./routers";
import { useKeycloak } from "@react-keycloak/web";
import { AbilityProvider } from "./context/AbilityContext";
import { getRolesFromToken } from "./auth/roles";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AntdConfig } from "./config/components";

const queryClient = new QueryClient();

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
    <QueryClientProvider client={queryClient}>
    <AntdConfig>
      <BrowserRouter>
        <AbilityProvider roles={roles}>
          <div className="min-h-screen flex flex-col">
          <main className="flex-1">
            <AppRouter />
          </main>
          </div>
        </AbilityProvider>
      </BrowserRouter>
    </AntdConfig>
    </QueryClientProvider>
  );
}

export default App;
