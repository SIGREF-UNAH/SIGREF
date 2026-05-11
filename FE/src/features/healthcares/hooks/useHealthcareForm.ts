import { useNavigate } from "react-router-dom";
import { useGetOrganizationList } from "../../../api/organizations/organizations";
import { useGetLocationList } from "../../../api/locations/locations";

export function useHealthcareForm() {
  const navigate = useNavigate();

  // Obtener las organizaciones desde la API
  const {
    data: organizationsResponse,
    isLoading: isLoadingOrganizations,
  } = useGetOrganizationList();

  // Obtener las ubicaciones desde la API
  const {
    data: locationsResponse,
    isLoading: isLoadingLocations,
  } = useGetLocationList();

  // Extraer los items de las respuestas (asumiendo estructura paginada)
  const organizations = organizationsResponse?.items || [];
  const locations = locationsResponse?.items || [];

  const handleCancel = () => {
    navigate("/healthcares/list");
  };

  return {
    organizations,
    locations,
    isLoadingOrganizations,
    isLoadingLocations,
    isLoading: isLoadingOrganizations || isLoadingLocations,
    handleCancel,
  };
}