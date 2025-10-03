import { BrowserRouter } from "react-router";
import { AppRouter } from "./routers";
import { AntdConfig } from "./config";
import { useKeycloak } from "@react-keycloak/web";

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
    <AntdConfig>
      <BrowserRouter>
        <AppRouter />
      </BrowserRouter>
    </AntdConfig>
  );
}

export default App;
