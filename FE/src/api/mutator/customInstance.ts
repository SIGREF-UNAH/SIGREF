import axios from "axios";

const API_URL = import.meta.env.VITE_API_URL;

export const AXIOS_INSTANCE = axios.create({
  baseURL: API_URL,
});

AXIOS_INSTANCE.interceptors.request.use((config) => {
  const token = localStorage.getItem("kc_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const customInstance = async <T = any>(config: any): Promise<T> => {
  const response = await AXIOS_INSTANCE.request<T>(config);
  return response.data;
};