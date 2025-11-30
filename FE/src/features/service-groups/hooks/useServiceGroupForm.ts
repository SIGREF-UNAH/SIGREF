import { useNavigate } from "react-router";

export function useServiceGroupForm() {
  const navigate = useNavigate();

  const handleCancel = () => {
    navigate("/service-groups/list");
  };

  return {
    handleCancel,
  };
}
