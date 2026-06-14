import { useState } from "react";
import { Form } from "antd";
import type { Dayjs } from "dayjs";
import type { EventHistoryFormValues, EventHistoryFilters } from "../types";
import { DEFAULT_FILTERS } from "../constants";

export const useEventHistoryFilters = () => {
  const [form] = Form.useForm();
  const [formValues, setFormValues] = useState<EventHistoryFormValues>({} as EventHistoryFormValues);
  const [filters, setFilters] = useState<EventHistoryFilters>(DEFAULT_FILTERS);

  const handleSearch = () => {
    const { dateRange, ...rest } = formValues;
    setFilters({
      ...DEFAULT_FILTERS,
      ...rest,
      startDate: dateRange?.[0]?.toISOString(),
      endDate: dateRange?.[1]?.toISOString(),
    });
  };

  const handleClearFilters = () => {
    form.resetFields();
    setFormValues({} as EventHistoryFormValues);
    setFilters(DEFAULT_FILTERS);
  };

  const handlePageChange = (page: number, pageSize: number) => {
    setFilters((prev) => ({ ...prev, CurrentPage: page, PageSize: pageSize }));
  };

  return {
    form,
    formValues,
    filters,
    setFormValues,
    setFilters,
    handleSearch,
    handleClearFilters,
    handlePageChange,
  };
};