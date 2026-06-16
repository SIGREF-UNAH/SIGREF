import { ProFormText, ProFormSelect, ProFormDatePicker } from "@ant-design/pro-components";
import { Spin } from "antd";
import type { FormInstance } from 'antd';
import type { RuleObject } from 'antd/es/form'; 
import dayjs from "dayjs";
import { usePractitionerRoleOptions } from "../hooks";
import { useEffect } from "react";

type SelectOption = { label: string; value: string };

type PractitionerRoleFormProps = {
  orgOptions: SelectOption[];
  locationOptions: SelectOption[];
};

export default function PractitionerRoleForm({
  orgOptions,
  locationOptions,
}: PractitionerRoleFormProps) {
  const { options: roleOptions, loading, hasMore, loadMore, reset } = usePractitionerRoleOptions();

  // Cargar primera página al montar
  useEffect(() => {
    reset();
    loadMore();
  }, []);

  const handleScroll = (e: React.UIEvent<HTMLDivElement>) => {
    const { scrollTop, scrollHeight, clientHeight } = e.currentTarget;
    const nearBottom = scrollHeight - scrollTop - clientHeight < 50;
    if (nearBottom && hasMore && !loading) {
      loadMore();
    }
  };

  return (
    <>
      <ProFormText
        name="roleName"
        label="Título"
        placeholder="Ej. Médico General"
        rules={[{ required: true, message: "Campo obligatorio" }]}
      />

      <ProFormSelect
        name="role"
        label="Tipo"
        placeholder="Seleccionar"
        rules={[{ required: true, message: "Seleccione el tipo de cargo" }]}
        // Usamos fieldProps para inyectar scroll infinito en el dropdown
        fieldProps={{
          options: roleOptions,
          loading,
          notFoundContent: loading ? <Spin size="small" /> : "Sin resultados",
          onPopupScroll: handleScroll,
          filterOption: false,
          showSearch: false,
        }}
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
          placeholder="Seleccione fecha"
          fieldProps={{ format: "DD/MM/YYYY", className: "w-full" }}
          rules={[{ required: true, message: "Fecha de inicio obligatoria" }]}
        />

        <ProFormDatePicker
          name="endDate"
          label="Fecha de fin (opcional)"
          placeholder="Dejar vacío si es permanente"
          fieldProps={{ format: "DD/MM/YYYY", className: "w-full" }}
          rules={[
            { required: false },
            ({ getFieldValue }: FormInstance) => ({
              // Tipamos '_' como RuleObject y 'value' como una instancia de Dayjs (o null)
              validator(_: RuleObject, value: dayjs.Dayjs | null) {
                if (!value) return Promise.resolve();
                
                const start = getFieldValue("startDate");
                // Como 'value' ya es tratado como un objeto Dayjs válido por TypeScript,
                // puede usar sus métodos directamente sin envolverlo de nuevo en dayjs()
                if (start && value.isBefore(dayjs(start), "day")) {
                  return Promise.reject(
                    new Error("Fecha fin debe ser igual o posterior a inicio")
                  );
                }
                return Promise.resolve();
              },
            }),
          ]}
        />
      </div>
    </>
  );
}