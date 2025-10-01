import { BrowserRouter } from "react-router";
import { AppRouter } from "./routers";
import { AntdConfig } from "./config";
import { Navbar } from "./shared/components/layout/navbar/Navbar";
import { Footer } from "./shared/components/layout/footer/Footer";
import { useKeycloak } from "@react-keycloak/web";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

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


  return (
    <QueryClientProvider client={queryClient}>
    <AntdConfig>
      <BrowserRouter>
        <div className="min-h-screen flex flex-col">
          <Navbar />
          <main className="flex-1">
            <AppRouter />
          </main>
          <Footer />
        </div>
      </BrowserRouter>
    </AntdConfig>
    </QueryClientProvider>
  );
}

export default App;
