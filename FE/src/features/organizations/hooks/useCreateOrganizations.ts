import { useNavigate } from "react-router";
import type { CreateOrganizationDto } from "@models";
import { 
} from "@models";
import { OrganizationTypeEnum } from "../types";
import { NullableOfContactPointSystem as ContactPointSystem, NullableOfContactPointUse as ContactPointUse } from "@types/shared";
import { NullableOfAddressUse as AddressUse, NullableOfAddressType as AddressType } from "@types/shared";
import { useQueryClient } from "@tanstack/react-query";
import {
  getGetOrganizationListQueryKey,
  useCreateOrganization as useCreateOrganizationMutation,
} from "@endpoints/organizations/organizations";
import { useMessage } from "../../../shared/hooks";

/**
 * Hook personalizado para crear organizaciones
 * 
 * Utiliza la terminología estándar HL7 FHIR v4.3.0 para tipos de organización
 * @see http://hl7.org/fhir/R4/valueset-organization-type.html
 * @see http://terminology.hl7.org/CodeSystem/organization-type
 * @see http://hl7.org/fhir/R4/valueset-contact-point-system.html
 * @see http://hl7.org/fhir/R4/valueset-contact-point-use.html
 * 
 * Los códigos y displays están sujetos a cambios según los ValueSets de HL7 FHIR
 */
export function useCreateOrganization() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  const { mutateAsync: createOrganization, isPending } =
    useCreateOrganizationMutation({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetOrganizationListQueryKey(),
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
    type: OrganizationTypeEnum;
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

  /**
   * Mapeo de tipos de organización de FHIR a códigos HL7 estándar
   * 
   * Mapea los valores del OrganizationTypeEnum generado por Orval
   * a los códigos estándar de HL7 FHIR v4.3.0
   * 
   * @see http://terminology.hl7.org/CodeSystem/organization-type
   * 
   * NOTA: Los códigos HL7 son los valores canónicos para la API FHIR.
   * Los nombres en español son para la interfaz de usuario (UI).
   * Este mapeo está sujeto a cambios según evolucionen los ValueSets.
   */
  const organizationTypeToFhirMap: Record<OrganizationTypeEnum, { 
    code: string;           // Código HL7 FHIR estándar
    display: string;        // Display oficial HL7 FHIR (inglés)
    displayEs: string;      // Nombre en español para la UI
    definition: string;     // Definición del tipo según HL7
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

  const handleFinish = async (values: FormValues) =>  {
    const typeMapping = organizationTypeToFhirMap[values.type];

    /**
     * Construcción del payload FHIR para crear organización
     * 
     * Sistemas de terminología utilizados:
     * - Identificador: http://terminology.hl7.org/CodeSystem/v2-0203 (HL7 v2 Identifier Type)
     * - Tipo de organización: http://terminology.hl7.org/CodeSystem/organization-type (FHIR v4.3.0)
     * - Tipo de contacto: http://terminology.hl7.org/CodeSystem/contactentity-type (FHIR v4.3.0)
     * 
     * @see https://www.hl7.org/fhir/organization.html
     * @see https://www.hl7.org/fhir/datatypes.html#Identifier
     * @see https://www.hl7.org/fhir/datatypes.html#ContactPoint
     */
    const payload: CreateOrganizationDto = {
      identifier: [
        {
          use: "official" as const,
          type: {
            coding: [
              {
                system: "http://terminology.hl7.org/CodeSystem/v2-0203",
                code: "PRN", // Provider Number
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
          text: typeMapping.displayEs, // Mostrar en español en la UI
        },
      ],
      name: values.name,
      alias: [values.name],
      description: values.description || null,

      contact: [
        {
          purpose: {
            coding: [
              {
                system: "http://terminology.hl7.org/CodeSystem/contactentity-type",
                version: "4.3.0",
                code: "ADMIN", // Administrative contact
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
            use: AddressUse.work,
            type: AddressType.both,
            text: values.address || null,
            line: [values.address || ""],
            city: values.city || null,
            state: values.state || null,
            country: values.country || null,
          },
        },
      ],
    };

    await createOrganization({ data: payload });
  };

  return {
    handleFinish,
    isPending,
    organizationTypeToFhirMap, // Exportamos para usar en la UI (nombres en español)
  };
}
