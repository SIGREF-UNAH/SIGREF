import { ACTION_COLOR_MAP, HTTP_METHOD_COLOR_MAP, STATUS_COLOR_MAP } from "../constants";

export const useColorUtils = () => {
  const getActionColor = (action: string): string =>
    ACTION_COLOR_MAP[action?.toLowerCase()] ?? "default";

  const getHttpMethodColor = (method: string): string =>
    HTTP_METHOD_COLOR_MAP[method?.toUpperCase()] ?? "default";

  const getStatusColor = (statusCode?: number): string => {
    if (!statusCode) return "default";
    return STATUS_COLOR_MAP[statusCode] ?? (statusCode >= 500 ? "error" : statusCode >= 400 ? "warning" : "success");
  };

  return { getActionColor, getHttpMethodColor, getStatusColor };
};