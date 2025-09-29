import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormTextArea,
} from "@ant-design/pro-components";
import { message } from "antd";
import useLocationForm from "../../hooks/useLocationForm";
import { BiChevronDown } from "react-icons/bi";
import { BsBuilding, BsGeoAltFill, BsPersonFill } from "react-icons/bs";
import { MdOutlineAddLocationAlt } from "react-icons/md";
import { FaCheck } from "react-icons/fa";
import { Button } from "antd";

export default function LocationForm() {
  const { formData, setField, handleSubmit, isSubmitting, error } =
    useLocationForm();

  const onFinish = async () => {
    const success = await handleSubmit();
    if (success) {
      message.success("Ubicación creada exitosamente");
    } else {
      message.error(error || "Error al crear la ubicación");
    }
  };

  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      {/* Header */}
      <div className="flex items-center gap-3 mb-8">
        <MdOutlineAddLocationAlt className="w-10 h-10 text-blue-500" />
        <span className="text-xl font-semibold text-[#333333]">
          Crear Ubicación
        </span>
      </div>

      <ProForm onFinish={onFinish} initialValues={formData} submitter={false}>
        <div className="max-w-[105rem] mx-auto">
          {/* Información Básica */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <BsBuilding className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">
                Información Básica
              </span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-2">
              <ProFormText
                name="identifier"
                label={
                  <span className="text-[#616161] font-medium">
                    Identificador
                  </span>
                }
                placeholder="Ej. Juan"
                fieldProps={{
                  value: formData.identifier,
                  onChange: (e) => setField("identifier", e.target.value),
                }}
              />
              <ProFormText
                name="locationName"
                label={
                  <span className="text-[#616161] font-medium">
                    Nombre de la ubicación
                  </span>
                }
                placeholder="Ej. Sala de emergencias"
                fieldProps={{
                  value: formData.locationName,
                  onChange: (e) => setField("locationName", e.target.value),
                }}
              />
              <ProFormText
                name="alias"
                label={
                  <span className="text-[#616161] font-medium">
                    Alias
                  </span>
                }
                placeholder="Ej. Emergencias, ER"
                fieldProps={{
                  value: formData.alias,
                  onChange: (e) => setField("alias", e.target.value),
                }}
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-2">
              <ProFormSelect
                name="state"
                label={
                  <span className="text-[#616161] font-medium">
                    Estado
                  </span>
                }
                options={[
                  { label: "Seleccionar estado", value: "" },
                  { label: "Activo", value: "activo" },
                  { label: "Inactivo", value: "inactivo" },
                  { label: "Suspendido", value: "suspendido" },
                ]}
                fieldProps={{
                  value: formData.state,
                  onChange: (value) => setField("state", value),
                  suffixIcon: (
                    <BiChevronDown className="w-4 h-4 text-[#616161]" />
                  ),
                }}
              />
              <ProFormSelect
                name="mode"
                label={
                  <span className="text-[#616161] font-medium">
                    Modo
                  </span>
                }
                options={[
                  { label: "Seleccionar modo", value: "" },
                  { label: "Kind", value: "kind" },
                  { label: "Instance", value: "instance" },
                ]}
                fieldProps={{
                  value: formData.mode,
                  onChange: (value) => setField("mode", value),
                  suffixIcon: (
                    <BiChevronDown className="w-4 h-4 text-[#616161]" />
                  ),
                }}
              />
              <ProFormText
                name="functionType"
                label={
                  <span className="text-[#616161] font-medium">
                    Tipo de función
                  </span>
                }
                placeholder="Ej. Cuarto de emergencias"
                fieldProps={{
                  value: formData.functionType,
                  onChange: (e) => setField("functionType", e.target.value),
                }}
              />
            </div>

            <ProFormTextArea
              name="description"
              label={
                  <span className="text-[#616161] font-medium">
                    Descripción
                  </span>
                }
              placeholder="Descripción adicional de la ubicación"
              fieldProps={{
                value: formData.description,
                onChange: (e) => setField("description", e.target.value),
                rows: 2,
              }}
            />
          </section>

          <hr className="border-[#000] my-8" />

          {/* Dirección física */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <BsGeoAltFill className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">
                Dirección física
              </span>
            </div>

            <ProFormText
              name="address"
              label={
                  <span className="text-[#616161] font-medium">
                    Dirección
                  </span>
                }
              placeholder="Ej. Avenida principal 123"
              fieldProps={{
                value: formData.address,
                onChange: (e) => setField("address", e.target.value),
              }}
            />

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-2">
              <ProFormText
                name="city"
                label={
                  <span className="text-[#616161] font-medium">
                    Ciudad
                  </span>
                }
                placeholder="Ej. Ciudad"
                fieldProps={{
                  value: formData.city,
                  onChange: (e) => setField("city", e.target.value),
                }}
              />
              <ProFormText
                name="stateProvince"
                label={
                  <span className="text-[#616161] font-medium">
                    Estado/Provincia
                  </span>
                }
                placeholder="Ej. Provincia"
                fieldProps={{
                  value: formData.stateProvince,
                  onChange: (e) => setField("stateProvince", e.target.value),
                }}
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
              <ProFormText
                name="postalCode"
                label={
                  <span className="text-[#616161] font-medium">
                    Código Postal
                  </span>
                }
                placeholder="Ej. 12345"
                fieldProps={{
                  value: formData.postalCode,
                  onChange: (e) => setField("postalCode", e.target.value),
                }}
              />
              <ProFormText
                name="country"
                label={
                  <span className="text-[#616161] font-medium">
                    País
                  </span>
                }
                placeholder="Ej. Honduras"
                fieldProps={{
                  value: formData.country,
                  onChange: (e) => setField("country", e.target.value),
                }}
              />
            </div>
          </section>

          <hr className="border-[#000] mt-2 mb-8" />

          {/* Información de contacto */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <BsPersonFill className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">
                Información de contacto
              </span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
              <ProFormText
                name="contactName"
                label={
                  <span className="text-[#616161] font-medium">
                    Nombre de contacto
                  </span>
                }
                placeholder="Ej. Dr. Juan"
                fieldProps={{
                  value: formData.contactName,
                  onChange: (e) => setField("contactName", e.target.value),
                }}
              />
              <ProFormText
                name="phone"
                label={
                  <span className="text-[#616161] font-medium">
                    Teléfono
                  </span>
                }
                placeholder="Ej. 9878-8967"
                fieldProps={{
                  value: formData.phone,
                  onChange: (e) => setField("phone", e.target.value),
                }}
              />
              <ProFormText
                name="email"
                label={
                  <span className="text-[#616161] font-medium">
                    Correo Electrónico
                  </span>
                }
                placeholder="Ej. ever@me.gmail"
                fieldProps={{
                  type: "email",
                  value: formData.email,
                  onChange: (e) => setField("email", e.target.value),
                }}
              />
            </div>
          </section>

          <hr className="border-[#000] mt-2 mb-8" />

          {/* Organización y jerarquía */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <BsBuilding className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">
                Organización y jerarquía
              </span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
              <ProFormText
                name="responsibleOrganization"
                label={
                  <span className="text-[#616161] font-medium">
                    Organización responsable
                  </span>
                }
                placeholder="Ej. Hospital nacional"
                fieldProps={{
                  value: formData.responsibleOrganization,
                  onChange: (e) =>
                    setField("responsibleOrganization", e.target.value),
                }}
              />
              <ProFormText
                name="parentLocation"
                label={
                  <span className="text-[#616161] font-medium">
                    Parte de (ubicación padre)
                  </span>
                }
                placeholder="Ej. Edificio principal"
                fieldProps={{
                  value: formData.parentLocation,
                  onChange: (e) => setField("parentLocation", e.target.value),
                }}
              />
            </div>
          </section>

          {/* Submit Button */}
          <div className="flex justify-end pt-2 pb-2">
            <Button
              type="primary"
              htmlType="submit"
              loading={isSubmitting}
              className="px-6 py-2 !bg-green-500 hover:!bg-green-600 text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2"
            >
              <FaCheck className="w-4 h-4" />{" "}
              {isSubmitting ? "Creando..." : "Crear Ubicación"}
            </Button>
          </div>
        </div>
      </ProForm>
    </div>
  );
}
