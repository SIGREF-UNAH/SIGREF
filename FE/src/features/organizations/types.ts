export type OrganizationTypeEnum =
  | "provider" | "department" | "team" | "government" | "insurer" | "payer"
  | "educational" | "regligious" | "clinicalResearchSponsor" | "communityGroup"
  | "nonHealthcareBusiness" | "network";

export const OrganizationTypeEnum = {
  provider: "provider", department: "department", team: "team", government: "government",
  insurer: "insurer", payer: "payer", educational: "educational", regligious: "regligious",
  clinicalResearchSponsor: "clinicalResearchSponsor", communityGroup: "communityGroup",
  nonHealthcareBusiness: "nonHealthcareBusiness", network: "network",
} as const satisfies Record<OrganizationTypeEnum, OrganizationTypeEnum>;
