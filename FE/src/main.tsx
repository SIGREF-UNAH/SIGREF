import { ReactKeycloakProvider } from "@react-keycloak/web";
import { createRoot } from "react-dom/client";
import App from "./App.tsx";
import "./index.css";
import { keycloak } from "./auth/keycloak.ts";
import { cleanupClientSession } from "./auth/sessionCleanup";

createRoot(document.getElementById("root")!).render(
  <ReactKeycloakProvider
    authClient={keycloak}
    onTokens={() => localStorage.setItem("kc_token", keycloak.token || "")}
    onEvent={(event) => {
      if (event === "onAuthLogout" || event === "onAuthRefreshError") {
        cleanupClientSession();
      }
    }}
  >
    <App />
  </ReactKeycloakProvider>
);
