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
    types: OrgTypeKey;
    active: boolean;
    description?: string;
    phone?: string;
    email?: string;
    apiLink?: string;
    address: string;
    city: string;
    state: string;
    country?: string;
  };

  type OrgTypeKey =
    | "prov"
    | "dept"
    | "team"
    | "govt"
    | "ins"
    | "pay"
    | "edu"
    | "reli"
    | "crs"
    | "cg"
    | "bus"
    | "other";

  const handleFinish = async (values: FormValues) =>  {
    const typeMap: Record<OrgTypeKey, { code: string; display: string }> = {
      prov: { code: "prov", display: "Proveedor de salud" },
      dept: { code: "dept", display: "Departamento" },
      team: { code: "team", display: "Equipo" },
      govt: { code: "govt", display: "Gobierno" },
      ins: { code: "ins", display: "Aseguradora" },
      pay: { code: "pay", display: "Pagador" },
      edu: { code: "edu", display: "Educativo" },
      reli: { code: "reli", display: "Religioso" },
      crs: { code: "crs", display: "Investigación clínica" },
      cg: { code: "cg", display: "Comunidad" },
      bus: { code: "bus", display: "Negocio no médico" },
      other: { code: "other", display: "Otro" },
    };

    if (!values.types) {
      values.types = "prov";
    }

    console.log("Tipo seleccionado:", values.types);
    console.log("Mapa de tipo:", typeMap[values.types]);

    const payload: CreateOrganizationDto & { types?: any } = {
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
          value: Array.isArray(values.identifier)
            ? values.identifier[0]
            : values.identifier,
        },
      ],
      active: values.active,
      types: [
        {
          coding: [
            {
              system: "http://terminology.hl7.org/CodeSystem/organization-type",
              version: "1.0",
              code: typeMap[values.types].code,
              display: typeMap[values.types].display,
              userSelected: true,
            },
          ],
          text: typeMap[values.types].display,
        },
      ],
      name: values.name,
      alias: [values.name],
      description: values.description || "",

      contact: [
        {
          purpose: {
            coding: [
              {
                system:
                  "http://terminology.hl7.org/CodeSystem/contactentity-type",
                version: "1.0",
                code: "ADMIN",
                display: "Administrativo",
                userSelected: true,
              },
            ],
            text: "Administrativo",
          },
          name: "Contacto principal",
          telecom: [
            { system: 0, value: values.phone || "", use: 0, rank: 1 },
            { system: 2, value: values.email || "", use: 0, rank: 2 },
          ],
          address: {
            use: 0,
            type: 0,
            text: values.address || "",
            line: [values.address || ""],
            city: values.city || "",
            state: values.state || "",
            country: values.country || "Honduras",
          },
        },
      ],
    };

    console.log("Payload:", JSON.stringify(payload, null, 2));
    await createOrganization({ data: payload });
  };

  return {
    handleFinish,
    isPending,
  };
}
