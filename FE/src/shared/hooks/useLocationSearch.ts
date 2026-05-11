import { useState, useCallback } from "react";
import { getLocationList } from "../../api/locations/locations";

interface LocationOption {
  label: string;
  value: string;
}

export function useLocationSearch() {
  const [options, setOptions] = useState<LocationOption[]>([]);
  const [loading, setLoading] = useState(false);

  const searchLocations = useCallback(async (searchText: string) => {
    setLoading(true);
    try {
      const response = await getLocationList({
        Name: searchText || undefined,
        PageNumber: 1,
        PageSize: searchText ? 20 : 10,
      });

      const locationOptions = (response?.items || []).map((loc: any) => ({
        label: loc.name || "",
        value: loc.id || "",
      }));

      setOptions(locationOptions);
    } catch (error) {
      console.error("Error searching locations:", error);
      setOptions([]);
    } finally {
      setLoading(false);
    }
  }, []);

  const clearOptions = useCallback(() => {
    setOptions([]);
  }, []);

  return {
    options,
    loading,
    searchLocations,
    clearOptions,
  };
}
