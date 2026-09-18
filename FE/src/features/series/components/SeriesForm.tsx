import { ProForm, ProFormText, ProFormDigit } from "@ant-design/pro-components";
import { Alert, Space, Divider } from "antd";
import type { CreateSeriesDto, SerieDto, UpdateSeriesDto } from "@models";

interface Props {
  mode: "create" | "edit";
  initialValues?: Partial<SerieDto>;
  onFinish: (values: CreateSeriesDto | UpdateSeriesDto) => void;
  loading?: boolean;
}

export const SeriesForm: React.FC<Props> = ({ mode, initialValues, onFinish, loading }) => {
  return (
    <ProForm
      initialValues={initialValues}
      onFinish={onFinish}
      loading={loading}
      submitter={{
        searchConfig: {
          submitText: mode === "create" ? "Crear Serie" : "Guardar Cambios",
          resetText: "Cancelar",
        },
        submitButtonProps: {
          type: "primary",
          size: "large",
        },
        resetButtonProps: {
          size: "large",
        },
      }}
      layout="vertical"
    >
      {mode === "create" && (
        <Alert
          message="Nueva Serie de Numeración"
          description="Complete la información para crear una nueva serie de numeración para sus documentos."
          type="info"
          showIcon
          style={{ marginBottom: 24 }}
        />
      )}

      <ProFormText
        name="name"
        label="Nombre de la Serie"
        placeholder="Ej: Serie A, Serie B etc."
        rules={[
          { required: true, message: "El nombre es obligatorio" },
          { min: 3, message: "El nombre debe tener al menos 3 caracteres" },
          { max: 100, message: "El nombre no puede exceder 100 caracteres" },
        ]}
        fieldProps={{
          size: "large",
          showCount: true,
          maxLength: 100,
        }}
        tooltip="Nombre descriptivo para identificar esta serie"
      />

      <ProFormText
        name="prefix"
        label="Prefijo"
        placeholder="Ej: SA-00001 SB-00001 etc."
        rules={[
          { required: true, message: "El prefijo es obligatorio" },
          { min: 1, message: "El prefijo debe tener al menos 1 carácter" },
          { max: 10, message: "El prefijo no puede exceder 10 caracteres" },
          {
            pattern: /^[A-Z0-9-]+$/,
            message: "Solo se permiten letras mayúsculas, números y guiones",
          },
        ]}
        fieldProps={{
          size: "large",
          showCount: true,
          maxLength: 10,
        }}
        tooltip="Prefijo que aparecerá antes del número (ej: SA-, SB-)"
        transform={(value) => value?.toUpperCase()}
      />

      <Divider>Rango de Numeración</Divider>

      <Space size="large" style={{ width: "100%", display: "flex" }}>
        <ProFormDigit
          name="startNumber"
          label="Número Inicial"
          placeholder="1"
          min={1}
          max={999999}
          rules={[
            { required: true, message: "El número inicial es obligatorio" },
          ]}
          fieldProps={{
            size: "large",
            precision: 0,
            style: { width: "100%" },
          }}
          tooltip="Primer número de la serie"
        />

        <ProFormDigit
          name="endNumber"
          label="Número Final"
          placeholder="9999"
          min={1}
          max={999999}
          rules={[
            { required: true, message: "El número final es obligatorio" },
            ({ getFieldValue }) => ({
              validator(_, value) {
                const startNumber = getFieldValue("startNumber");
                if (!value || !startNumber || value > startNumber) {
                  return Promise.resolve();
                }
                return Promise.reject(
                  new Error("El número final debe ser mayor que el inicial")
                );
              },
            }),
          ]}
          fieldProps={{
            size: "large",
            precision: 0,
            style: { width: "100%" },
          }}
          tooltip="Último número disponible en la serie"
        />
      </Space>

      {mode === "edit" && (
        <>
          <Divider>Información Actual</Divider>
          <ProFormDigit
            name="currentNumber"
            label="Número Actual"
            disabled
            fieldProps={{
              size: "large",
              precision: 0,
              readOnly: true,
            }}
            tooltip="Último número utilizado en esta serie (solo lectura)"
          />
          <Alert
            message="Nota"
            description="El número actual se incrementa automáticamente cada vez que se genera un nuevo documento con esta serie."
            type="warning"
            showIcon
            style={{ marginTop: 16, marginBottom: 24 }}
          />
        </>
      )}
    </ProForm>
  );
};

