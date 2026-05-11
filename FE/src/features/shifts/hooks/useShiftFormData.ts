import { useGetLocationList } from "../../../api/locations/locations";

// ! no hay paginacion real en locations

export function useShiftFormData() {
  const { data: locationsResponse, isLoading } = useGetLocationList({
    PageNumber: 1,
    PageSize: 9999,
  });

  const locations = locationsResponse?.items || [];

  return {
    locations,
    isLoading,
  };
}
