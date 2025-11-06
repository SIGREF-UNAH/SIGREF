import {
  usePostApiPatients,
  getGetApiPatientsQueryKey,
} from "../../../api/patients/patients";
import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import type { CreatePatientDto } from "../../../api/models";
import { useNavigate } from "react-router";
import { PatientExtensionsUrls } from "../../../shared/constants";

export default function useCreatePatientForm() {
  const navigate = useNavigate();
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
          use: values.tipoNombre,
          family: values.apellidos,
          given: [values.primerNombre, values.segundoNombre || ""].filter(
            Boolean
          ),
          prefix: [],
          suffix: [],
        },
      ],
      gender: values.gender,
      birthDate: values.fechanacimiento,
      active: values.estadoVital === 1,
      maritalStatus: {
        coding: [
          {
            system: "http://terminology.hl7.org/CodeSystem/v3-MaritalStatus",
            code: values.estadoCivilCodigo || "UNK",
            display: values.estadoCivil || "Desconocido",
          },
        ],
        text: values.estadoCivil || "Desconocido",
      },
      extension: [
        {
          url: PatientExtensionsUrls.nationality,
          valueString: values.nacionalidad || "Desconocido",
        },
      ],

      telecom:
        values.telecom?.map((item: any, index: number) => ({
          system: item.system,
          use: item.use,
          value: item.value || "",
          rank: index + 1,
        })) || [],

      address:
        values.address?.map((addr: any, index: number) => ({
          use: addr.tipoDireccion || "casa",
          type: addr.type || 0,
          text: addr.line?.[0] || "",
          line: addr.line || [],
          city: addr.city || "",
          district: addr.district || "",
          state: addr.state || "",
          postalCode: addr.postalCode || "",
          country: addr.country || "",
          rank: index + 1,
        })) || [],

      identifier: [
        {
          use: 0,
          type: {
            coding: [
              {
                system: "http://terminology.hl7.org/CodeSystem/v2-0203",
                code: values.tipoIdentificacion,
                display:
                  values.tipoIdentificacion === "PPN"
                    ? "Número de Pasaporte"
                    : values.tipoIdentificacion === "NI"
                      ? "Documento de Identificación"
                      : "Documento Nacional de Identidad",
                userSelected: true,
              },
            ],
            text: values.tipoIdentificacion,
          },
          system: values.emisor || null,
          value: values.identifier?.[0]?.value || null,
        },
      ],
    };

    setIsSubmitting(true);
    setError(null);

    try {
      await createPatient({ data: payload });
      setIsSubmitting(false);
      navigate("/patients/list");
      return true;
    } catch (err: any) {
      setError(err.message || "Error al crear paciente");
      setIsSubmitting(false);
      return false;
    }
  };

  return { handleSubmit, isSubmitting, error };
}
