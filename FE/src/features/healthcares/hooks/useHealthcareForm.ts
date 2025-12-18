import { useNavigate } from "react-router";
import { useGetApiOrganizations } from "../../../api/organizations/organizations";
import { useGetApiLocations } from "../../../api/locations/locations";

export function useHealthcareForm() {
  const navigate = useNavigate();

  // Obtener las organizaciones desde la API
  const { data: organizationsResponse, isLoading: isLoadingOrganizations } = useGetApiOrganizations();

  // Obtener las ubicaciones desde la API
  const { data: locationsData, isLoading: isLoadingLocations } = useGetApiLocations();

  // Extraer los items de las respuestas
  const organizations = organizationsResponse?.items || [];
  const locations = locationsData?.items || [];

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