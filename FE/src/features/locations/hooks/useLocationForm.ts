// TODO : Refactorizar para separar lógica de contactos en un hook aparte

import { useState, useEffect, useRef } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import type { ProFormInstance } from "@ant-design/pro-components";
import {
  useGetLocationList,
  useGetLocationById,
  useCreateLocation,
  useUpdateLocationById,
  getGetLocationListQueryKey,
  type GetLocationByIdQueryResult,
  type GetLocationListQueryResult,
} from "@endpoints/locations/locations";
import {
  useGetOrganizationList,
  type GetOrganizationListQueryResult,
} from "@endpoints/organizations/organizations";
import type {
  LocationDto,
  OrganizationDto,
  ContactPointDto,
  AddressDto,
  ReferenceDto,
  CreateLocationDto,
  UpdateLocationDto,
} from "@models";
import type { NullableOfContactPointSystem as ContactPointSystem } from "@types/shared";
import { NullableOfLocationStatus as LocationStatus, LocationMode } from "@types/locations";
import { useMessage } from "../../../shared/hooks";
import {
  getCityOptionsByCountryAndState,
  getCountryOptions,
  getStateOptionsByCountry,
} from "../../../shared/utils";

type Mode = "create" | "edit";

interface UseLocationFormProps {
  mode: Mode;
}

interface Contact {
  id: string;
  system?: ContactPointSystem;
  value: string;
}

interface SelectOption {
  label: string;
  value: string | number;
}

interface LocationFormValues {
  name: string;
  alias?: string[] | null;
  description?: string | null;
  status: LocationStatus;
  mode?: LocationMode;
  address: {
    line?: string[] | null;
    city?: string | null;
    state?: string | null;
    postalCode?: string | null;
    country?: string | null;
  };
  type?: string | null;
  telecom?: ContactPointDto[] | null;
  partOf?: string | null;
  managingOrganization?: string | null;
}

type ContactTypesMap = Record<string, ContactPointSystem>;

