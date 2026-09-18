import { useParams, useNavigate } from "react-router-dom";
import { useGetOrganizationList } from "@endpoints/organizations/organizations";
import { useGetLocationList } from "@endpoints/locations/locations";
import type { ProFormInstance } from "@ant-design/pro-components";
import { ROLE_OPTIONS } from "../../../shared/constants";
import { useState, useRef, useEffect } from "react";
import { useMessage } from '../../../shared/hooks';
import { useAbility } from "../../../config";
import dayjs from "dayjs";
import {
  useDeletePractitionerById,
  useGetPractitionerById,
} from "@endpoints/practitioners/practitioners";
import {
  useDeletePractitionerRoleById,
  useGetPractitionerRoleByPractitionerId,
  useCreatePractitionerRole,
  useUpdatePractitionerRoleById,
} from "@endpoints/practitioner-roles/practitioner-roles";
import type {
  PractitionerDto,
  PractitionerRoleDto,
  OrganizationDto,
  LocationDto,
} from "@models";

/**
 * Hook personalizado para gestionar la información de un profesional de salud
 * 
 * Utiliza la terminología estándar HL7 FHIR para profesionales y roles
 * @see https://www.hl7.org/fhir/practitioner.html
 * @see https://www.hl7.org/fhir/practitionerrole.html
 */
export default function usePractitionerInfo() {
  const navigate = useNavigate();
  const ability = useAbility();
  const message = useMessage();
  const formRef = useRef<ProFormInstance>(null);
  const { id } = useParams<{ id: string }>();
  const [roleModalOpen, setRoleModalOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<PractitionerRoleDto | null>(null);

  // Obtener profesional por ID
  const {
    data: practitioner,
    isLoading: practitionerLoading,
    isError: practitionerError,
  } = useGetPractitionerById<PractitionerDto>(id!, {
    query: {
      enabled: !!id,
    },
  });

  // Obtener roles del profesional
  const {
    data: practitionerRoles,
    isLoading: rolesLoading,
    refetch: refetchRoles,
  } = useGetPractitionerRoleByPractitionerId<PractitionerRoleDto[]>(id!, {
    query: {
      enabled: !!id,
    },
  });

  // Obtener organizaciones
  const { data: orgsData } = useGetOrganizationList<{
    items: OrganizationDto[];
  }>();

  // Obtener ubicaciones
  const { data: locationsData } = useGetLocationList<{
    items: LocationDto[];
  }>();

  // Mutación para crear rol
  const createRoleMutation = useCreatePractitionerRole({
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

  // Mutación para actualizar rol
  const updateRoleMutation = useUpdatePractitionerRoleById({
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

  // Mutación para eliminar rol
  const deleteRoleMutation = useDeletePractitionerRoleById({
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

  // Mutación para eliminar profesional
  const deletePractitionerMutation = useDeletePractitionerById({
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

  // Reset form cuando el modal se abre/cierra
  useEffect(() => {
    if (!roleModalOpen) {
      setEditingRole(null);
      formRef.current?.resetFields();
    } else if (editingRole && formRef.current) {
      const values = getRoleFormValues(editingRole);
      formRef.current.setFieldsValue(values);
    }
  }, [roleModalOpen, editingRole]);

  /**
   * Obtiene los valores del formulario desde un rol existente
   */
  const getRoleFormValues = (role: PractitionerRoleDto) => {
    return {
      roleName: role.code?.[0]?.text || "",
      role: role.code?.[0]?.coding?.[0]?.code || "",
      organizationId:
        role.organization?.reference?.replace("Organization/", "") || undefined,
      locationId:
        role.location?.[0]?.reference?.replace("Location/", "") || undefined,
      startDate: role.period?.start ? dayjs(role.period.start) : null,
      endDate: role.period?.end ? dayjs(role.period.end) : null,
    };
  };

  /**
   * Formatea una fecha para mostrar en la UI
   */
  const formatDate = (dateString?: string | null) => {
    if (!dateString) return "-";
    return dayjs(dateString).format("DD/MM/YYYY");
  };

  /**
 * Obtiene la etiqueta del género según código FHIR
 * @see https://www.hl7.org/fhir/valueset-administrative-gender.html
 */
const getGenderLabel = (gender?: string | null) => {
  const genderMap: Record<string, string> = {
    "male": "Masculino",
    "female": "Femenino",
    "other": "Otro",
    "unknown": "Desconocido",
  };
  
  // Verificar que gender sea un string antes de llamar a toLowerCase
  if (!gender || typeof gender !== 'string') return "N/A";
  
  return genderMap[gender.toLowerCase()] || gender;
};

  /**
   * Maneja el envío del formulario de rol
   */
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
        start: values.startDate
          ? dayjs(values.startDate).format("YYYY-MM-DD")
          : null,
        end: values.endDate
          ? dayjs(values.endDate).format("YYYY-MM-DD")
          : null,
      },
      practitioner: { reference: `Practitioner/${id}` },
      code: [
        {
          coding: [
            {
              system: "https://hospitalpublico.hn/fhir/CodeSystem/roles-admin",
              code: selectedRole?.value || values.role,
              display: selectedRole?.label || values.roleName,
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
      console.error("Error al enviar rol:", error);
      return false;
    }
  };

  /**
   * Navega a la página de edición del profesional
   */
  const handleEdit = () => {
    if (id) navigate(`/practitioners/update/${id}`);
  };

  /**
   * Elimina un rol del profesional
   */
  const handleDeleteRole = async (roleId: string) => {
    try {
      await deleteRoleMutation.mutateAsync({ id: roleId });
    } catch (error) {
      console.error("Error deleting role:", error);
    }
  };

  /**
   * Elimina al profesional
   */
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