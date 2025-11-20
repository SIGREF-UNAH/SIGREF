import { useNavigate, useParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import {
  getGetApiOrganizationsQueryKey,
  useGetApiOrganizationsId,
  usePutApiOrganizationsId,
} from "../../../api/organizations/organizations";
import type { CreateOrganizationDto } from "../../../api/models";

export function useUpdateOrganization() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  interface OrganizationFormValues {
    name: string;
    identifier: string;
    type: {
      coding: {
        system: string;
        version?: string;
        code: string;
        display: string;
        userSelected?: boolean;
      }[];
      text: string;
    }[];
    active: boolean;
    description?: string;
    phone?: string;
    email?: string;
    address?: string;
    city?: string;
    state?: string;
    country?: string;
    postalCode?: string;
  }

  // Obtener organización por ID
  const {
    data: organization,
    isLoading,
    isError,
  } = useGetApiOrganizationsId(id!, {
    query: { enabled: !!id },
  });

  // Mutación para actualizar
  const { mutateAsync: updateOrganization, isPending } =
    usePutApiOrganizationsId({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetApiOrganizationsQueryKey(),
          });
          msg.success("Organización actualizada correctamente");
          navigate("/organizations/list");
        },
        onError: (error: any) => {
          console.error("Error al actualizar la organización:", error);
          msg.error(
            error?.response?.data?.message ||
              "Error al actualizar la organización"
          );
        },
      },
    });

  // --- Mapeo seguro de datos ---
  let telecom = organization?.contact?.flatMap((c) => c.telecom || []) || [];

  const phone =
    telecom.find((t) => String(t.system).toLowerCase() === "phone")?.value ||
    "";
  const email =
    telecom.find((t) => String(t.system).toLowerCase() === "email")?.value ||
    "";

  const addressObj = organization?.contact?.[0]?.address;

  const initialValues: OrganizationFormValues | undefined = organization
    ? {
        name: organization.name || "",
        identifier: organization.identifier?.[0]?.value || "",
        type: organization.type?.length ? organization.type : [],
        active: organization.active ?? true,
        description: organization.description || "",
        phone,
        email,
        address: addressObj?.text || "",
        city: addressObj?.city || "",
        state: addressObj?.state || "",
        country: addressObj?.country || "",
        postalCode: addressObj?.postalCode || "",
      }
    : undefined;

  console.log("Organization raw data:", organization);
  console.log("Mapped initialValues:", initialValues);

  // --- Submit ---
  const handleFinish = async (values: OrganizationFormValues) => {
    if (!id) {
      msg.error("ID de la organización no encontrado");
      return;
    }

    // Reconstruir payload completo
    const payload: CreateOrganizationDto = {
      name: values.name,
      identifier: [
        {
          use: 0,
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
      type: values.type?.length
        ? values.type
        : [
            {
              coding: [
                {
                  system:
                    "http://terminology.hl7.org/CodeSystem/organization-type",
                  version: "1.0",
                  code: "oth",
                  display: "Otro",
                  userSelected: true,
                },
              ],
              text: "Otro",
            },
          ],
      active: values.active,
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
            country: values.country || "HN",
          },
        },
      ],
    };

    try {
      await updateOrganization({
        id,
        data: payload,
      });
    } catch (error) {
      console.error("Error en handleFinish:", error);
    }
  };

  return {
    initialValues,
    isPending,
    isLoading,
    isError,
    handleFinish,
  };
}