export default function useLocationForm({ mode }: UseLocationFormProps) {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const numericId = id ? Number(id) : null;
  const isEdit = mode === "edit" && numericId !== null;
  const queryClient = useQueryClient();
  const formRef = useRef<ProFormInstance>(null);
  const message = useMessage();

  const [contacts, setContacts] = useState<Contact[]>([
    { id: "1", system: undefined, value: "" },
  ]);

  const [countryOptions] = useState<SelectOption[]>(() => getCountryOptions());
  const [stateOptions, setStateOptions] = useState<SelectOption[]>([]);
  const [cityOptions, setCityOptions] = useState<SelectOption[]>([]);
  const [contactTypes, setContactTypes] = useState<ContactTypesMap>({});

  // Queries
  const {
    data: location,
    isLoading: locationLoading,
    isError: locationError,
  } = useGetLocationById(String(numericId!), { 
    query: { enabled: isEdit } 
  }) as { 
    data: GetLocationByIdQueryResult; 
    isLoading: boolean; 
    isError: boolean;
  };

  const {
    data: orgsData,
    isLoading: orgsLoading,
    isError: orgsError,
  } = useGetOrganizationList(undefined, {
    query: { select: (data: GetOrganizationListQueryResult) => data?.items || [] },
  });

  const {
    data: locationsData,
    isLoading: locationsLoading,
    isError: locationsError,
  } = useGetLocationList(undefined, {
    query: { select: (data: GetLocationListQueryResult) => data?.items || [] },
  });

  // Mutations
  const { mutateAsync: createLocation, isPending: isCreating } =
    useCreateLocation({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetLocationListQueryKey(),
          });
          message.success("Ubicación creada correctamente");
          navigate("/locations/list");
        },
        onError: () => message.error("Error al crear"),
      },
    });

  const { mutate: updateLocation, isPending: isUpdating } =
    useUpdateLocationById({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetLocationListQueryKey(),
          });
          message.success("Ubicación actualizada correctamente");
          navigate("/locations/list");
        },
        onError: () => message.error("Error al actualizar"),
      },
    });

  // Opciones tipadas
  const organizationOptions: SelectOption[] =
    orgsData?.map((org: OrganizationDto) => ({
      label: org.name || "",
      value: org.id || "",
    })) || [];

  const locationOptions: SelectOption[] = (locationsData || [])
    .filter((loc: LocationDto) => !isEdit || String(loc.id) !== String(numericId))
    .map((loc: LocationDto) => ({
      label: loc.name || "",
      value: loc.id || "",
    }));

  // Cargar datos en edición
  useEffect(() => {
    if (isEdit && location && formRef.current) {
      const telecoms: ContactPointDto[] = location.telecom || [];
      setContacts(
        telecoms.length > 0
          ? telecoms.map((t: ContactPointDto, i: number) => ({
              id: Date.now().toString() + i,
              system: t.system,
              value: t.value || "",
            }))
          : [{ id: "1", system: undefined, value: "" }]
      );

      const address: AddressDto | undefined = location.address;

      if (address?.country) {
        setStateOptions(getStateOptionsByCountry(address.country));

        if (address?.state) {
          setCityOptions(
            getCityOptionsByCountryAndState(
              address.country,
              address.state
            )
          );
        }
      }

      const partOfReference: ReferenceDto | undefined = location.partOf;
      const managingOrgReference: ReferenceDto | undefined = location.managingOrganization;

      const partOfId: string | null = partOfReference?.reference?.split("/")?.[1] || null;
      const managingOrgId: string | null = managingOrgReference?.reference?.split("/")?.[1] || null;

      formRef.current.setFieldsValue({
        name: location.name || "",
        alias: location.alias || [],
        description: location.description || null,
        status: location.status,
        mode: location.mode || undefined,
        address: {
          line: address?.line || [""],
          city: address?.city || null,
          state: address?.state || null,
          postalCode: address?.postalCode || null,
          country: address?.country || null,
        },
        type: location.type || null,
        partOf: partOfId,
        managingOrganization: managingOrgId,
        telecom: telecoms.map((t: ContactPointDto) => ({
          system: t.system,
          value: t.value || "",
        })),
      });
    }
  }, [location, isEdit]);

  // Inicializar tipos de contacto cuando se cargan los contactos
  useEffect(() => {
    if (contacts.length > 0) {
      const initialContactTypes: ContactTypesMap = {};
      contacts.forEach((contact: Contact) => {
        if (contact.system) {
          initialContactTypes[contact.id] = contact.system;
        }
      });
      setContactTypes(initialContactTypes);
    }
  }, [contacts]);

  // Handlers
  const handleCountryChange = (countryShort?: string): void => {
    if (!countryShort) {
      setStateOptions([]);
      setCityOptions([]);
      formRef.current?.setFieldValue(["address", "state"], null);
      formRef.current?.setFieldValue(["address", "city"], null);
      return;
    }
    setStateOptions(getStateOptionsByCountry(countryShort));
    setCityOptions([]);
    formRef.current?.setFieldValue(["address", "state"], null);
    formRef.current?.setFieldValue(["address", "city"], null);
  };

  const handleStateChange = (stateName?: string): void => {
    const countryShort = formRef.current?.getFieldValue(["address", "country"]) as string | undefined;
    if (!countryShort || !stateName) {
      setCityOptions([]);
      formRef.current?.setFieldValue(["address", "city"], null);
      return;
    }
    setCityOptions(getCityOptionsByCountryAndState(countryShort, stateName));
  };

  const addContact = (): void => {
    setContacts((prev: Contact[]) => [
      ...prev,
      { id: Date.now().toString(), system: undefined, value: "" },
    ]);
  };

  const removeContact = (id: string): void => {
    if (contacts.length > 1) {
      setContacts((prev: Contact[]) => prev.filter((c: Contact) => c.id !== id));
    }
  };

  const updateContact = (
    id: string, 
    field: "system" | "value", 
    value: ContactPointSystem | string
  ): void => {
    setContacts((prev: Contact[]) =>
      prev.map((c: Contact) => (c.id === id ? { ...c, [field]: value } : c))
    );
  };

  const handleContactTypeChange = (contactId: string, system: ContactPointSystem): void => {
    setContactTypes((prev: ContactTypesMap) => ({
      ...prev,
      [contactId]: system,
    }));
    updateContact(contactId, "system", system);
  };

  const getPlaceholderByType = (type: ContactPointSystem): string => {
    switch (type) {
      case "pager":
        return "Ej. Número de biper";
      case "sms":
        return "Ej. Número para SMS";
      case "other":
        return "Ej. Información de contacto";
      case "phone":
        return "Ej. Número de teléfono";
      case "fax":
        return "Ej. Número de fax";
      case "email":
        return "Ej. Correo electrónico";
      case "url":
        return "Ej. URL de contacto";
    }
  };

  const buildAddressDto = (address: LocationFormValues["address"]): AddressDto => {
    return {
      line: address.line,
      city: address.city,
      state: address.state,
      postalCode: address.postalCode,
      country: address.country,
    };
  };

  const onFinish = async (values: LocationFormValues): Promise<void> => {
    let partOf: ReferenceDto | undefined;
    let managingOrganization: ReferenceDto | undefined;

    if (values.managingOrganization) {
      const selectedOrg = orgsData?.find(
        (org: OrganizationDto) => String(org.id) === String(values.managingOrganization)
      );
      if (selectedOrg) {
        managingOrganization = {
          reference: `Organization/${selectedOrg.id}`,
          display: selectedOrg.name || "",
          type: "Organization",
        };
      }
    }

    if (values.partOf) {
      const selectedLoc = locationsData?.find(
        (loc: LocationDto) => String(loc.id) === String(values.partOf)
      );
      if (selectedLoc) {
        partOf = {
          reference: `Location/${selectedLoc.id}`,
          display: selectedLoc.name || "",
          type: "Location",
        };
      }
    }

    const telecomPayload: ContactPointDto[] = contacts
      .filter((c: Contact) => c.system && c.value)
      .map((c: Contact) => ({ 
        system: c.system!, 
        value: c.value 
      }));

    const addressDto = buildAddressDto(values.address);

    if (isEdit && numericId) {
      const updatePayload: UpdateLocationDto = {
        name: values.name || undefined,
        alias: values.alias,
        description: values.description,
        status: values.status,
        mode: values.mode,
        address: addressDto,
        telecom: telecomPayload.length > 0 ? telecomPayload : undefined,
        type: values.type,
        partOf,
        managingOrganization,
      };
      updateLocation({ id: String(numericId), data: updatePayload });
    } else {
      if (!values.name) {
        message.error("Nombre obligatorio");
        return;
      }
      if (!values.address?.line?.[0]) {
        message.error("Dirección obligatoria");
        return;
      }

      const createPayload: CreateLocationDto = {
        name: values.name,
        status: values.status,
        alias: values.alias,
        description: values.description,
        mode: values.mode,
        address: addressDto,
        telecom: telecomPayload.length > 0 ? telecomPayload : undefined,
        type: values.type,
        partOf,
        managingOrganization,
      };
      await createLocation({ data: createPayload });
    }
  };

  const isSubmitting = isCreating || isUpdating;

  return {
    // Form
    formRef,
    isSubmitting,
    onFinish,

    // Data
    contacts,
    countryOptions,
    stateOptions,
    cityOptions,
    organizationOptions,
    locationOptions,

    // Loading / Errors
    locationLoading,
    locationError,
    orgsLoading,
    orgsError,
    locationsLoading,
    locationsError,

    // Handlers
    handleCountryChange,
    handleStateChange,
    addContact,
    removeContact,
    updateContact,

    // Contacto
    contactTypes,
    getPlaceholderByType,
    handleContactTypeChange,

    // Modo
    isEdit,
    title: isEdit ? "Editar Ubicación" : "Crear Ubicación",
  };
}
