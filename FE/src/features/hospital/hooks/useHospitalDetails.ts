import { useNavigate } from "react-router";
import { useGetApiHospitalPropertiesDetails } from "../../../api/hospital-properties/hospital-properties";

export default function useHospitalDetails() {
  const navigate = useNavigate();

  const { data: response, isLoading, isError, error } = useGetApiHospitalPropertiesDetails();

  // Extraer los datos del objeto data
  const hospital = response?.data;

  return {
    hospital,
    isLoading,
    isError,
    error,
    navigate,
  };
}
