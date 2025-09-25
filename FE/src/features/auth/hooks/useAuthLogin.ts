import { message } from "antd";
import { useState } from "react";

interface LoginValues {
  usuario: string;
  password: string;
}

export const useAuthLogin = () => {
  const [loading, setLoading] = useState(false);

  const login = async (values: LoginValues) => {
    try {
      setLoading(true);

      const url = `${import.meta.env.VITE_KEYCLOAK_URL}realms/${import.meta.env.VITE_KEYCLOAK_REALM}/protocol/openid-connect/token`;

console.log("URL:", url);
console.log("Parametros:", {
  client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
  grant_type: "password",
  username: values.usuario,
  password: values.password,
});

      const response = await fetch(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
        },
        body: new URLSearchParams({
          client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID || "",
          grant_type: "password",
          username: values.usuario,
          password: values.password,
        }),
      });

      if (!response.ok) {
        throw new Error("Credenciales incorrectas");
      }

      const data = await response.json();

      console.log("Token recibido:", data);

      localStorage.setItem("access_token", data.access_token);
      localStorage.setItem("refresh_token", data.refresh_token);

      message.success("Login exitoso");

      return data;
    } catch (error) {
      console.error("Error en login:", error);
      message.error("Credenciales incorrectas");
      throw error;
    } finally {
      setLoading(false);
    }
  };

  return { login, loading };
};
