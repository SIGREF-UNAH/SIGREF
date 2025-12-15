import { ModalForm, ProFormText, ProFormSelect, type ProFormInstance, ProFormDateRangePicker } from "@ant-design/pro-components";
import { useEffect } from "react";

type PractitionerRoleModalProps = {
  title?: string;
  open: boolean;
  roleOptions: { label: string; value: string }[];
  orgOptions: { label: string; value: string }[];
  locationOptions: { label: string; value: string }[];
  initialValues?: any;
  formRef?: React.RefObject<ProFormInstance | null>;
  onOpenChange: (open: boolean) => void;
  onSubmit: (values: any) => Promise<boolean>;
};

export default function PractitionerRoleModal({
  title,
  open,
  roleOptions,
  orgOptions,
  locationOptions,
  initialValues,
  formRef,
  onOpenChange,
  onSubmit,
}: PractitionerRoleModalProps) {
  
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
        label="Titulo"
        placeholder="Ej. Médico General"
        rules={[{ required: true, message: "Este campo es obligatorio" }]}
      />

      <ProFormSelect
        name="role"
        label="Tipo"
        placeholder="Seleccionar"
        options={roleOptions}
        rules={[{ required: true, message: "Seleccione el tipo de cargo" }]}
      />

      <ProFormSelect
        name="organizationId"
        label="Organización"
        placeholder="Seleccionar"
        allowClear
        options={orgOptions}
      />

      <ProFormSelect
        name="locationId"
        label="Ubicación"
        placeholder="Seleccionar"
        allowClear
        options={locationOptions}
      />

      <ProFormDateRangePicker
        name="period"
        label="Periodo (Inicio - Fin)"
        placeholder="Ej. 1999-12-31"
        allowClear
        rules={[{ required: false, message: "Seleccione un rango de fechas" }]}
      />
    </ModalForm>
  );
}
