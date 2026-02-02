import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router";
import { useMessage } from "../../../shared/hooks";
import { getGetApiSeriesQueryKey, usePostApiSeries } from "../../../api/series/series";
import type { CreateSeriesDto } from "../../../api/models";


export function useCreateSerie() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Crear serie
  const { mutateAsync: createSerie, isPending } = usePostApiSeries({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiSeriesQueryKey() });
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