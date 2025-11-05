import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormTextArea,
} from "@ant-design/pro-components";
import { Button } from "antd";
import {
  BarChartOutlined,
  EnvironmentOutlined,
  CheckOutlined,
} from "@ant-design/icons";
import { useCreateOrganization } from "../../hooks";

export default function NuevaOrganizacionPage() {
  const { handleFinish, isPending } = useCreateOrganization();

  return (
    <div className="min-h-screen w-full bg-gray-50">

      {/* Contenedor del formulario */}
      <main className="w-full px-8 ">
        <div className="w-full bg-white border border-gray-200 rounded-lg shadow-sm p-8">
          <ProForm
            onFinish={handleFinish}
            submitter={{
              render: () => (
                <div className="flex justify-end mt-6">
                  <Button
                    type="primary"
                    htmlType="submit"
                    icon={<CheckOutlined />}
                    size="large"
                    loading={isPending}  
                    className="!bg-green-600 !hover:bg-green-700 border-none px-8"
                  >
                    Crear Organización
                  </Button>
                </div>
              ),
            }}
          >
            {/* Información de la Organización */}
            <div className="mb-8">
              <div className="flex items-center gap-2 mb-6 pb-3 border-b border-gray-200">
                <BarChartOutlined className="text-lg !text-blue-400" />
                <h2 className="text-lg font-semibold text-gray-900">
                  Información de la Organización
                </h2>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <ProFormText
                  name="name"
                  label="Nombre de la Organización"
                  placeholder="Ej. Hospital Regional Norte"
                  rules={[{ required: true, message: "Este campo es requerido" }]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
                <ProFormText
                  name="identifier"
                  label="Identificador"
                  placeholder="# Ej. HSJ-HRN"
                  rules={[{ required: true, message: "Este campo es requerido" }]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
                <ProFormSelect
                  name="type"
                  label="Tipo de Organización"
                  placeholder="Seleccionar tipo"
                  options={[
                    { label: "Hospital", value: "hospital" },
                    { label: "Clínica", value: "clinica" },
                    { label: "Centro de Salud", value: "centro_salud" },
                    { label: "Laboratorio", value: "laboratorio" },
                  ]}
                  rules={[{ required: true, message: "Este campo es requerido" }]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
                <ProFormSelect
                  name="active"
                  label="Estado inicial"
                  placeholder="Activo"
                  initialValue={true}
                  options={[
                    { label: "Activo", value: true },
                    { label: "Inactivo", value: false },
                  ]}
                  rules={[{ required: true, message: "Este campo es requerido" }]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
              </div>
            </div>

            {/* Información de Contacto */}
            <div className="mb-8">
              <h3 className="text-base font-semibold text-gray-900 mb-4 pb-3 border-b border-gray-200">
                Información de Contacto
              </h3>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ProFormText
                  name="email"
                  label="Email de Contacto"
                  placeholder="contacto@gmail.com"
                  rules={[
                    { required: true, message: "Este campo es requerido" },
                    { type: "email", message: "Ingrese un email válido" },
                  ]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
                <ProFormText
                  name="phone"
                  label="Teléfono"
                  placeholder="+504 8819-4589"
                  rules={[{ required: true, message: "Este campo es requerido" }]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
              </div>
            </div>

            {/* Ubicación */}
            <div className="mb-8">
              <div className="flex items-center gap-2 mb-6 pb-3 border-b border-gray-200">
                <EnvironmentOutlined className="text-lg !text-blue-400" />
                <h2 className="text-lg font-semibold text-gray-900">
                  Ubicación
                </h2>
              </div>

              <div className="mb-4">
                <ProFormText
                  name="address"
                  label="Dirección Completa"
                  placeholder="Ej. Av. Principal 123, Ciudad"
                  rules={[{ required: true, message: "Este campo es requerido" }]}
                  fieldProps={{
                    prefix: <EnvironmentOutlined className="text-gray-400" />,
                    className: "bg-gray-100",
                  }}
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <ProFormText
                  name="city"
                  label="Ciudad"
                  placeholder="Ciudad"
                  rules={[{ required: true, message: "Este campo es requerido" }]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
                <ProFormText
                  name="state"
                  label="Departamento"
                  placeholder="Departamento"
                  rules={[{ required: true, message: "Este campo es requerido" }]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
                <ProFormText
                  name="postalCode"
                  label="Código Postal"
                  placeholder="12345"
                  rules={[{ required: true, message: "Este campo es requerido" }]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
              </div>
            </div>

            {/* Descripción y API Link */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <ProFormTextArea
                name="description"
                label="Descripción Opcional"
                placeholder="Breve descripción de la organización, sus funciones principales..."
                fieldProps={{ rows: 4, className: "bg-gray-100" }}
              />
              <ProFormTextArea
                name="apiLink"
                label="API Link"
                placeholder="Ej. https://api.organizacion.com"
                fieldProps={{ rows: 4, className: "bg-gray-100" }}
              />
            </div>
          </ProForm>
        </div>
      </main>
    </div>
  );
}
