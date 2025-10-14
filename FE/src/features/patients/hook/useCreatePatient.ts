import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { usePostApiPatients, getGetApiPatientsQueryKey } from "../../../api/patients/patients";
import type { CreatePatientDto } from "../../../api/models";

export default function useCreatePatientForm() {
  const queryClient = useQueryClient();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { mutateAsync: createPatient } = usePostApiPatients({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiPatientsQueryKey() });
      },
    },
  });

  const handleSubmit = async (values: any): Promise<boolean> => {
  const payload: CreatePatientDto = {
    gender: values.gender === "0" ? "male" : "female",
    birthDate: values.fechanacimiento,
    name: [
      {
        use: "official",  
        given: [values.primerNombre, values.segundoNombre].filter(Boolean),
        family: values.apellidos,
        text: `${values.primerNombre} ${values.segundoNombre ?? ""} ${values.apellidos}`.trim(),
      },
    ],
    telecom: [],  
    address: [],  
    identifier: [],  
  };

  console.log("Datos enviados al backend:", payload);

  setIsSubmitting(true);
  setError(null);

  try {
    await createPatient({ data: payload });
    setIsSubmitting(false);
    return true;
  } catch (err: any) {
    setError(err.message || "Error al crear paciente");
    setIsSubmitting(false);
    return false;
  }
};

  return { handleSubmit, isSubmitting, error };
}
