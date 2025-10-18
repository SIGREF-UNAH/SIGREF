import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormTextArea,
} from "@ant-design/pro-components";
import { BiChevronDown } from "react-icons/bi";
import { MdOutlineAddLocationAlt } from "react-icons/md";
import { FaCheck } from "react-icons/fa";
import { LocationStatus, LocationMode } from "../../../../api/models";
import { useNavigate } from "react-router-dom";
import useLocationForm from "../../hooks/useLocationForm";
import { useRef, useState } from "react";
import { BsBuilding, BsGeoAltFill, BsPersonFill } from "react-icons/bs";
import { Button } from "antd";
import { DeleteOutlined, PlusOutlined } from "@ant-design/icons";

export default function LocationForm() {
  const navigate = useNavigate();
  const { handleSubmit, isSubmitting } = useLocationForm();
   const [contacts, setContacts] = useState([{ id: "1", name: "", phone: "", email: "" }]);
  const formRef = useRef<any>();

  const onFinish = async (values: any) => {
    const success = await handleSubmit(values);
    if (success) {
      formRef.current?.resetFields();
      navigate("/locations/list");
    } else {
      console.log("Creado correctamente");
    }
  };

   const addContact = () => {
    setContacts([...contacts, { id: Date.now().toString(), name: "", phone: "", email: "" }]);
  };

  const removeContact = (id: string) => {
    if (contacts.length > 1) {
      setContacts(contacts.filter((contact) => contact.id !== id));
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

      <ProForm
        formRef={formRef}
        onFinish={onFinish}
        initialValues={{
          name: "",
          alias: [],
          description: null,
          status: LocationStatus.NUMBER_0,
          mode: LocationMode.NUMBER_0,
          address: {
            line: [""],
            city: null,
            state: null,
            postalCode: null,
            country: null,
          },
          telecom: [],
          type: null,
          partOfId: null,
          managingOrganizationIds: null,
        }}
        submitter={{
          searchConfig: { submitText: "Crear Ubicación" },
          resetButtonProps: false,
          submitButtonProps: {
            loading: isSubmitting,
            icon: <FaCheck className="w-4 h-4" />,
            className:
              "px-6 py-2 !bg-green-500 hover:!bg-green-600 text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2",
          },
          render: (_, dom) => (
            <div className="flex justify-end pt-2 pb-2">{dom[0]}</div>
          ),
        }}
      >
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
              name="name"
              label="Nombre de la ubicación"
              placeholder="Ej. Sala de emergencias"
              rules={[{ required: true, message: "El nombre es obligatorio" }]}
            />
            <ProFormText
              name="alias"
              label="Alias"
              placeholder="Ej. Emergencias, ER"
            />
            <ProFormSelect
              name="status"
              label="Estado"
              options={[
                { label: "Activo", value: LocationStatus.NUMBER_0 },
                { label: "Inactivo", value: LocationStatus.NUMBER_1 },
                { label: "Suspendido", value: LocationStatus.NUMBER_2 },
              ]}
              rules={[{ required: true, message: "El estado es obligatorio" }]}
              fieldProps={{
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-2">
            <ProFormSelect
              name="mode"
              label="Modo"
              options={[
                { label: "Kind", value: LocationMode.NUMBER_0 },
                { label: "Instance", value: LocationMode.NUMBER_1 },
              ]}
              rules={[{ required: true, message: "El modo es obligatorio" }]}
              fieldProps={{
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />
            <ProFormText
              name="type"
              label="Tipo de función"
              placeholder="Ej. Cuarto de emergencias"
            />
          </div>

          <ProFormTextArea
            name="description"
            label="Descripción"
            placeholder="Descripción adicional"
            fieldProps={{ rows: 2 }}
          />
        </section>

        {/* Dirección física */}
        <section className="mt-6">
          <div className="flex items-center gap-3 mb-6">
            <BsGeoAltFill className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Dirección física
            </span>
          </div>

          <ProFormText
            name={["address", "line", 0]}
            label="Dirección"
            placeholder="Ej. Avenida principal 123"
            rules={[{ required: true, message: "La dirección es obligatoria" }]}
          />
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-2">
            <ProFormText name={["address", "city"]} label="Ciudad" />
            <ProFormText name={["address", "state"]} label="Estado/Provincia" />
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
            <ProFormText
              name={["address", "postalCode"]}
              label="Código Postal"
            />
            <ProFormText name={["address", "country"]} label="País" />
          </div>
        </section>

        <div className="mb-8 border-t pt-6">
          <div className="mb-4 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <BsPersonFill className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">
                Información de contacto
              </span>
            </div>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={addContact}
              className="bg-green-500 hover:bg-green-600"
            >
              Agregar Contacto
            </Button>
          </div>
          <div className="space-y-4">
            {contacts.map((contact, index) => (
              <div
                key={contact.id}
                className="grid grid-cols-[1fr_1fr_1fr_auto] items-end gap-4"
              >
                <ProFormText
                  name={`contacto_nombre_${contact.id}`}
                  label={index === 0 ? "Nombre de contacto" : undefined}
                  placeholder="Ej. Juan"
                  initialValue={contact.name}
                  onChange={(e) =>
                    setContacts((prev) =>
                      prev.map((c) =>
                        c.id === contact.id ? { ...c, name: e.target.value } : c
                      )
                    )
                  }
                />
                <ProFormText
                  name={`contacto_telefono_${contact.id}`}
                  label={index === 0 ? "Teléfono" : undefined}
                  placeholder="Ej. +504 4864-5945"
                  initialValue={contact.phone}
                  onChange={(e) =>
                    setContacts((prev) =>
                      prev.map((c) =>
                        c.id === contact.id
                          ? { ...c, phone: e.target.value }
                          : c
                      )
                    )
                  }
                />
                <ProFormText
                  name={`contacto_email_${contact.id}`}
                  label={index === 0 ? "Correo electrónico" : undefined}
                  placeholder="Ej. contacto@hospital.cr"
                  initialValue={contact.email}
                  onChange={(e) =>
                    setContacts((prev) =>
                      prev.map((c) =>
                        c.id === contact.id
                          ? { ...c, email: e.target.value }
                          : c
                      )
                    )
                  }
                />
                <Button
                  danger
                  icon={<DeleteOutlined />}
                  onClick={() => removeContact(contact.id)}
                  disabled={contacts.length === 1}
                  className="mb-6"
                />
              </div>
            ))}
          </div>
        </div>

        {/* Jerarquía */}
        <section className="mt-6">
          <div className="flex items-center gap-3 mb-6">
            <BsBuilding className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Organización y jerarquía
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
            <ProFormText
              name="managingOrganizationIds"
              label="Organización responsable"
            />
            <ProFormText name="partOfId" label="Parte de (ubicación padre)" />
          </div>
        </section>
      </ProForm>
    </div>
  );
}
