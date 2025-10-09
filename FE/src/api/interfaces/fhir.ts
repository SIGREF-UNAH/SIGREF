export interface Identifier {
  use?: number;
  type?: string;
  system?: string;
  value?: string;
}

export interface CodeableConcept {
  coding?: Coding[];
  text?: string;
}

export interface Coding {
  system?: string;
  version?: string;
  code?: string;
  display?: string;
  userSelected: boolean | true;
}

export interface Reference {
  type?: string;
  identifier?: Identifier;
  reference?: string;
  display?: string;
}

export interface Address {
  use?: number;
  type?: number;
  text?: string;
  line?: string[];
  city?: string;
  district?: string;
  state?: string;
  postalCode?: string;
  country?: string;
}

export interface Contact {
  purpose?: CodeableConcept;
  name?: string;
  telecom?: Telecom[];
  address?: Address;
  organization?: Reference;
  period?: Period;
}

export interface Telecom {
  system?: number;
  value?: string;
  use?: number;
  rank?: number;
}

export interface Period {
  start: string;
  end: string;
}
