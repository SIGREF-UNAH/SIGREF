import { useState } from "react";
import type { LocationFormData } from "../types";
import { emptyLocationFormData as defaultData } from "../types";

type SetField = (field: keyof LocationFormData, value: string) => void;

export default function useLocationForm(initial?: LocationFormData) {
  const [formData, setFormData] = useState<LocationFormData>(
    initial ?? defaultData
  );
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const setField: SetField = (field, value) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const reset = () => setFormData(defaultData);

  const handleSubmit = async (e?: React.FormEvent) => {
    if (e && typeof e.preventDefault === "function") e.preventDefault();
    setIsSubmitting(true);
    setError(null);
    try {
      // Aqui ira la llamada a la API
      console.log("Submitting location:", formData);
      // simular latencia
      await new Promise((r) => setTimeout(r, 300));
      setIsSubmitting(false);
      return { ok: true };
    } catch (err: any) {
      setError(err?.message ?? "Error desconocido");
      setIsSubmitting(false);
      return { ok: false, error };
    }
  };

  return { formData, setField, reset, handleSubmit, isSubmitting, error };
}
