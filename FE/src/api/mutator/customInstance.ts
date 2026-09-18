import axios, {
  type AxiosError,
  type AxiosRequestConfig,
  type AxiosResponse,
} from 'axios'

const API_URL = import.meta.env.VITE_API_URL

export const AXIOS_INSTANCE = axios.create({
  baseURL: API_URL,
})

AXIOS_INSTANCE.interceptors.request.use((config) => {
  const token = localStorage.getItem('kc_token')

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

export const customInstance = async <T>(
  config: AxiosRequestConfig,
  options?: AxiosRequestConfig,
): Promise<T> => {
  const response: AxiosResponse<T> =
    await AXIOS_INSTANCE.request<T>({
      ...config,
      ...options,
    })

  return response.data
}

export type ErrorType<Error> = AxiosError<Error>

export type BodyType<BodyData> = BodyData