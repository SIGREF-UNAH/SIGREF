import { useNavigate, useParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import type { UpdateOrganizationDto, CodingDto } from "@models";
import { 
} from "@models";
import { OrganizationTypeEnum } from "../types";
import { NullableOfContactPointSystem as ContactPointSystem, NullableOfContactPointUse as ContactPointUse } from "@types/shared";
import type { OrganizationFormValues } from "../components/OrganizationsForm";
import {
  getGetOrganizationListQueryKey,
  useGetOrganizationById,
  useUpdateOrganizationById,
} from "@endpoints/organizations/organizations";

/**
 * Hook personalizado para actualizar organizaciones
 * 
 * Utiliza la terminología estándar HL7 FHIR v4.3.0 para tipos de organización
 * @see http://hl7.org/fhir/R4/valueset-organization-type.html
 * @see http://terminology.hl7.org/CodeSystem/organization-type
 * @see http://hl7.org/fhir/R4/valueset-contact-point-system.html
 * @see http://hl7.org/fhir/R4/valueset-contact-point-use.html
 * 
 * Los códigos y displays están sujetos a cambios según los ValueSets de HL7 FHIR
 */
export function useUpdateOrganization() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  /**
   * Mapeo de tipos de organización de FHIR a códigos HL7 estándar
   * 
   * @see http://terminology.hl7.org/CodeSystem/organization-type
   * 
   * NOTA: Los códigos HL7 son los valores canónicos para la API FHIR.
   * Los nombres en español son para la interfaz de usuario (UI).
   * Este mapeo está sujeto a cambios según evolucionen los ValueSets.
   */
  const organizationTypeToFhirMap: Record<OrganizationTypeEnum, { 
    code: string;
    display: string;
    displayEs: string;
    definition: string;
  }> = {
    [OrganizationTypeEnum.provider]: { 
      code: "prov", 
      display: "Healthcare Provider",
      displayEs: "Proveedor de Salud",
      definition: "An organization that provides healthcare services."
    },
    [OrganizationTypeEnum.department]: { 
      code: "dept", 
      display: "Hospital Department",
      displayEs: "Departamento Hospitalario",
      definition: "A department or ward within a hospital"
    },
    [OrganizationTypeEnum.team]: { 
      code: "team", 
      display: "Organizational team",
      displayEs: "Equipo Organizacional",
      definition: "An organizational team is usually a grouping of practitioners"
    },
    [OrganizationTypeEnum.government]: { 
      code: "govt", 
      display: "Government",
      displayEs: "Gobierno",
      definition: "A political body"
    },
    [OrganizationTypeEnum.insurer]: { 
      code: "ins", 
      display: "Insurance Company",
      displayEs: "Compañía de Seguros",
      definition: "A company that provides insurance to its subscribers"
    },
    [OrganizationTypeEnum.payer]: { 
      code: "pay", 
      display: "Payer",
      displayEs: "Pagador",
      definition: "A company, charity, or governmental organization, which processes claims"
    },
    [OrganizationTypeEnum.educational]: { 
      code: "edu", 
      display: "Educational Institute",
      displayEs: "Instituto Educativo",
      definition: "An educational institution that provides education or research facilities"
    },
    [OrganizationTypeEnum.regligious]: { 
      code: "reli", 
      display: "Religious Institution",
      displayEs: "Institución Religiosa",
      definition: "An organization that is identified as a part of a religious institution"
    },
    [OrganizationTypeEnum.clinicalResearchSponsor]: { 
      code: "crs", 
      display: "Clinical Research Sponsor",
      displayEs: "Patrocinador de Investigación Clínica",
      definition: "An organization that is identified as a Pharmaceutical/Clinical Research Sponsor"
    },
    [OrganizationTypeEnum.communityGroup]: { 
      code: "cg", 
      display: "Community Group",
      displayEs: "Grupo Comunitario",
      definition: "An un-incorporated community group"
    },
    [OrganizationTypeEnum.nonHealthcareBusiness]: { 
      code: "bus", 
      display: "Non-Healthcare Business or Corporation",
      displayEs: "Negocio o Corporación No Sanitaria",
      definition: "An organization that is a registered business or corporation"
    },
    [OrganizationTypeEnum.network]: { 
      code: "other", 
      display: "Other",
      displayEs: "Otro",
      definition: "Other type of organization not already specified"
    },
  };

  /**
   * Mapeo inverso: de código HL7 a OrganizationTypeEnum
   */
  const fhirCodeToOrgTypeMap: Record<string, OrganizationTypeEnum> = {
    "prov": OrganizationTypeEnum.provider,
    "dept": OrganizationTypeEnum.department,
    "team": OrganizationTypeEnum.team,
    "govt": OrganizationTypeEnum.government,
    "ins": OrganizationTypeEnum.insurer,
    "pay": OrganizationTypeEnum.payer,
    "edu": OrganizationTypeEnum.educational,
    "reli": OrganizationTypeEnum.regligious,
    "crs": OrganizationTypeEnum.clinicalResearchSponsor,
    "cg": OrganizationTypeEnum.communityGroup,
    "bus": OrganizationTypeEnum.nonHealthcareBusiness,
    "other": OrganizationTypeEnum.network,
  };

  // Obtener organización por ID
  const {
    data: organization,
    isLoading,
    isError,
  } = useGetOrganizationById(id!, {
    query: { enabled: !!id },
  });

  // Mutación para actualizar
  const { mutateAsync: updateOrganization, isPending } =
    useUpdateOrganizationById({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetOrganizationListQueryKey(),
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

  /**
   * Extrae datos de contacto de manera segura
   */
  const extractContactData = () => {
    const telecom = organization?.contact?.flatMap((c) => c.telecom || []) || [];
    
    const phone = telecom.find(
      (t) => t.system === ContactPointSystem.phone
    )?.value || "";
    
    const email = telecom.find(
      (t) => t.system === ContactPointSystem.email
    )?.value || "";
    
    const addressObj = organization?.contact?.[0]?.address;

    return { phone, email, addressObj };
  };

  const { phone, email, addressObj } = extractContactData();

  /**
   * Limpia el HTML de la descripción para mostrarlo en el formulario
   */
  const formatDescription = (html?: string | null): string => {
    if (!html) return "";
    return html.replace(/<[^>]*>/g, '').trim();
  };

  /**
   * Extrae de manera segura el código FHIR del tipo de organización
   */
  const extractFhirCode = (): string | null => {
    const typeItem = organization?.type?.[0];
    if (!typeItem) return null;

    const coding = (typeItem as any)?.coding;
    if (Array.isArray(coding) && coding.length > 0) {
      const firstCoding = coding[0] as CodingDto;
      return firstCoding?.code || null;
    }

    return null;
  };

  /**
   * Obtiene el tipo de organización para el formulario
   */
  const getOrganizationType = (): OrganizationTypeEnum => {
    const fhirCode = extractFhirCode();
    if (!fhirCode) {
      return OrganizationTypeEnum.provider;
    }
    
    return fhirCodeToOrgTypeMap[fhirCode] || OrganizationTypeEnum.provider;
  };

  // Valores iniciales para el formulario
  const initialValues: OrganizationFormValues | undefined = organization
    ? {
        name: organization.name || "",
        identifier: organization.identifier?.[0]?.value || "",
        type: getOrganizationType(),
        active: organization.active ?? true,
        description: formatDescription(organization.description),
        phone: phone || "",
        email: email || "",
        address: addressObj?.text || "",
        city: addressObj?.city || "",
        state: addressObj?.state || "",
        country: addressObj?.country || "",
        postalCode: addressObj?.postalCode || "",
      }
    : undefined;

  /**
   * Maneja el envío del formulario de actualización
   */
  const handleFinish = async (values: OrganizationFormValues) => {
    if (!id) {
      msg.error("ID de la organización no encontrado");
      return;
    }

    const typeMapping = organizationTypeToFhirMap[values.type];

    const payload: UpdateOrganizationDto = {
      name: values.name,
      identifier: [
        {
          use: "official" as const,
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
      type: [
        {
          coding: [
            {
              system: "http://terminology.hl7.org/CodeSystem/organization-type",
              version: "4.3.0",
              code: typeMapping.code,
              display: typeMapping.display,
              userSelected: true,
            },
          ],
          text: typeMapping.displayEs,
        },
      ],
      active: values.active,
      description: values.description || null,
      alias: [values.name],
      contact: [
        {
          purpose: {
            coding: [
              {
                system: "http://terminology.hl7.org/CodeSystem/contactentity-type",
                version: "4.3.0",
                code: "ADMIN",
                display: "Administrative",
                userSelected: true,
              },
            ],
            text: "Administrativo",
          },
          name: "Contacto principal",
          telecom: [
            { 
              system: ContactPointSystem.phone,
              value: values.phone || null, 
              use: ContactPointUse.work,
              rank: 1 
            },
            { 
              system: ContactPointSystem.email,
              value: values.email || null, 
              use: ContactPointUse.work,
              rank: 2 
            },
          ],
          address: {
            use: "work" as const,
            type: "both" as const,
            text: values.address || null,
            line: [values.address || ""],
            city: values.city || null,
            state: values.state || null,
            country: values.country || null,
            postalCode: values.postalCode || null,
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
    organizationTypeToFhirMap,
  };
}
