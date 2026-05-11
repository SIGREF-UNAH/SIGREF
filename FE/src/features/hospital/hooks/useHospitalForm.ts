import type { ProFormInstance } from "@ant-design/pro-components";
import { useEffect, useRef, useState } from "react";
import {
  getCityOptionsByCountryAndState,
  getCountryOptions,
  getStateOptionsByCountry,
} from "../../../shared/utils";

export default function useHospitalForm(initialValues?: any) {
  // formRef para el componente PhoneInput
  const formRef = useRef<ProFormInstance>(null);

  // Variables y funciones para selectores de ubicación
  const [countryOptions] = useState(() => getCountryOptions());
  const [stateOptions, setStateOptions] = useState<
    { label: string; value: string }[]
  >([]);
  const [cityOptions, setCityOptions] = useState<
    { label: string; value: string }[]
  >([]);

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
    }
    if (city && state) {
      return `${city}, ${state}`;
    }
    if (city && countryName) {
      return `${city}, ${countryName}`;
    }
    if (state && countryName) {
      return `${state}, ${countryName}`;
    }
    if (city) {
      return city;
    }
    if (state) {
      return state;
    }
    if (countryName) {
      return countryName;
    }
    return "";
  };

  // Función para combinar ubicación con detalles
  const buildCompleteLocation = (
    location: string,
    details: string
  ): string => {
    if (location && details) {
      return `${location} - ${details}`;
    }
    if (location) {
      return location;
    }
    if (details) {
      return details;
    }
    return "";
  };

  // Efecto para actualizar opciones de estado cuando cambia el país
  useEffect(() => {
    if (selectedCountry) {
      setStateOptions(getStateOptionsByCountry(selectedCountry));
    } else {
      setStateOptions([]);
    }
  }, [selectedCountry]);

  // Efecto para actualizar opciones de ciudad cuando cambia el estado
  useEffect(() => {
    if (selectedCountry && selectedState) {
      setCityOptions(
        getCityOptionsByCountryAndState(selectedCountry, selectedState)
      );
    } else {
      setCityOptions([]);
    }
  }, [selectedCountry, selectedState]);

  // Inicializar valores si estamos en modo edición
  useEffect(() => {
    if (!initialValues || !formRef.current) return;

    let details = "";
    let location = initialValues.location || "";

    if (location.includes(" - ")) {
      const parts = location.split(" - ");
      location = parts[0];
      details = parts.slice(1).join(" - ");
    }

    // Establecer detalles
    formRef.current.setFieldValue("details", details);

    // Intentar parsear la ubicación para los selectores
    if (!location) return;

    const locationParts = location.split(", ");

    if (locationParts.length >= 3) {
      const [city, state, countryName] = locationParts;

      const countryObj = countryOptions.find(
        (c) => c.label.toLowerCase() === countryName.toLowerCase()
      );

      if (countryObj) {
        setSelectedCountry(countryObj.value);
        setSelectedState(state);
        setSelectedCity(city);
      }
    } else if (locationParts.length === 2) {
      const [firstPart, secondPart] = locationParts;

      const countryObj = countryOptions.find(
        (c) => c.label.toLowerCase() === secondPart.toLowerCase()
      );

      if (countryObj) {
        setSelectedCountry(countryObj.value);
        setSelectedCity(firstPart);
      } else {
        setSelectedState(secondPart);
        setSelectedCity(firstPart);
      }
    } else if (locationParts.length === 1) {
      const [singlePart] = locationParts;

      const countryObj = countryOptions.find(
        (c) => c.label.toLowerCase() === singlePart.toLowerCase()
      );

      if (countryObj) {
        setSelectedCountry(countryObj.value);
      } else {
        setSelectedCity(singlePart);
      }
    }
  }, [initialValues, countryOptions]);

  const handleCountryChange = (countryShort?: string) => {
    if (!countryShort) {
      setSelectedCountry("");
      setSelectedState("");
      setSelectedCity("");
      formRef.current?.setFieldsValue({
        state: null,
        city: null,
        country: null,
      });
      return;
    }

    setSelectedCountry(countryShort);
    setSelectedState("");
    setSelectedCity("");
    formRef.current?.setFieldsValue({
      state: null,
      city: null,
      country: countryShort,
    });
  };

  const handleStateChange = (stateName?: string) => {
    if (!stateName) {
      setSelectedState("");
      setSelectedCity("");
      formRef.current?.setFieldValue("city", null);
      return;
    }

    setSelectedState(stateName);
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