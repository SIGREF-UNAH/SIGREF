import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  usePostApiLocations,
  getGetApiLocationsQueryKey,
} from "../../../api/locations/locations";
import {
  LocationMode,
  type CreateLocationDto,
  LocationStatus,
} from "../../../api/models/";

export default function useLocationForm() {
  const queryClient = useQueryClient();
  const { mutateAsync: createLocation } = usePostApiLocations({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiLocationsQueryKey(),
        });
      },
    },
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (values: CreateLocationDto): Promise<boolean> => {
    setIsSubmitting(true);
    setError(null);

    try {
      if (!values.name) {
        throw new Error("El nombre de la ubicación es obligatorio");
      }
      if (!values.address?.line?.length || !values.address.line[0]) {
        throw new Error("La dirección es obligatoria");
      }

      await createLocation({ data: values });
      setIsSubmitting(false);
      return true;
    } catch (err: any) {
      setError(err?.message ?? "Error al crear la ubicación");
      setIsSubmitting(false);
      return false;
    }
  };

  return { handleSubmit, isSubmitting, error };
}
