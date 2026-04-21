import { BrowserRouter } from "react-router";
import { AppRouter } from "./routers";
import { useKeycloak } from "@react-keycloak/web";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Spin } from "antd";
import { getRolesFromToken } from "./auth";
import { AbilityProvider, AntDesignProvider, ShortcutsProvider } from "./config";
import { CashierSessionChecker } from "./features/cashier-sessions/components";

/*

* Trabajo sobre la ISSUE #401
* En esta configuración, el QueryClient se ha personalizado para que no intente reintentar las consultas que devuelven un error 404. Esto es útil porque si el backend indica que un recurso no existe, no tiene sentido seguir intentando obtenerlo. Para otros tipos de errores, se mantendrá el comportamiento predeterminado de reintentar hasta 3 veces.
*/
 
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: (failureCount, error: any) => {
        // Si el backend dice que el recurso no existe, no tiene sentido insistir
        if (error?.response?.status === 404 || error?.status === 404) {
          return false;
        }
        // Para lo demás, el default de 3 reintentos está bien
        return failureCount < 3;
      },
    },
  },
});

export default function App() {
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
        <Spin size="large" />
      </div>
    );
  }

  const roles = getRolesFromToken(keycloak);

  return (
    <QueryClientProvider client={queryClient}>
      <AntDesignProvider>
        <BrowserRouter>
          <ShortcutsProvider>
            <AbilityProvider roles={roles}>
              <CashierSessionChecker />
              <div className="min-h-screen flex flex-col">
                <main className="flex-1">
                  <AppRouter />
                </main>
              </div>
            </AbilityProvider>
          </ShortcutsProvider>
        </BrowserRouter>
      </AntDesignProvider>
    </QueryClientProvider>
  );
}
