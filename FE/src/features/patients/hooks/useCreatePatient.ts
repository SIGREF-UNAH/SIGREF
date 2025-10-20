import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  usePostApiPatients,
  getGetApiPatientsQueryKey,
} from "../../../api/patients/patients";
import type { CreatePatientDto } from "../../../api/models";

export default function useCreatePatientForm() {
  const queryClient = useQueryClient();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { mutateAsync: createPatient } = usePostApiPatients({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiPatientsQueryKey(),
        });
      },
    },
  });

  const handleSubmit = async (values: any): Promise<boolean> => {
    const payload: CreatePatientDto = {
      name: [
        {
          use: values.tipoNombre === "legal" ? 0 : 1,
          text: `${values.primerNombre} ${values.apellidos}`,
          family: values.apellidos,
          given: [values.primerNombre, values.segundoNombre || ""].filter(
            Boolean
          ),
          prefix: [],
          suffix: [],
        },
      ],
      gender: Number(values.gender),
      birthDate: values.fechanacimiento,
      active: values.estadoVital === 1,
      telecom: values.telecom?.map((item: any, index: number) => ({
        system: item.system,
        use: item.use,
        value: item.value || "",
        rank: index + 1,
      })),
      address:
        values.address?.map((addr: any, index: number) => ({
          use: 0,
          type: 0,
          text: addr.line?.[0] || "",
          line: addr.line || [],
          city: addr.city || "",
          district: addr.district || "",
          state: addr.state || "",
          postalCode: addr.postalCode || "",
          country: addr.country || "",
        })) || [],
      identifier: [
        {
          use: 0,
          type: {
            coding: [
              {
                system: "http://terminology.hl7.org/CodeSystem/v2-0203",
                version: "2.9",
                code: "ID",
                display: values.tipoIdentificacion,
                userSelected: true,
              },
            ],
            text: values.tipoIdentificacion,
          },
          system: "https://example.com/identifiers",
          value: values.identifier?.[0]?.value || "",
        },
      ],
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
