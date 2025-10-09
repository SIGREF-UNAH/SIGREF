import type { Address, Telecom } from "./fhir";

export interface Location {
  id: string;
  identifier?: string;
  name: string;
  description?: string;
  status: string;
  address?: Address;
  telecom?: Telecom[];
  type?: string;
  lastUpdated?: string;
}
