import { useNavigate } from "react-router";
import { useGetApiOrganizations } from "../../../api/organizations/organizations";
import { useGetApiLocations } from "../../../api/locations/locations";

export function useHealthcareForm() {
  const navigate = useNavigate();

  // Obtener las organizaciones desde la API
  const { data: organizationsResponse, isLoading: isLoadingOrganizations } = useGetApiOrganizations({
    PageNumber: 1,
    PageSize: 9999, // Obtener 9999 organizaciones
  });

  // Obtener las ubicaciones desde la API
  const { data: locationsResponse, isLoading: isLoadingLocations } = useGetApiLocations({
    PageNumber: 1,
    PageSize: 9999, // Obtener 9999 ubicaciones
  });

  // Extraer los items de las respuestas
  const organizations = organizationsResponse?.items || [];
  const locations = locationsResponse?.items || [];

  const handleCancel = () => {
    navigate("/healthcares");
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