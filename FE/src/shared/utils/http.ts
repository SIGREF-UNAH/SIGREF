import axios from "axios";
import { keycloak } from "../../config";

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "/",
});

http.interceptors.request.use(async (config) => {
  if (keycloak?.authenticated) {
    try {
      await keycloak.updateToken(30);
    } catch (err) {
      // Si no se pudo refrescar el token, forzar login
      keycloak.login();
    }

    const token = keycloak?.token;
    if (token) {
      config.headers = config.headers ?? {};
      (config.headers as Record<string, string>)["Authorization"] = `Bearer ${token}`;
    }
  }
  return config;
});

http.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error?.response?.status === 401) {
      // Intentar refrescar y reintentar una vez
      try {
        await keycloak.updateToken(0);
        const token = keycloak?.token;
        if (token) {
          error.config.headers = error.config.headers ?? {};
          error.config.headers["Authorization"] = `Bearer ${token}`;
        }
        return http.request(error.config);
      } catch (_) {
        keycloak.login();
      }
    }
    return Promise.reject(error);
  }
);
