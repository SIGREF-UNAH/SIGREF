import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormTextArea,
} from "@ant-design/pro-components";
import { BiChevronDown } from "react-icons/bi";
import { BsBuilding, BsPersonFill, BsGeoAltFill } from "react-icons/bs";
import { FaCheck } from "react-icons/fa";
import { OrganizationTypeEnum } from "../../../../api/models";
import { useCreateOrganization } from "../../hooks";
export default function OrganizationForm() {
  const { handleFinish, isPending } = useCreateOrganization();
  return (
    <div className="bg-white rounded-lg border border-gray-200 p-8 shadow-sm">
      {/* Header */}
      <div className="flex items-center gap-3 mb-8">
        <BsBuilding className="w-10 h-10 text-blue-500" />
        <span className="text-2xl font-semibold text-gray-800">
          {"Crear Organización"}
        </span>
      </div>
      {/* Boton de crear organizacion */}
      <ProForm
        onFinish={handleFinish}
        submitter={{
          searchConfig: { submitText: "Crear Organización" },
          submitButtonProps: {
            loading: isPending,
            icon: <FaCheck className="w-4 h-4" />,
            className:
              "px-6 py-2 !bg-green-500 hover:!bg-green-600 text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2",
          },
          render: (_, doms) => {
            return <div className="flex justify-end pt-6">{doms}</div>; // solo el botón submit
          },
        }}
      >
        {/* Información de la Organización */}
        <section className="mb-8">
          <div className="flex items-center gap-3 mb-6 pb-3 border-b border-gray-200">
            <BsBuilding className="w-6 h-6 text-blue-500" />
            <span className="text-lg font-semibold text-gray-700">
              Información General
            </span>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-4">
            <ProFormText
              name="name"
              label="Nombre de la Organización"
              placeholder="Ej. Hospital Regional Norte"
              rules={[
                { required: true, message: "El nombre es obligatorio" },
                { max: 200, message: "No puede exceder 200 caracteres" },
              ]}
              required
            />
            <ProFormText
              name="identifier"
              label="Identificador"
              placeholder="Ej. HS-HG-FHIR1"
              rules={[
                { required: true, message: "El identificador es obligatorio" },
              ]}
              required
            />
            <ProFormSelect
              name="type"
              label="Tipo de Organización"
              placeholder="Seleccionar tipo..."
              rules={[{ required: true, message: "El tipo es obligatorio" }]}
              options={[
                { label: "Proveedor", value: OrganizationTypeEnum.Provider },
                {
                  label: "Departamento",
                  value: OrganizationTypeEnum.Department,
                },
                { label: "Equipo", value: OrganizationTypeEnum.Team },
                { label: "Gobierno", value: OrganizationTypeEnum.Government },
                { label: "Aseguradora", value: OrganizationTypeEnum.Insurer },
                { label: "Pagador", value: OrganizationTypeEnum.Payer },
                { label: "Educativo", value: OrganizationTypeEnum.Educational },
                { label: "Religioso", value: OrganizationTypeEnum.Regligious },
                { label: "Red", value: OrganizationTypeEnum.Network },
              ]}
              fieldProps={{
                suffixIcon: <BiChevronDown className="w-4 h-4 text-gray-600" />,
              }}
              required
            />
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <ProFormSelect
              name="active"
              label="Estado"
              options={[
                { label: "Activo", value: "Activo" },
                { label: "Inactivo", value: "Inactivo" },
              ]}
              rules={[{ required: true, message: "El estado es obligatorio" }]}
              fieldProps={{
                suffixIcon: <BiChevronDown className="w-4 h-4 text-gray-600" />,
              }}
              required
            />
          </div>
        </section>
        {/* Información de Contacto */}
        <section className="mb-8">
          <div className="flex items-center gap-3 mb-6 pb-3 border-b border-gray-200">
            <BsPersonFill className="w-6 h-6 text-blue-500" />
            <span className="text-lg font-semibold text-gray-700">
              Contacto
            </span>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <ProFormText
              name="email"
              label="Email"
              placeholder="contacto@gmail.com"
            />
            <ProFormText
              name="phone"
              label="Teléfono"
              placeholder="+504 8819-4588"
            />
          </div>
        </section>
        {/* Ubicación */}
        <section className="mb-8">
          <div className="flex items-center gap-3 mb-6 pb-3 border-b border-gray-200">
            <BsGeoAltFill className="w-6 h-6 text-blue-500" />
            <span className="text-lg font-semibold text-gray-700">
              Ubicación
            </span>
          </div>
          <ProFormText
            name="address"
            label="Dirección"
            placeholder="Ej. Av. Principal 123, Ciudad"
            className="mb-4"
          />
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <ProFormText name="city" label="Ciudad" placeholder="Ciudad" />
            <ProFormText
              name="state"
              label="Departamento"
              placeholder="Departamento"
            />
            <ProFormText
              name="postalCode"
              label="Código Postal"
              placeholder="12345"
            />
          </div>
        </section>
        {/* Descripción y API Link */}
        <section>
          <div className="grid grid-cols-1 gap-6">
            <ProFormTextArea
              name="description"
              label="Descripción (Opcional)"
              placeholder="Breve descripción de la organización..."
              fieldProps={{ rows: 3, maxLength: 500 }}
              rules={[{ max: 500, message: "No puede exceder 500 caracteres" }]}
            />
            <ProFormText
              name="apiLink"
              label="API Link"
              placeholder="Ej. https://..."
            />
          </div>
        </section>
      </ProForm>
    </div>
  );
}
