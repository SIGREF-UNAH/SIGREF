import { useState, useEffect, useRef } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import type { ProFormInstance } from "@ant-design/pro-components";
import {
  useGetApiLocations,
  useGetApiLocationsId,
  usePostApiLocations,
  usePutApiLocationsId,
  getGetApiLocationsQueryKey,
} from "../../../api/locations/locations";
import { useGetApiOrganizations } from "../../../api/organizations/organizations";
import { message } from "antd";
import ccsj from "countrycitystatejson";

type Mode = "create" | "edit";

interface UseLocationFormProps {
  mode: Mode;
}

export default function useLocationForm({ mode }: UseLocationFormProps) {
  const navigate = useNavigate();
  const { id } = useParams();
  const numericId = id ? Number(id) : null;
  const isEdit = mode === "edit" && numericId !== null;
  const queryClient = useQueryClient();
  const formRef = useRef<ProFormInstance>(null);

  const [contacts, setContacts] = useState<
    { id: string; system?: number; value?: string }[]
  >([{ id: "1", system: undefined, value: "" }]);

  const [countryOptions] = useState(() =>
    ccsj.getCountries().map((c: any) => ({
      label: c.name,
      value: c.shortName,
    }))
  );
  const [stateOptions, setStateOptions] = useState<
    { label: string; value: string }[]
  >([]);
  const [cityOptions, setCityOptions] = useState<
    { label: string; value: string }[]
  >([]);

  // Queries
  const {
    data: location,
    isLoading: locationLoading,
    isError: locationError,
  } = useGetApiLocationsId(numericId!, { query: { enabled: isEdit } });

  const {
    data: orgsData,
    isLoading: orgsLoading,
    isError: orgsError,
  } = useGetApiOrganizations(undefined, {
    query: { select: (data) => data.items || [] },
  });

  const {
    data: locationsData,
    isLoading: locationsLoading,
    isError: locationsError,
  } = useGetApiLocations(undefined, {
    query: { select: (data) => data.items || [] },
  });

  // Mutations
  const { mutateAsync: createLocation, isPending: isCreating } =
    usePostApiLocations({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetApiLocationsQueryKey(),
          });
          message.success("Ubicación creada");
          navigate("/locations/list");
        },
        onError: () => message.error("Error al crear"),
      },
    });

  const { mutate: updateLocation, isPending: isUpdating } =
    usePutApiLocationsId({
      mutation: {
        onSuccess: () => {
          queryClient.invalidateQueries({
            queryKey: getGetApiLocationsQueryKey(),
          });
          message.success("Ubicación actualizada");
          navigate("/locations/list");
        },
        onError: () => message.error("Error al actualizar"),
      },
    });

  // Opciones
  const organizationOptions =
    orgsData?.map((org) => ({
      label: org.name,
      value: org.id,
    })) || [];

  const locationOptions = (locationsData || [])
    .filter((loc) => !isEdit || String(loc.id) !== String(numericId))
    .map((loc) => ({
      label: loc.name,
      value: loc.id,
    }));

  // Cargar datos en edición
  useEffect(() => {
    if (isEdit && location && formRef.current) {
      const telecoms = location.telecom || [];
      setContacts(
        telecoms.length > 0
          ? telecoms.map((t: any, i: number) => ({
              id: Date.now().toString() + i,
              system: t.system,
              value: t.value,
            }))
          : [{ id: "1", system: undefined, value: "" }]
      );

      if (location.address?.country) {
        const states = ccsj.getStatesByShort(location.address.country) ?? [];
        setStateOptions(states.map((s) => ({ label: s, value: s })));

        if (location.address?.state) {
          const cities =
            ccsj.getCities(location.address.country, location.address.state) ??
            [];
          setCityOptions(cities.map((c) => ({ label: c, value: c })));
        }
      }

      const partOfId = location.partOf?.reference?.split("/")?.[1] || null;
      const managingOrgId =
        location.managingOrganization?.reference?.split("/")?.[1] || null;

      formRef.current.setFieldsValue({
        name: location.name || "",
        alias: location.alias || [],
        description: location.description || null,
        status: location.status,
        mode: location.mode,
        address: {
          line: location.address?.line || [""],
          city: location.address?.city || null,
          state: location.address?.state || null,
          postalCode: location.address?.postalCode || null,
          country: location.address?.country || null,
        },
        type: location.type || null,
        partOf: partOfId,
        managingOrganization: managingOrgId,
        telecom: telecoms.map((t: any) => ({
          system: t.system,
          value: t.value,
        })),
      });
    }
  }, [location, isEdit]);

  // Handlers
  const handleCountryChange = (countryShort?: string) => {
    if (!countryShort) {
      setStateOptions([]);
      setCityOptions([]);
      formRef.current?.setFieldValue(["address", "state"], null);
      formRef.current?.setFieldValue(["address", "city"], null);
      return;
    }
    const states = ccsj.getStatesByShort(countryShort) ?? [];
    setStateOptions(states.map((s) => ({ label: s, value: s })));
    setCityOptions([]);
    formRef.current?.setFieldValue(["address", "state"], null);
    formRef.current?.setFieldValue(["address", "city"], null);
  };

  const handleStateChange = (stateName?: string) => {
    const countryShort = formRef.current?.getFieldValue(["address", "country"]);
    if (!countryShort || !stateName) {
      setCityOptions([]);
      formRef.current?.setFieldValue(["address", "city"], null);
      return;
    }
    const cities = ccsj.getCities(countryShort, stateName) ?? [];
    setCityOptions(cities.map((c) => ({ label: c, value: c })));
  };

  const addContact = () => {
    setContacts((prev) => [
      ...prev,
      { id: Date.now().toString(), system: undefined, value: "" },
    ]);
  };

  const removeContact = (id: string) => {
    if (contacts.length > 1) {
      setContacts((prev) => prev.filter((c) => c.id !== id));
    }
  };

  const updateContact = (id: string, field: "system" | "value", value: any) => {
    setContacts((prev) =>
      prev.map((c) => (c.id === id ? { ...c, [field]: value } : c))
    );
  };

  const onFinish = async (values: any) => {
    let partOf: { reference: string; display: string; type?: string } | null =
      null;

    let managingOrganization: {
      reference: string;
      display: string;
      type?: string;
    } | null = null;

    if (values.managingOrganization) {
      const selectedOrg = orgsData?.find(
        (org) => String(org.id) === String(values.managingOrganization)
      );
      if (selectedOrg) {
        managingOrganization = {
          reference: `Organization/${selectedOrg.id}`,
          display: selectedOrg.name as string,
          type: "Organization",
        };
      }
    }

    if (values.partOf) {
      const selectedLoc = locationsData?.find(
        (loc) => String(loc.id) === String(values.partOf)
      );
      if (selectedLoc) {
        partOf = {
          reference: `Location/${selectedLoc.id}`,
          display: selectedLoc.name,
          type: "Location",
        };
      }
    }

    const payload = {
      ...values,
      partOf,
      managingOrganization,
      telecom: contacts
        .filter((c) => c.system !== undefined && c.value)
        .map((c) => ({ system: c.system, value: c.value })),
    };

    if (isEdit) {
      updateLocation({ id: numericId!, data: payload });
    } else {
      if (!payload.name) return message.error("Nombre obligatorio");
      if (!payload.address?.line?.[0])
        return message.error("Dirección obligatoria");
      await createLocation({ data: payload });
    }
  };

  const isSubmitting = isCreating || isUpdating;

  return {
    // Form
    formRef,
    onFinish,
    isSubmitting,

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

    // Modo
    isEdit,
    title: isEdit ? "Editar Ubicación" : "Crear Ubicación",
  };
}
