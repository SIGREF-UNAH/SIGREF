import { useGetApiLocations } from "../../../api/locations/locations";

export function useShiftFormData() {
  const { data: locationsResponse, isLoading } = useGetApiLocations({
    PageNumber: 1,
    PageSize: 9999,
  });

  const locations = locationsResponse?.items || [];

  return {
    locations,
    isLoading,
  };
}
