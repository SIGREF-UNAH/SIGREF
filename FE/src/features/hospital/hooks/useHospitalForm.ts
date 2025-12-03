import type { ProFormInstance } from "@ant-design/pro-components";
import { useEffect, useRef, useState } from "react";
import ccsj from "countrycitystatejson";

export default function useHospitalForm(initialValues?: any) {
  // formRef para el componente PhoneInput
  const formRef = useRef<ProFormInstance>(null);

  // Variables y funciones para selectores de ubicación
  const [countryOptions] = useState( ccsj.getCountries().map((c) => ({ label: c.name, value: c.shortName })));
  const [stateOptions, setStateOptions] = useState<{ label: string; value: string }[]>([]);
  const [cityOptions, setCityOptions] = useState<{ label: string; value: string }[]>([]);

  // Estados para manejar los valores seleccionados
  const [selectedCountry, setSelectedCountry] = useState<string>("");
  const [selectedState, setSelectedState] = useState<string>("");
  const [selectedCity, setSelectedCity] = useState<string>("");

  // Función para construir la ubicación completa
  const buildUbication = (
    country: string,
    state: string,
    city: string
  ): string => {
    const countryObj = countryOptions.find((c) => c.value === country);
    const countryName = countryObj ? countryObj.label : country;

    if (city && state && countryName) {
      return `${city}, ${state}, ${countryName}`;
    } else if (city && state) {
      return `${city}, ${state}`;
    } else if (city && countryName) {
      return `${city}, ${countryName}`;
    } else if (state && countryName) {
      return `${state}, ${countryName}`;
    } else if (city) {
      return city;
    } else if (state) {
      return state;
    } else if (countryName) {
      return countryName;
    }
    return "";
  };

  // Función para combinar ubicación con detalles
  const buildCompleteLocation = (location: string, details: string): string => {
    if (location && details) {
      return `${location} - ${details}`;
    } else if (location) {
      return location;
    } else if (details) {
      return details;
    }
    return "";
  };

  // Inicializar valores si estamos en modo edición
  useEffect(() => {
    if (initialValues && formRef.current) {
      let details = "";
      let location = initialValues.ubication || "";

      if (location && location.includes(" - ")) {
        const parts = location.split(" - ");
        location = parts[0];
        details = parts.slice(1).join(" - ");
      }

      // Establecer detalles
      formRef.current.setFieldValue("details", details);

      // Intentar parsear la ubicación para los selectores
      if (location) {
        const locationParts = location.split(", ");

        if (locationParts.length >= 3) {
          const city = locationParts[0];
          const state = locationParts[1];
          const countryName = locationParts[2];

          const countryObj = countryOptions.find(
            (c) => c.label.toLowerCase() === countryName.toLowerCase()
          );

          if (countryObj) {
            handleCountryChange(countryObj.value, true);
            setSelectedCountry(countryObj.value);
            setSelectedState(state);
            setSelectedCity(city);
          }
        } else if (locationParts.length === 2) {
          const firstPart = locationParts[0];
          const secondPart = locationParts[1];

          const countryObj = countryOptions.find(
            (c) => c.label.toLowerCase() === secondPart.toLowerCase()
          );

          if (countryObj) {
            handleCountryChange(countryObj.value, true);
            setSelectedCountry(countryObj.value);
            setSelectedCity(firstPart);
          } else {
            setSelectedState(secondPart);
            setSelectedCity(firstPart);
          }
        } else if (locationParts.length === 1) {
          const singlePart = locationParts[0];

          const countryObj = countryOptions.find(
            (c) => c.label.toLowerCase() === singlePart.toLowerCase()
          );

          if (countryObj) {
            handleCountryChange(countryObj.value, true);
            setSelectedCountry(countryObj.value);
          } else {
            setSelectedCity(singlePart);
          }
        }
      }
    }
  }, [initialValues]);

  const handleCountryChange = (
    countryShort?: string,
    isInitializing = false
  ) => {
    if (!countryShort) {
      setStateOptions([]);
      setCityOptions([]);
      setSelectedCountry("");
      setSelectedState("");
      setSelectedCity("");
      if (!isInitializing) {
        formRef.current?.setFieldValue("state", null);
        formRef.current?.setFieldValue("city", null);
        formRef.current?.setFieldValue("country", null);
      }
      return;
    }

    setSelectedCountry(countryShort);
    const states = ccsj.getStatesByShort(countryShort) ?? [];
    setStateOptions(states.map((s: string) => ({ label: s, value: s })));
    setCityOptions([]);

    if (!isInitializing) {
      setSelectedState("");
      setSelectedCity("");
      formRef.current?.setFieldValue("state", null);
      formRef.current?.setFieldValue("city", null);
      formRef.current?.setFieldValue("country", countryShort);
    }
  };

  const handleStateChange = (stateName?: string) => {
    if (!stateName) {
      setCityOptions([]);
      setSelectedState("");
      setSelectedCity("");
      formRef.current?.setFieldValue("city", null);
      return;
    }

    setSelectedState(stateName);
    const countryShort =
      formRef.current?.getFieldValue("country") || selectedCountry;

    if (!countryShort) {
      setCityOptions([]);
      return;
    }

    const cities = ccsj.getCities(countryShort, stateName) ?? [];
    setCityOptions(cities.map((c: string) => ({ label: c, value: c })));
    setSelectedCity("");
    formRef.current?.setFieldValue("city", null);
  };

  const handleCityChange = (cityName?: string) => {
    setSelectedCity(cityName || "");
  };

  return {
    formRef,
    countryOptions,
    stateOptions,
    cityOptions,
    selectedCountry,
    selectedState,
    selectedCity,
    buildUbication,
    buildCompleteLocation,
    handleCountryChange,
    handleStateChange,
    handleCityChange,
  };
}
