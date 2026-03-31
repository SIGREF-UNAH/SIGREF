import type { ProFormInstance } from "@ant-design/pro-components";
import { useRef, useState, useEffect } from "react";
import { message } from "antd";
import {
  getCityOptionsByCountryAndState,
  getCountryOptions,
  getStateOptionsByCountry,
} from "../../../shared/utils";

export default function usePatientForm(
  onSubmit: any,
  mode: string,
  initialValues?: any
) {
  const [messageApi, contextHolder] = message.useMessage();
  const formRef = useRef<ProFormInstance>(null);

  // Estado para manejar los tipos de contacto seleccionados por cada campo
  const [contactTypes, setContactTypes] = useState<{ [key: number]: string }>({});

  // Estados para los selectores de ubicación
  const [countryOptions] = useState(() =>
    getCountryOptions()
  );

  // Estado para guardar las opciones de estados y ciudades por cada dirección
  const [addressLocations, setAddressLocations] = useState<{
    [key: number]: {
      stateOptions: { label: string; value: string }[];
      cityOptions: { label: string; value: string }[];
    };
  }>({});

  // Función para manejar el cambio de tipo de contacto
  const handleContactTypeChange = (index: number, system: string) => {
    setContactTypes((prev) => ({
      ...prev,
      [index]: system,
    }));
  };

  // Función auxiliar para obtener placeholder según el tipo
  const getPlaceholderByType = (type: string) => {
    switch (type) {
      case "Url":
        return "https://ejemplo.com";
      case "Pager":
        return "Número de biper";
      case "SMS":
        return "Número para SMS";
      case "Other":
        return "Información de contacto";
      default:
        return "Valor del contacto";
    }
  };

  // Función para manejar el cambio de país
  const handleCountryChange = (index: number, countryShort?: string) => {
    if (!countryShort) {
      setAddressLocations((prev) => ({
        ...prev,
        [index]: { stateOptions: [], cityOptions: [] },
      }));
      // Limpiar campos de estado y ciudad en el formulario
      if (formRef.current) {
        formRef.current.setFieldValue(["address", index, "state"], null);
        formRef.current.setFieldValue(["address", index, "city"], null);
      }
      return;
    }

    setAddressLocations((prev) => ({
      ...prev,
      [index]: {
        stateOptions: getStateOptionsByCountry(countryShort),
        cityOptions: [],
      },
    }));

    // Limpiar campos de estado y ciudad en el formulario
    if (formRef.current) {
      formRef.current.setFieldValue(["address", index, "state"], null);
      formRef.current.setFieldValue(["address", index, "city"], null);
    }
  };

  // Función para manejar el cambio de estado
  const handleStateChange = (
    index: number,
    countryShort: string,
    stateName?: string
  ) => {
    if (!countryShort || !stateName) {
      setAddressLocations((prev) => ({
        ...prev,
        [index]: {
          ...prev[index],
          cityOptions: [],
        },
      }));
      // Limpiar campo de ciudad en el formulario
      if (formRef.current) {
        formRef.current.setFieldValue(["address", index, "city"], null);
      }
      return;
    }

    setAddressLocations((prev) => ({
      ...prev,
      [index]: {
        ...prev[index],
        cityOptions: getCityOptionsByCountryAndState(countryShort, stateName),
      },
    }));

    // Limpiar campo de ciudad en el formulario
    if (formRef.current) {
      formRef.current.setFieldValue(["address", index, "city"], null);
    }
  };

  // Inicializar opciones de estado y ciudad si hay valores iniciales
  useEffect(() => {
    if (mode === "edit" && initialValues?.address && formRef.current) {
      initialValues.address.forEach((addr: any, index: number) => {
        if (addr.country) {
          const stateOptions = getStateOptionsByCountry(addr.country);

          let cityOptions: { label: string; value: string }[] = [];
          if (addr.state) {
            cityOptions = getCityOptionsByCountryAndState(
              addr.country,
              addr.state
            );
          }

          setAddressLocations((prev) => ({
            ...prev,
            [index]: { stateOptions, cityOptions },
          }));
        }
      });
    }

    // Inicializar tipos de contacto si hay valores iniciales
    if (mode === "edit" && initialValues?.telecom && formRef.current) {
      const initialContactTypes: { [key: number]: string } = {};
      initialValues.telecom.forEach((contact: any, index: number) => {
        if (contact.system) {
          initialContactTypes[index] = contact.system;
        }
      });
      setContactTypes(initialContactTypes);
    }
  }, [mode, initialValues]);

  // Función para enviar el formulario
  const onFinish = async (values: any) => {
    await onSubmit(values);
  };

  // Función para cancelar
  const onCancel = () => {
    messageApi.info("Operación cancelada");
  };

  return {
    contextHolder,
    contactTypes,
    countryOptions,
    addressLocations,
    formRef,
    onFinish,
    onCancel,
    handleContactTypeChange,
    getPlaceholderByType,
    handleCountryChange,
    handleStateChange,
  };
}
