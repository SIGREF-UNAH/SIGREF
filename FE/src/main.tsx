import type { KeycloakInstance } from 'keycloak-js';

export class ApiService {
  private baseURL: string;
  private keycloak: KeycloakInstance;

  constructor(baseURL: string, keycloak: KeycloakInstance) {
    this.baseURL = baseURL;
    this.keycloak = keycloak;
  }

  private async getHeaders(): Promise<HeadersInit> {
    // Asegurar que el token esté actualizado
    if (this.keycloak.token) {
      try {
        await this.keycloak.updateToken(30); // Renovar si expira en 30 segundos
      } catch (error) {
        console.error('Error renovando token:', error);
        this.keycloak.login();
      }
    }

    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${this.keycloak.token}`,
    };
  }

  async get<T>(endpoint: string): Promise<T> {
    const response = await fetch(`${this.baseURL}${endpoint}`, {
      method: 'GET',
      headers: await this.getHeaders(),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response.json();
  }

  async post<T>(endpoint: string, data: any): Promise<T> {
    const response = await fetch(`${this.baseURL}${endpoint}`, {
      method: 'POST',
      headers: await this.getHeaders(),
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response.json();
  }

  async put<T>(endpoint: string, data: any): Promise<T> {
    const response = await fetch(`${this.baseURL}${endpoint}`, {
      method: 'PUT',
      headers: await this.getHeaders(),
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response.json();
  }

  async delete<T>(endpoint: string): Promise<T> {
    const response = await fetch(`${this.baseURL}${endpoint}`, {
      method: 'DELETE',
      headers: await this.getHeaders(),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response.json();
  }
}

// Hook personalizado para usar el servicio
import { useKeycloak } from "@react-keycloak/web";
import { useMemo } from "react";

export const useApiService = () => {
  const { keycloak } = useKeycloak();
  
  return useMemo(() => {
    return new ApiService('http://localhost:5000/api', keycloak); // Ajusta la URL de tu API
  }, [keycloak]);
};
