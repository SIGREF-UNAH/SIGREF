import { Navigate } from "react-router";
import type { JSX } from "react";
import type { Actions, Subjects } from "../../auth/abilities";
import { useAbility } from "../../config";

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
