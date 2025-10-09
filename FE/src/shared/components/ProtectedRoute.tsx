// src/components/ProtectedRoute.tsx
// import { Can } from "@casl/react";
import { Navigate } from "react-router";
import type { JSX } from "react";
import { useAbility } from "../../context/AbilityContext";

export const ProtectedRoute = ({
  action,
  subject,
  children,
}: {
  action: string;
  subject: string;
  children: JSX.Element;
}) => {
  const ability = useAbility();

  return ability.can(action, subject) ? children : <Navigate to="/" />;
};
