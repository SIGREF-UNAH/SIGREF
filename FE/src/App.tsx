import { BrowserRouter } from "react-router";
import { AppRouter } from "./routers";
import { useKeycloak } from "@react-keycloak/web";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Spin } from "antd";
import { getRolesFromToken } from "./auth";
import { AbilityProvider, AntDesignProvider, ShortcutsProvider } from "./config";
import { CashierSessionChecker } from "./features/cashier-sessions/components";

const queryClient = new QueryClient();

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
