import { useState } from "react";
import { useNavigate } from "react-router";
import { useGetApiHealthcares } from "../../../api/healthcares/healthcares";
import { useGetApiLocations } from "../../../api/locations/locations";

export function useServiceGroupForm() {
  const navigate = useNavigate();

  // Estados para paginación de servicios de salud
  const [healthcarePageNumber, setHealthcarePageNumber] = useState(1);
  const [healthcarePageSize, setHealthcarePageSize] = useState(10);
  const [healthcareSearch, setHealthcareSearch] = useState("");

  // Estados para paginación de ubicaciones
  const [locationPageNumber, setLocationPageNumber] = useState(1);
  const [locationPageSize, setLocationPageSize] = useState(10);
  const [locationSearch, setLocationSearch] = useState("");

  // Cargar servicios de salud con paginación
  const {
    data: healthcaresResponse,
    isLoading: isLoadingHealthcares,
    isFetching: isFetchingHealthcares,
  } = useGetApiHealthcares({
    PageNumber: healthcarePageNumber,
    PageSize: healthcarePageSize,
    Name: healthcareSearch || undefined,
  });

  // Cargar ubicaciones con paginación
  const {
    data: locationsResponse,
    isLoading: isLoadingLocations,
    isFetching: isFetchingLocations,
  } = useGetApiLocations({
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
    setHealthcarePageNumber,
    setHealthcarePageSize,
    setHealthcareSearch,

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
