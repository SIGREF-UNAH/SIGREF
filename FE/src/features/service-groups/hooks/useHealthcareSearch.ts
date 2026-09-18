import { useState, useCallback } from "react";
import { getHealtcareList } from "@endpoints/healthcare-services/healthcare-services";

interface HealthcareOption {
  label: string;
  value: string;
}

export function useHealthcareSearch() {
  const [options, setOptions] = useState<HealthcareOption[]>([]);
  const [loading, setLoading] = useState(false);

  const searchHealthcares = useCallback(async (searchText: string) => {
    setLoading(true);
    try {
      const response = await getHealtcareList({
        Name: searchText || undefined,
        PageNumber: 1,
        PageSize: searchText ? 20 : 10,
      });

      const healthcareOptions = (response?.items || []).map((item: any) => ({
        label: item.name || "",
        value: item.id || "",
      }));

      setOptions(healthcareOptions);
    } catch (error) {
      console.error("Error searching healthcares:", error);
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
    searchHealthcares,
    clearOptions,
  };
}
