import { Navigate } from "react-router";
import type { JSX } from "react";
import { useAbility } from "../../config/providers";
import type { Actions, Subjects } from "../../auth/abilities";

export const ProtectedRoute = ({
  action,
  subject,
  children,
}: {
  action: Actions;
  subject: Subjects;
  children: JSX.Element;
}) => {
  const ability = useAbility();

  return ability.can(action, subject)
    ? children
    : <Navigate to="/" replace />;
};
