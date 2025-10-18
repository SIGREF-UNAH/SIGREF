import React, { createContext, useContext, useMemo } from "react";
import { Ability } from "@casl/ability";
import { defineAbilitiesFor } from "../../auth";

const AbilityContext = createContext<Ability | null>(null);

export const AbilityProvider = ({
  roles,
  children,
}: {
  roles: string[];
  children: React.ReactNode;
}) => {
  const ability = useMemo(() => defineAbilitiesFor(roles), [roles]);
  return (
    <AbilityContext.Provider value={ability}>
      {children}
    </AbilityContext.Provider>
  );
};

export const useAbility = () => {
  const ctx = useContext(AbilityContext);
  if (!ctx)
    throw new Error("useAbility debe usarse dentro de <AbilityProvider>");
  return ctx;
};
