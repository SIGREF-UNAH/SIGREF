import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useNavigate } from "react-router";
import { useMessage } from "../../../shared/hooks";
import type { UpdateHospitalPropertiesDto } from "../../../api/models";
import {
  getGetApiHospitalPropertiesDetailsQueryKey,
  useGetApiHospitalPropertiesDetails,
  usePutApiHospitalProperties,
} from "../../../api/hospital-properties/hospital-properties";

export default function useUpdateHospital() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  const {
    data: response,
    isLoading,
    isError,
  } = useGetApiHospitalPropertiesDetails();

  // Extraer los datos del objeto data
  const hospital = response?.data;

  const updateMutation = usePutApiHospitalProperties({
    mutation: {
      onSuccess: () => {
        msg.success("Información del hospital actualizada exitosamente");
        // Invalidar la query de detalles para que se recargue
        queryClient.invalidateQueries({
          queryKey: getGetApiHospitalPropertiesDetailsQueryKey(),
        });
        navigate("/hospital/details");
      },
      onError: (error) => {
        console.error("Error al actualizar hospital:", error);
        msg.error("Error al actualizar la información del hospital");
      },
    },
  });

  useEffect(() => {
    // Si hay error o no hay datos, redirigir a detalles
    if (isError || (!isLoading && !hospital)) {
      msg.warning("No hay información para actualizar");
      navigate("/hospital/details");
    }
  }, [isError, isLoading, hospital, navigate]);

  const handleUpdate = (values: UpdateHospitalPropertiesDto) => {
    updateMutation.mutate({ data: values });
  };

  const handleCancel = () => {
    navigate("/hospital/details");
  };

  return {
    hospital,
    updateMutation,
    isLoading,
    isError,
    navigate,
    handleUpdate,
    handleCancel,
  };
}
