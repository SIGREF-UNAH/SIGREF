import { useState } from "react";
import { useGetApiOrganizations } from "../../../api/organizations/organizations";

export default function useOrganizationList() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState("");

  const { data, isLoading, error, refetch } = useGetApiOrganizations({
    PageNumber: page,
    PageSize: pageSize,
    SearchTerm: search || undefined,
  });

  const handlePageChange = (newPage: number, newPageSize: number) => {
    setPage(newPage);
    setPageSize(newPageSize);
  };

  const handleSearch = (searchTerm: string) => {
    setSearch(searchTerm);
    setPage(1);
  };

  return {
    organizations: data?.items || [],
    total: data?.totalCount || 0,
    page,
    pageSize,
    isLoading,
    error,
    handlePageChange,
    handleSearch,
    refetch,
  };
}