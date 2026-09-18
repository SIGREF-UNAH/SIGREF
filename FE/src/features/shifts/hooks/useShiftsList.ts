import { useState, useMemo } from "react";
import { useNavigate } from "react-router";
import { useUrlFilters } from "../../../shared/hooks";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import { createTablePagination } from "../../../shared/components/ui";
import type { ShiftDto } from "@models";
import {
  getGetShiftListQueryKey,
  useDeleteShiftById,
  useGetShiftList,
} from "@endpoints/shifts/shifts";

export function useShiftsList() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  const [searchInput, setSearchInput] = useState("");

  const { filters, setFilter, setFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      location: undefined as string | undefined,
      status: undefined as string | undefined,
      pageNumber: 1,
      pageSize: 10,
    },
  });

  const queryParams = useMemo(() => {
    const params: any = {
      PageNumber: filters.pageNumber,
      PageSize: filters.pageSize,
    };

    if (filters.search) {
      params.Name = filters.search.trim();
    }

    if (filters.location) {
      params.LocationId = filters.location;
    }

    if (filters.status === "active") {
      params.IsActive = true;
    } else if (filters.status === "inactive") {
      params.IsActive = false;
    }

    return params;
  }, [filters]);

  const {
    data: response,
    isLoading,
    isFetching,
    isError,
  } = useGetShiftList(queryParams, {
    query: {
      placeholderData: (previousData) =>
        previousData ?? {
          data: {
            items: [],
            pagination: { totalItems: 0, currentPage: 1, pageSize: 10 },
          },
        },
      staleTime: 1000 * 30, // 30 segundos
      retry: 1,
    },
  });

  const shifts = useMemo<ShiftDto[]>(() => {
    return response?.data?.items ?? [];
  }, [response]);

  const pagination = response?.data?.pagination;

  const { mutate: deleteShift } = useDeleteShiftById({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetShiftListQueryKey() });
        msg.success("Turno desactivado correctamente");
      },
      onError: () => msg.error("Error al desactivar el turno"),
    },
  });

  const handleEdit = (id: string) => {
    if (id) {
      navigate(`/shifts/update/${id}`);
    }
  };

  const handleDelete = (id: string) => {
    if (id) {
      deleteShift({ id });
    }
  };

  const handleSearchInputChange = (value: string) => {
    setSearchInput(value);
  };

  const handleSearch = () => {
    const trimmed = searchInput.trim();
    setFilter("search", trimmed || undefined);
  };

  const handleClearSearch = () => {
    setSearchInput("");
    setFilter("search", undefined);
  };

  const paginationConfig = createTablePagination({
    current: pagination?.currentPage || 1,
    pageSize: pagination?.pageSize || 10,
    total: pagination?.totalItems || 0,
    pageSizeOptions: ["10", "20", "50", "100"],
    onChange: (page, pageSize) => {
      setFilters({
        pageNumber: page,
        pageSize: pageSize || 10,
      });
    },
  });

  return {
    filters,
    shifts,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    searchInput,
    handleEdit,
    handleDelete,
    setFilter,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
  };
}
