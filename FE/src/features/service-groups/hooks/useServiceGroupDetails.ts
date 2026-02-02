import { useNavigate, useParams } from "react-router";
import { useAbility } from "../../../config";
import { useGetApiServiceGroupId } from "../../../api/service-group/service-group";

export function useServiceGroupDetails() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const ability = useAbility();

  const {
    data: serviceGroup,
    isLoading,
    error,
  } = useGetApiServiceGroupId(id || "", {
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
