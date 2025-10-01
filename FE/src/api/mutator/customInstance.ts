import axios from "axios";

const API_URL = import.meta.env.VITE_API_URL;

export const AXIOS_INSTANCE = axios.create({
  baseURL: API_URL,
});

export const customInstance = async <T = any>(config: any): Promise<T> => {
  const response = await AXIOS_INSTANCE.request<T>(config);
  return response.data;
};