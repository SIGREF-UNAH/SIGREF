export const HTTP_METHODS = ["GET", "POST", "PUT", "PATCH", "DELETE"] as const;

export const ACTIONS = ["read", "create", "update", "delete"] as const;

export const PAGE_SIZE_OPTIONS = ["10", "20", "50", "100"];

export const DEFAULT_FILTERS = {
  CurrentPage: 1,
  PageSize: 10,
};

export const ACTION_COLOR_MAP: Record<string, string> = {
  read: "blue",
  create: "green",
  update: "orange",
  delete: "red",
};

export const HTTP_METHOD_COLOR_MAP: Record<string, string> = {
  GET: "blue",
  POST: "green",
  PUT: "orange",
  PATCH: "gold",
  DELETE: "red",
};

export const STATUS_COLOR_MAP: Record<number, string> = {
  200: "success",
  201: "success",
  204: "success",
  400: "warning",
  401: "error",
  403: "error",
  404: "warning",
  500: "error",
};