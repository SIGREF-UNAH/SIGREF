import { useQuery, useMutation } from "@tanstack/react-query";
import { type LocationDto } from "../../../api/models"; 

export default function useEditLocationForm(id: string) {
  const { data: formData, isLoading, error } = useQuery<LocationDto>({
    queryKey: ["location", id],
    queryFn: async () => {
      const res = await fetch(`/api/locations/${id}`);
      if (!res.ok) throw new Error("Error cargando la ubicación");
      return res.json();
    },
  });

  // Mutation para actualizar
  const mutation = useMutation({
    mutationFn: async (updated: LocationDto) => {
      const res = await fetch(`/api/locations/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(updated),
      });
      if (!res.ok) throw new Error("Error al actualizar ubicación");
      return res.json();
    },
  });

  const setField = (field: string, value: any) => {
    if (!formData) return;
    formData[field as keyof LocationDto] = value;
  };

  const handleSubmit = async () => {
    if (!formData) return false;
    await mutation.mutateAsync(formData);
    return true;
  };

  return {
    formData,
    setField,
    handleSubmit,
    isSubmitting: mutation.isPending,
    error: error ? (error as Error).message : null,
    isLoading,
  };
}
