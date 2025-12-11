import { useParams, useNavigate } from "react-router-dom";
import { useGetApiOrganizations } from "../../../api/organizations/organizations";
import { useGetApiLocations } from "../../../api/locations/locations";
import type { ProFormInstance } from "@ant-design/pro-components";
import { ROLE_OPTIONS } from "../../../shared/constants";
import { useState, useRef, useEffect } from "react";
import { useMessage } from '../../../shared/hooks';
import { useAbility } from "../../../config";
import dayjs from "dayjs";
import {
  useDeleteApiPractitionerId,
  useGetApiPractitionerId,
} from "../../../api/practitioner/practitioner";
import {
  useDeleteApiPractitionerRoleId,
  useGetApiPractitionerRolePractitionerId,
  usePostApiPractitionerRole,
  usePutApiPractitionerRoleId,
} from "../../../api/practitioner-role/practitioner-role";
import type {
  PractitionerDto,
  PractitionerRoleDto,
  OrganizationDto,
  LocationDto,
} from "../../../api/models";

export default function usePractitionerInfo() {
  const navigate = useNavigate();
  const ability = useAbility();
  const message = useMessage();
  const formRef = useRef<ProFormInstance>(null);
  const { id } = useParams<{ id: string }>();
  const [roleModalOpen, setRoleModalOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<PractitionerRoleDto | null>(null);

  // Fetch data
  const {
    data: practitioner,
    isLoading: practitionerLoading,
    isError: practitionerError,
  } = useGetApiPractitionerId<PractitionerDto>(id!, {
    query: {
      enabled: !!id,
    },
  });

  const {
    data: practitionerRoles,
    isLoading: rolesLoading,
    refetch: refetchRoles,
  } = useGetApiPractitionerRolePractitionerId<PractitionerRoleDto[]>(id!, {
    query: {
      enabled: !!id,
    },
  });

  const { data: orgsData } = useGetApiOrganizations<{
    items: OrganizationDto[];
  }>();
  const { data: locationsData } = useGetApiLocations<{
    items: LocationDto[];
  }>();

  // Mutations
  const createRoleMutation = usePostApiPractitionerRole({
    mutation: {
      onSuccess: () => {
        message.success("Cargo asignado correctamente");
        refetchRoles();
        setRoleModalOpen(false);
        setEditingRole(null);
      },
      onError: (error: any) => {
        console.error("Error al asignar cargo:", error);
        message.error(
          error?.response?.data?.title || "Error al asignar el cargo"
        );
      },
    },
  });

  const updateRoleMutation = usePutApiPractitionerRoleId({
    mutation: {
      onSuccess: () => {
        message.success("Cargo actualizado correctamente");
        refetchRoles();
        setRoleModalOpen(false);
        setEditingRole(null);
      },
      onError: (error: any) => {
        console.error("Error al actualizar cargo:", error);
        message.error(
          error?.response?.data?.title || "Error al actualizar el cargo"
        );
      },
    },
  });

  const deleteRoleMutation = useDeleteApiPractitionerRoleId({
    mutation: {
      onSuccess: () => {
        message.success("Cargo eliminado correctamente");
        refetchRoles();
      },
      onError: (error: any) => {
        console.error("Error al eliminar cargo:", error);
        message.error("Error al eliminar el cargo");
      },
    },
  });

  const deletePractitionerMutation = useDeleteApiPractitionerId({
    mutation: {
      onSuccess: () => {
        message.success("Empleado eliminado correctamente");
        navigate("/practitioners/list");
      },
      onError: (error: any) => {
        console.error("Error al eliminar empleado:", error);
        message.error("Error al eliminar el empleado");
      },
    },
  });

  // Reset form when modal opens/closes
  useEffect(() => {
    if (!roleModalOpen) {
      setEditingRole(null);
      formRef.current?.resetFields();
    } else if (editingRole && formRef.current) {
      const values = getRoleFormValues(editingRole);
      formRef.current.setFieldsValue(values);
    }
  }, [roleModalOpen, editingRole]);

  // Helper functions
  const getRoleFormValues = (role: PractitionerRoleDto) => {
    return {
      roleName: role.code?.[0]?.text || "",
      role: role.code?.[0]?.coding?.[0]?.code || "",
      organizationId:
        role.organization?.reference?.replace("Organization/", "") || undefined,
      locationId:
        role.location?.[0]?.reference?.replace("Location/", "") || undefined,
      period: [
        role.period?.start ? dayjs(role.period.start) : null,
        role.period?.end ? dayjs(role.period.end) : null,
      ],
    };
  };

  const formatDate = (dateString?: string | null) => {
    if (!dateString) return "-";
    return dayjs(dateString).format("DD/MM/YYYY");
  };

  const getGenderLabel = (gender?: number | string | null) => {
    const genderMap: Record<number, string> = {
      0: "Desconocido",
      1: "Masculino",
      2: "Femenino",
      3: "Otro",
    };
    return genderMap[Number(gender)] || "N/A";
  };

  // Handle role submission
  const handleRoleSubmit = async (values: any) => {
    if (!id) return false;

    const selectedRole = ROLE_OPTIONS.find((r) => r.value === values.role);

    const payload: any = {
      identifier: editingRole
        ? editingRole.identifier
        : [
            {
              use: "usual",
              system:
                "https://hospitalpublico.hn/fhir/identifier/practitionerrole",
              value: `role-${id}-${Date.now()}`,
            },
          ],
      period: {
        start: values.period?.[0]
          ? dayjs(values.period[0]).format("YYYY-MM-DD")
          : null,
        end: values.period?.[1]
          ? dayjs(values.period[1]).format("YYYY-MM-DD")
          : null,
      },
      practitioner: { reference: `Practitioner/${id}` },
      code: [
        {
          coding: [
            {
              system: "https://hospitalpublico.hn/fhir/CodeSystem/roles-admin",
              code: selectedRole?.value || values.role,
              display: selectedRole?.label || values.role,
            },
          ],
          text: values.roleName,
        },
      ],
      active: true,
    };

    if (values.organizationId) {
      const org = orgsData?.items?.find((o) => o.id === values.organizationId);
      payload.organization = {
        reference: `Organization/${values.organizationId}`,
        display: org?.name || "",
      };
    }

    if (values.locationId) {
      const location = locationsData?.items?.find(
        (l) => l.id === values.locationId
      );
      payload.location = [
        {
          reference: `Location/${values.locationId}`,
          display: location?.name || "",
        },
      ];
    }
    try {
      if (editingRole) {
        await updateRoleMutation.mutateAsync({
          id: editingRole.id!,
          data: payload,
        });
      } else {
        await createRoleMutation.mutateAsync({ data: payload });
      }
      return true;
    } catch (error) {
      return false;
    }
  };

  // Handle edit practitioner
  const handleEdit = () => {
    if (id) navigate(`/practitioners/update/${id}`);
  };

  // Handle delete role
  const handleDeleteRole = async (roleId: string) => {
    try {
      await deleteRoleMutation.mutateAsync({ id: roleId });
    } catch (error) {
      console.error("Error deleting role:", error);
    }
  };

  // Handle delete practitioner
  const handleDeletePractitioner = async () => {
    if (!id) return;
    try {
      await deletePractitionerMutation.mutateAsync({ id });
    } catch (error) {
      console.error("Error deleting practitioner:", error);
    }
  };

  return {
    practitioner,
    practitionerLoading,
    practitionerError,
    rolesLoading,
    practitionerRoles,
    roleModalOpen,
    editingRole,  
    formRef,
    ability,
    orgsData,
    locationsData,
    formatDate,
    getGenderLabel,
    getRoleFormValues,
    handleRoleSubmit,
    handleEdit,
    handleDeleteRole,
    handleDeletePractitioner,
    setRoleModalOpen,
    setEditingRole,
  };
}
