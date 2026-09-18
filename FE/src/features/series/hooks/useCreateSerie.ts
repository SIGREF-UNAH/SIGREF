import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router";
import { useMessage } from "../../../shared/hooks";
import {
  getGetSerieListQueryKey,
  useCreateSerie as useCreateSerieMutation,
} from "@endpoints/series/series";
import type { CreateSeriesDto } from "@models";

/**
 * Hook personalizado para crear una serie
 */
export function useCreateSerie() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Crear serie
  const { mutateAsync: createSerie, isPending } = useCreateSerieMutation({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetSerieListQueryKey() });
        msg.success("Serie creada correctamente");
        navigate("/series");
      },
      onError: (error: any) => {
        const errorMsg =
          error?.response?.data?.title ||
          error?.response?.data?.detail ||
          "Error al crear la serie";
        msg.error(errorMsg);
      },
    },
  });

  const handleFinish = async (values: CreateSeriesDto) => {
    try {
      await createSerie({ data: values });
    } catch (error) {
      // El error ya se manejó en onError
    }
  };

  return {
    handleFinish,
    isPending,
  };
}