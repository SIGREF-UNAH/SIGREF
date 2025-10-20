import { BrowserRouter } from "react-router";
import { AppRouter } from "./routers";
import { useKeycloak } from "@react-keycloak/web";
import { getRolesFromToken } from "./auth/roles";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AbilityProvider, AntdProvider, ShortcutsProvider } from "./config/providers";
import { Spin } from "antd";

const queryClient = new QueryClient();

function App() {
  const { initialized, keycloak } = useKeycloak();
  if (!initialized) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Spin size="large" />
      </div>
    );
  }

  if (!keycloak.authenticated) {
    keycloak.login();
    return (
      <div className="flex items-center justify-center h-screen">
        Redirigiendo a la página de inicio de sesión...
      </div>
    );
  }
  const roles = getRolesFromToken(keycloak);

  return (
    <QueryClientProvider client={queryClient}>
      <AntdProvider>
        <BrowserRouter>
          <ShortcutsProvider>
            <AbilityProvider roles={roles}>
              <div className="min-h-screen flex flex-col">
                <main className="flex-1">
                  <AppRouter />
                </main>
              </div>
            </AbilityProvider>
          </ShortcutsProvider>
        </BrowserRouter>
      </AntdProvider>
    </QueryClientProvider>
  );
}

export default App;
