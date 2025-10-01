import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { usePutApiLocationsId, getGetApiLocationsQueryKey } from "../../../api/locations/locations";
import { type UpdateLocationDto, type LocationDto, type AddressDto, type ContactPointDto, LocationStatus, LocationMode } from "../../../api/models";
import { useParams } from "react-router-dom";

export default function useEditLocationForm(initial: LocationDto) {
     const { id } = useParams<{ id: string }>();  
  
  const queryClient = useQueryClient();
  const { mutateAsync: updateLocation } = usePutApiLocationsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiLocationsQueryKey() });
      },
    },
  });

  const [formData, setFormData] = useState<UpdateLocationDto>({
    name: initial.name ?? "",
    description: initial.description ?? null,
    status: LocationStatus.NUMBER_0,
    mode: LocationMode.NUMBER_0,
    address: initial.address ?? { line: [], city: null, state: null, postalCode: null, country: null },
    telecom: initial.telecom ?? [],
    type: initial.type ?? null,
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const setField = (field: keyof UpdateLocationDto | string, value: any) => {
    setFormData((prev) => {
      if (field in prev) {
        return { ...prev, [field]: value };
      }
      if (field.startsWith("address.")) {
        const subField = field.split(".")[1] as keyof AddressDto;
        return {
          ...prev,
          address: { ...prev.address, [subField]: subField === "line" ? [value] : value || null },
        };
      }
      if (field === "phone" || field === "email") {
        const system = field === "phone" ? "phone" : "email";
        const existingTelecom = (prev.telecom || [])?.filter((t) => t.system !== system) ?? [];
        const newTelecom: ContactPointDto = { system, value, use: "work" };
        return { ...prev, telecom: value ? [...existingTelecom, newTelecom] : existingTelecom };
      }
      return prev;
    });
  };

  const handleSubmit = async (): Promise<boolean> => {
    setIsSubmitting(true);
    setError(null);

    try {
      await updateLocation({ id: Number(id), data: formData });
      setIsSubmitting(false);
      return true;
    } catch (err: any) {
      setError(err?.message ?? "Error al actualizar la ubicación");
      setIsSubmitting(false);
      return false;
    }
  };

  return { formData, setField, handleSubmit, isSubmitting, error };
}
