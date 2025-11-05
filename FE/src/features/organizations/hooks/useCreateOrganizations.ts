// TODO: tipo se va vacio, endpoint da 500, Ubicacion hace falta en el hook 


import { useNavigate } from "react-router";
import type { CreateOrganizationDto } from "../../../api/models";
import { useQueryClient } from "@tanstack/react-query";
import {
  getGetApiOrganizationsQueryKey,
  usePostApiOrganizations,
} from "../../../api/organizations/organizations";
import { useMessage } from "../../../shared/hooks";

export function useCreateOrganization() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  const { mutateAsync: createOrganization, isPending } =
    usePostApiOrganizations({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetApiOrganizationsQueryKey(),
          });
          msg.success("Organización creada correctamente");
          navigate("/organizations");
        },
        onError: (error: any) => {
          console.error("Error al crear la organización:", error);
          msg.error(
            error?.response?.data?.message || "Error al crear la organización"
          );
        },
      },
    });

  type FormValues = {
    name: string;
    identifier: string;
    type: "hospital" | "clinica" | "centro_salud" | "laboratorio";
    active: boolean;
    description?: string;
    phone?: string;
    email?: string;
    apiLink?: string;
    address: string;
    city: string;
    state: string;
    postalCode: string;
  };

  const handleFinish = async (values: FormValues) => {
    const typeMap = {
      hospital: "prov",
      clinica: "prov",
      centro_salud: "prov",
      laboratorio: "dept",
    };

    const typeCode = typeMap[values.type];
if (!typeCode) {
  console.error("Tipo de organización inválido:", values.type);
  return;
}


    const payload: CreateOrganizationDto = {
      identifier: [
        {
          use: 1,
          type: {
            coding: [
              {
                system: "http://terminology.hl7.org/CodeSystem/v2-0203",
                code: "PRN",
                display: "Provider Number",
                userSelected: false,
              },
            ],
            text: "Identificador institucional",
          },
          system: "http://hospitalcentral.org/identifiers",
          value: values.identifier,
        },
      ],
      name: values.name,
      type: [
        {
          coding: [
            {
              system: "http://terminology.hl7.org/CodeSystem/organization-type",
              code: typeCode,
              display: values.type,
              userSelected: true,
            },
          ],
          text: values.type,
        },
      ],
      active: values.active,
      alias: [values.name, values.identifier],
      description:values.description || "",
      contact: [
        {
          purpose: {
            coding: [
              {
                system:
                  "http://terminology.hl7.org/CodeSystem/contactentity-type",
                code: "ADMIN",
                display: "Administrativo",
                userSelected: true,
              },
            ],
            text: "Administrativo",
          },
          name: "Contacto principal",
          telecom: [
            {
              system: 0,
              value: values.phone || "",
              use: 1, 
              rank: 1,
            },
            {
              system: 2, 
              value: values.email || "",
              use: 1, 
              rank: 1,
            },
          ],
        },
      ],
      endpoint: values.apiLink
  ? [
      {
        reference: values.apiLink,
        display: "API endpoint",
        type: "Endpoint",  
        identifier: {
          use: 1,
          type: { text: "API Identifier", coding: [{ system: "string", code: "API", display: "API", userSelected: true }] },
          system: "http://hospitalcentral.org/endpoints",
          value: values.apiLink,
        },
      },
    ]
  : [],
    };

    console.log("Payload:", JSON.stringify(payload, null, 2));
    await createOrganization({ data: payload });
  };

  return {
    handleFinish,
    isPending,
  };
}
