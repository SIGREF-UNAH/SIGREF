import { useNavigate, useParams } from "react-router";
import { useAbility } from "../../../config";
import { useGetServiceGroupById } from "@endpoints/service-groups/service-groups";

export function useServiceGroupDetails() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const ability = useAbility();

  const {
    data: serviceGroup,
    isLoading,
    error,
  } = useGetServiceGroupById(id || "", {
    query: {
      enabled: !!id,
    },
  });

  return {
    data: serviceGroup,
    isLoading,
    error,
    ability,
    navigate,
  };
}
