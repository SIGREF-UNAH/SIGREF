export interface LocationFormData {
  identifier: string;
  locationName: string;
  alias: string;
  state: string;
  mode: string;
  functionType: string;
  description: string;
  address: string;
  city: string;
  stateProvince: string;
  postalCode: string;
  country: string;
  contactName: string;
  phone: string;
  email: string;
  responsibleOrganization: string;
  parentLocation: string;
}

export const emptyLocationFormData: LocationFormData = {
  identifier: "",
  locationName: "",
  alias: "",
  state: "",
  mode: "",
  functionType: "",
  description: "",
  address: "",
  city: "",
  stateProvince: "",
  postalCode: "",
  country: "",
  contactName: "",
  phone: "",
  email: "",
  responsibleOrganization: "",
  parentLocation: "",
};
