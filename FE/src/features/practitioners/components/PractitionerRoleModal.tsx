import { ModalForm, ProFormText, ProFormSelect, type ProFormInstance, ProFormDatePicker } from "@ant-design/pro-components";
import { useEffect } from "react";
import dayjs from "dayjs";

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
  }, [initialValues, formRef]);

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
        rules={[{ required: true, message: "Seleccione la organización" }]}
      />

      <ProFormSelect
        name="locationId"
        label="Ubicación"
        placeholder="Seleccionar"
        allowClear
        options={locationOptions}
        rules={[{ required: true, message: "Seleccione la ubicación" }]}
      />

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-4">
        <ProFormDatePicker
          name="startDate"
          label="Fecha de inicio *"
          placeholder="Seleccione fecha de inicio"
          fieldProps={{
            format: "DD/MM/YYYY",
            className: "w-full",
          }}
          rules={[
            { required: true, message: "La fecha de inicio es obligatoria" },
          ]}
        />

        <ProFormDatePicker
          name="endDate"
          label="Fecha de fin (opcional)"
          placeholder="Dejar vacío si es permanente"
          fieldProps={{
            format: "DD/MM/YYYY",
            className: "w-full",
          }}
          rules={[
            { required: false },
            // Validación para que la fecha de fin no sea anterior a la de inicio
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value) return Promise.resolve();
                const start = getFieldValue("startDate");
                if (start && dayjs(value).isBefore(dayjs(start), "day")) {
                  return Promise.reject(
                    new Error("La fecha de fin debe ser posterior o igual a la de inicio")
                  );
                }
                return Promise.resolve();
              },
            }),
          ]}
        />
      </div>
    </ModalForm>
  );
}