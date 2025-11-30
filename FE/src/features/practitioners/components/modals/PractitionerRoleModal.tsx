import { ModalForm, ProFormText, ProFormSelect, type ProFormInstance, ProFormDateRangePicker } from "@ant-design/pro-components";
import { useEffect } from "react";

type PractitionerRoleModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (values: any) => Promise<boolean>;
  roleOptions: { label: string; value: string }[];
  orgOptions: { label: string; value: string }[];
  locationOptions: { label: string; value: string }[];
  initialValues?: any;
  formRef?: React.RefObject<ProFormInstance | null>;
  title?: string;
};

export default function PractitionerRoleModal({
  open,
  onOpenChange,
  onSubmit,
  roleOptions,
  orgOptions,
  locationOptions,
  initialValues,
  formRef,
  title,
}: PractitionerRoleModalProps) {
  // Cuando cambian los initialValues, actualizar el formulario
  useEffect(() => {
    if (formRef?.current && initialValues) {
      formRef.current.setFieldsValue(initialValues);
    }
  }, [initialValues]);

  return (
    <ModalForm
      title={title}
      open={open}
      onOpenChange={onOpenChange}
      formRef={formRef}
      modalProps={{ destroyOnClose: true }}
      initialValues={initialValues}
      onFinish={onSubmit}
    >
      <ProFormText
        name="roleName"
        label="Nombre del Rol"
        placeholder="Ej: Médico General"
        rules={[{ required: true, message: "Este campo es obligatorio" }]}
      />

      <ProFormSelect
        name="role"
        label="Rol"
        options={roleOptions}
        rules={[{ required: true, message: "Seleccione un rol" }]}
      />

      <ProFormSelect
        name="organizationId"
        label="Organización"
        allowClear
        options={orgOptions}
      />

      <ProFormSelect
        name="locationId"
        label="Ubicación"
        allowClear
        options={locationOptions}
      />

      <ProFormDateRangePicker
        name="period"
        label="Periodo (Inicio - Fin)"
        rules={[{ required: true, message: "Seleccione un rango de fechas" }]}
      />
    </ModalForm>
  );
}
