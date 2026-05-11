import { useState } from "react";
import { useNavigate } from "react-router";
import { useGetHealtcareList } from "../../../api/healthcares/healthcares";
import { useGetLocationList } from "../../../api/locations/locations";

export function useServiceGroupForm() {
  const navigate = useNavigate();

  // Estados para paginación de servicios de salud
  const [healthcarePageNumber, setHealthcarePageNumber] = useState(1);
  const [healthcarePageSize, setHealthcarePageSize] = useState(10);
  const [healthcareSearch, setHealthcareSearch] = useState("");
  const [healthcareScope, setHealthcareScope] = useState<string | undefined>(undefined);

  // Estados para paginación de ubicaciones
  const [locationPageNumber, setLocationPageNumber] = useState(1);
  const [locationPageSize, setLocationPageSize] = useState(10);
  const [locationSearch, setLocationSearch] = useState("");

  // Construir parámetros para healthcares
  const healthcareParams: any = {
    PageNumber: healthcarePageNumber,
    PageSize: healthcarePageSize,
  };

  if (healthcareSearch) {
    healthcareParams.Name = healthcareSearch;
  }

  if (healthcareScope) {
    healthcareParams.Scope = healthcareScope;
  }

  // Cargar servicios de salud con paginación
  const {
    data: healthcaresResponse,
    isLoading: isLoadingHealthcares,
    isFetching: isFetchingHealthcares,
  } = useGetHealtcareList(healthcareParams);

  // Cargar ubicaciones con paginación
  const {
    data: locationsResponse,
    isLoading: isLoadingLocations,
    isFetching: isFetchingLocations,
  } = useGetLocationList({
    PageNumber: locationPageNumber,
    PageSize: locationPageSize,
    Name: locationSearch || undefined,
  });

  const handleCancel = () => {
    navigate("/service-groups/list");
  };

  return {
    // Datos de servicios de salud
    healthcares: healthcaresResponse?.items || [],
    healthcarePagination: healthcaresResponse?.pagination,
    isLoadingHealthcares,
    isFetchingHealthcares,
    healthcarePageNumber,
    healthcarePageSize,
    healthcareScope,
    setHealthcarePageNumber,
    setHealthcarePageSize,
    setHealthcareSearch,
    setHealthcareScope,

    // Datos de ubicaciones
    locations: locationsResponse?.items || [],
    locationPagination: locationsResponse?.pagination,
    isLoadingLocations,
    isFetchingLocations,
    locationPageNumber,
    locationPageSize,
    setLocationPageNumber,
    setLocationPageSize,
    setLocationSearch,

    // Funciones comunes
    isLoading: isLoadingHealthcares || isLoadingLocations,
    handleCancel,
  };
}