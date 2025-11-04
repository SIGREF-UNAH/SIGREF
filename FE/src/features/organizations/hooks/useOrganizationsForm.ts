import { useNavigate } from "react-router";
import type { CreateOrganizationDto } from "../../../api/models";
import { useQueryClient } from "@tanstack/react-query";
import { getGetApiOrganizationsQueryKey, usePostApiOrganizations } from "../../../api/organizations/organizations";
import { useMessage } from "../../../shared/hooks";

export function useCreateOrganization() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  const { mutateAsync: createOrganization, isPending } = usePostApiOrganizations({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiOrganizationsQueryKey() });
        msg.success("Organización creada correctamente");
        navigate("/organizations");
      },
      onError: (error: any) => {
        console.error("Error al crear la organización:", error);
        msg.error(error?.response?.data?.message || "Error al crear la organización");
      },
    },
  });

  const handleFinish = async (values: CreateOrganizationDto) => {
    try {
      await createOrganization({ data: values });
    } catch (error) {
      console.error("Error en handleFinish:", error);
    }
  };

  return {
    handleFinish,
    isPending,
  };
}
