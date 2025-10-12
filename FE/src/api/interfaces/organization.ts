import type { Contact, Identifier, Reference } from "./fhir";

export interface Organization {
  id: string;
  identifiers?: Identifier[];
  active: boolean;
  types?: string[];
  name: string;
  alias?: string[];
  description?: string;
  contact?: Contact[];
  partOf?: Reference;
  endpoint?: Reference[];
  lastUpdated?: string;
}
