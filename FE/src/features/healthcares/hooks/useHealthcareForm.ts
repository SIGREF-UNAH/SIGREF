import { useNavigate } from "react-router";
import { mockLocations, mockOrganizations } from "../store";

export function useHealthcareForm() {
  const navigate = useNavigate();

  // Obtener las organizaciones (Despues sera desde la API)
  const organizations = mockOrganizations;

  // Obtener las ubicaciones (Despues sera desde la API)
  const locations = mockLocations;

  const handleCancel = () => {
    navigate("/healthcares");
  };

  return {
    organizations,
    locations,
    handleCancel,
  };
}
