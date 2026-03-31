import ccsj from "countrycitystatejson";
import { HONDURAS_MUNICIPALITIES } from "../constants/hondurasMunicipalities";

const HONDURAS_COUNTRY_CODE = "HN";

const normalizeName = (value: string) =>
  value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/\s+/g, " ")
    .trim()
    .toLowerCase();

const hondurasCitiesByState = new Map<string, string[]>(
  Object.entries(HONDURAS_MUNICIPALITIES).map(([state, cities]) => [
    normalizeName(state),
    cities,
  ])
);

const getHondurasCities = (stateName: string) =>
  hondurasCitiesByState.get(normalizeName(stateName)) ?? [];

export const getCountryOptions = () =>
  ccsj.getCountries().map((country: any) => ({
    label: country.name,
    value: country.shortName,
  }));

export const getStateOptionsByCountry = (countryShort?: string) => {
  if (!countryShort) return [];

  if (countryShort === HONDURAS_COUNTRY_CODE) {
    return Object.keys(HONDURAS_MUNICIPALITIES)
      .sort((a, b) => a.localeCompare(b))
      .map((stateName) => ({ label: stateName, value: stateName }));
  }

  return (ccsj.getStatesByShort(countryShort) ?? []).map((stateName) => ({
    label: stateName,
    value: stateName,
  }));
};

export const getCityOptionsByCountryAndState = (
  countryShort?: string,
  stateName?: string
) => {
  if (!countryShort || !stateName) return [];

  if (countryShort !== HONDURAS_COUNTRY_CODE) {
    const sourceCities = ccsj.getCities(countryShort, stateName) ?? [];
    return sourceCities.map((cityName) => ({
      label: cityName,
      value: cityName,
    }));
  }

  const hondurasCities = getHondurasCities(stateName);
  const sorted = [...hondurasCities].sort((a, b) => a.localeCompare(b, "es"));
  return sorted.map((cityName) => ({
    label: cityName,
    value: cityName,
  }));
};
