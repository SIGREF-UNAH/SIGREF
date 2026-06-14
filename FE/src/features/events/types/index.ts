import type { Dayjs } from "dayjs";

export interface EventHistoryFormValues {
  userName?: string;
  userId?: string;
  action?: string;
  httpMethod?: string;
  dateRange: [Dayjs, Dayjs] | null;
  resourceType?: string;
  traceId?: string;
  ipAddress?: string;
  success?: boolean;
}

export interface EventHistoryFilters {
  CurrentPage: number;
  PageSize: number;
  userName?: string;
  userId?: string;
  action?: string;
  httpMethod?: string;
  startDate?: string;
  endDate?: string;
  resourceType?: string;
  traceId?: string;
  ipAddress?: string;
  success?: boolean;
}