import "@ant-design/v5-patch-for-react-19";
import { ReactKeycloakProvider } from "@react-keycloak/web";
import { createRoot } from "react-dom/client";
import App from "./App.tsx";
import "./index.css";
import { keycloak } from "./auth/keycloak.ts";

createRoot(document.getElementById("root")!).render(
  <ReactKeycloakProvider
    authClient={keycloak}
    onTokens={() => localStorage.setItem("kc_token", keycloak.token || "")}
  >
    <App />
  </ReactKeycloakProvider>
);
