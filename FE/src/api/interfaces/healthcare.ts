import type { CodeableConcept, Identifier, Reference } from "./fhir";

export interface Healthcare {
  id: string;
  identifier?: Identifier[];
  active: boolean;
  name: string;
  comment?: string;
  abbreviation: string;
  cost: number | 0;
  specialty?: CodeableConcept[];
  providedBy?: Reference;
  location?: Reference[];
  lastUpdated?: string;
}
