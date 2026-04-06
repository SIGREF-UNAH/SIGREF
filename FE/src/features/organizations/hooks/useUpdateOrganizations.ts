import { useNavigate, useParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import type { CreateOrganizationDto } from "../../../api/models";
import {
  getGetApiOrganizationsQueryKey,
  useGetApiOrganizationsId,
  usePutApiOrganizationsId,
} from "../../../api/organizations/organizations";

export function useUpdateOrganization() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  interface OrganizationFormValues {
    name: string;
    identifier: string;
    type: string; // Cambiado a string para el formulario
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

  // Mapeo seguro de datos
  let telecom = organization?.contact?.flatMap((c) => c.telecom || []) || [];

  const phone = telecom.find((t) => String(t.system).toLowerCase() === "phone")?.value || null;
  const email = telecom.find((t) => String(t.system).toLowerCase() === "email")?.value || null;
  const addressObj = organization?.contact?.[0]?.address;

  const formatDescription = (html: string) => {
    if (!html) return "";
    // Remover todos los tags HTML
    return html.replace(/<[^>]*>/g, '').trim();
  };

  // Obtener el tipo de organización para el formulario
  const getOrganizationType = () => {
    if (!organization?.type?.[0]) return "oth"; // Valor por defecto
    
    // Priorizar el código del type
    return organization.type[0]?.coding?.[0]?.code || "oth";
  };

  const initialValues: OrganizationFormValues | undefined = organization
    ? {
        name: organization.name || "",
        identifier: organization.identifier?.[0]?.value || "",
        type: getOrganizationType(), // Usar el código para el formulario
        active: organization.active ? true : false,
        description: formatDescription(organization.description || ""),
        phone: phone || "",
        email: email || "",
        address: addressObj?.text || "",
        city: addressObj?.city || "",
        state: addressObj?.state || "",
        country: addressObj?.country || "",
        postalCode: addressObj?.postalCode || "",
      }
    : undefined;

  // Enviar datos
  const handleFinish = async (values: OrganizationFormValues) => {
    if (!id) {
      msg.error("ID de la organización no encontrado");
      return;
    }

    // Mapear el tipo seleccionado a la estructura correcta
    const getTypeStructure = (typeCode: string) => {
      const typeMappings: { [key: string]: { code: string; display: string } } = {
        "prov": { code: "prov", display: "Proveedor" },
        "dept": { code: "dept", display: "Departamento" },
        "team": { code: "team", display: "Equipo" },
        "govt": { code: "govt", display: "Gobierno" },
        "ins": { code: "ins", display: "Aseguradora" },
        "pay": { code: "pay", display: "Pago" },
        "edu": { code: "edu", display: "Educativa" },
        "reli": { code: "reli", display: "Religiosa" },
        "cr": { code: "cr", display: "Centro de Investigación" },
        "other": { code: "other", display: "Otro" },
        "bus": { code: "bus", display: "Negocio" },
        "oth": { code: "oth", display: "Otro" },
      };

      const selectedType = typeMappings[typeCode] || typeMappings["oth"];

      return [
        {
          coding: [
            {
              system: "http://terminology.hl7.org/CodeSystem/organization-type",
              version: "1.0",
              code: selectedType.code,
              display: selectedType.display,
              userSelected: true,
            },
          ],
          text: selectedType.display,
        },
      ];
    };

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
          value: values.identifier || "",
        },
      ],
      type: getTypeStructure(values.type), // Usar la función mapeadora
      active: values.active,
      description: values.description || "",
      contact: [
        {
          purpose: {
            coding: [
              {
                system: "http://terminology.hl7.org/CodeSystem/contactentity-type",
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
            country: values.country || "",
            postalCode: values.postalCode || "",
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