
// ! Cuando se quite el wrapper se eliminara el .data

import { useNavigate } from "react-router";
import { useGetHospitalPropertiesDetails } from "../../../api/hospital-properties/hospital-properties";

export default function useHospitalDetails() {
  const navigate = useNavigate();

  const { data: response, isLoading, isError, error } = useGetHospitalPropertiesDetails();

  // Extraer los datos del objeto data
  const hospital = response?.data?? null;

  return {
    hospital,
    isLoading,
    isError,
    error,
    navigate,
  };
}
