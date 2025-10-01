import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { usePostApiLocations, getGetApiLocationsQueryKey } from "../../../api/locations/locations";
import { LocationMode, type CreateLocationDto, LocationStatus, type ContactPointDto, type AddressDto } from "../../../api/models/";

export default function useLocationForm(initial?: Partial<CreateLocationDto>) {
  const queryClient = useQueryClient();
  const { mutateAsync: createLocation } = usePostApiLocations({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiLocationsQueryKey() });
      },
    },
  });

  const [formData, setFormData] = useState<CreateLocationDto>({
    name: "",
    alias: [],
    description: null,
    status: LocationStatus.NUMBER_0,
    mode: LocationMode.NUMBER_0,
    address: { line: [], city: null, state: null, postalCode: null, country: null },
    telecom: [], // Inicializamos como array vacío para evitar null/undefined
    type: null,
    partOfId: null,
    managingOrganizationIds: null,
    ...initial,
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const setField = (field: keyof CreateLocationDto | string, value: any) => {
    setFormData((prev) => {
      // Manejo de campos simples
      if (field in prev) {
        return { ...prev, [field]: value };
      }
      // Manejo de subcampos como address.line, address.city, etc.
      if (field.startsWith("address.")) {
        const subField = field.split(".")[1] as keyof AddressDto;
        return {
          ...prev,
          address: { ...prev.address, [subField]: subField === "line" ? [value] : value || null },
        };
      }
      // Manejo de telecom (phone -> {system: "phone"}, email -> {system: "email"})
      if (field === "phone" || field === "email") {
        const system = field === "phone" ? "phone" : "email";
        const existingTelecom = (prev.telecom || []).filter((t) => t.system !== system); // Verificación de null/undefined
        const newTelecom: ContactPointDto = { system, value, use: "work" };
        return { ...prev, telecom: value ? [...existingTelecom, newTelecom] : existingTelecom };
      }
      // Manejo de alias como array
      if (field === "alias") {
        return { ...prev, alias: value ? [value] : [] };
      }
      return prev;
    });
  };

  const reset = () => {
    setFormData({
      name: "",
      alias: [],
      description: null,
      status: LocationStatus.NUMBER_0,
      mode: LocationMode.NUMBER_0,
      address: { line: [], city: null, state: null, postalCode: null, country: null },
      telecom: [],
      type: null,
      partOfId: null,
      managingOrganizationIds: null,
    });
    setError(null);
  };

  const handleSubmit = async (): Promise<boolean> => {
    setIsSubmitting(true);
    setError(null);

    try {
      if (!formData.name) {
        throw new Error("El nombre de la ubicación es obligatorio");
      }
      if (!formData.address?.line?.[0]) {
        throw new Error("La dirección es obligatoria");
      }

      await createLocation({ data: formData });
      reset();
      setIsSubmitting(false);
      return true;
    } catch (err: any) {
      setError(err?.message ?? "Error al crear la ubicación");
      setIsSubmitting(false);
      return false;
    }
  };

  return { formData, setField, reset, handleSubmit, isSubmitting, error };
}