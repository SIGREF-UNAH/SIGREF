import { useState, useEffect } from "react";
import {
  ProForm,
  ProFormText,
  ProFormTextArea,
  ProFormSelect,
} from "@ant-design/pro-components";
import { Button } from "antd";
import { PlusOutlined, DeleteOutlined } from "@ant-design/icons";
import { BsBuilding, BsGeoAltFill, BsPersonFill } from "react-icons/bs";
import { MdOutlineAddLocationAlt } from "react-icons/md";
import {
  usePutApiLocationsId,
  useGetApiLocationsId,
} from "../../../../api/locations/locations";
import { useParams } from "react-router";
import { useNavigate } from "react-router-dom";
import { message, Spin } from "antd";
import type { UpdateLocationDto } from "../../../../api/models";

export default function EditLocation() {
  const { id } = useParams();
  const numericId = Number(id);

  const [contacts, setContacts] = useState([
    { id: "1", name: "", phone: "", email: "" },
  ]);
  const { data: location, isPending } = useGetApiLocationsId(numericId);

  const [form] = ProForm.useForm();

  const { mutate: updateLocation, isPending: isUpdating } =
    usePutApiLocationsId({
      mutation: {
        onSuccess: () => message.success("Ubicación actualizada con éxito"),
        onError: () => message.error("Error al actualizar la ubicación"),
      },
    });

  useEffect(() => {
    if (location) {
      form.setFieldsValue(location);

      if (location.contactos && location.contactos.length > 0) {
        setContacts(
          location.contactos.map((c: any, index: number) => ({
            id: Date.now().toString() + index,
            name: c.nombre,
            phone: c.telefono,
            email: c.email,
          }))
        );
      }
    }
  }, [location, form]);

  const navigate = useNavigate();
  const addContact = () => {
    setContacts([
      ...contacts,
      { id: Date.now().toString(), name: "", phone: "", email: "" },
    ]);
  };

  const removeContact = (id: string) => {
    if (contacts.length > 1) {
      setContacts(contacts.filter((contact) => contact.id !== id));
    }
  };

 useEffect(() => {
  if (location) {
    form.setFieldsValue({
      nombreUbicacion: location.name || "",
      alias: location.alias?.join(", ") || "",
      tipoFuncion: location.type || "",
      descripcion: location.description || "",
      estado: location.status || "active",
      modo: location.mode || "Instance",
      direccion: location.address?.line?.[0] || "",
      ciudad: location.address?.city || "",
      estadoProvincia: location.address?.state || "",
      codigoPostal: location.address?.postalCode || "",
      pais: location.address?.country || "",
      organizacionResponsable: location.managingOrganizationIds || "",
      locationPadre: location.partOfId || "",
    });
    if (location.contactos && location.contactos.length > 0) {
      setContacts(
        location.contactos.map((c, index) => ({
          id: Date.now().toString() + index,
          name: c.nombre,
          phone: c.telefono,
          email: c.email,
        }))
      );
    }
  }
}, [location, form]);
  if (isPending) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      <div className="mb-6 flex items-center gap-2">
        <MdOutlineAddLocationAlt className="w-10 h-10 text-blue-500" />
        <span className="text-xl font-semibold text-[#333333]">
          Actualizar Ubicación
        </span>
      </div>

      <ProForm
        form={form}
        initialValues={location}
        submitter={{
          render: (props) => (
            <div className="flex justify-end gap-4">
              <Button
                type="default"
                onClick={() => navigate("/locations/list")}
                className="border-gray-300 hover:border-blue-500"
                size="large"
              >
                Cancelar
              </Button>
              <Button
                type="primary"
                onClick={() => props.form?.submit?.()}
                className="bg-green-500 hover:bg-green-600"
                size="large"
                loading={isUpdating}
              >
                Editar Ubicación
              </Button>
            </div>
          ),
        }}
        onFinish={(values) => {
          const payload: UpdateLocationDto = {
            name: values.nombreUbicacion || "",
            description: values.descripcion || null,
            status: values.estado || "active",
            mode: values.modo || "Instance",
            type: values.tipoFuncion || null,
            address: {
              line: values.direccion ? [values.direccion] : [],
              city: values.ciudad || null,
              state: values.estadoProvincia || null,
              postalCode: values.codigoPostal || null,
              country: values.pais || null,
            },
            telecom: contacts
              .flatMap((c, idx) => [
                c.phone
                  ? {
                      system: "phone",
                      value: c.phone,
                      use: "work",
                      rank: idx + 1,
                    }
                  : null,
                c.email
                  ? {
                      system: "email",
                      value: c.email,
                      use: "work",
                      rank: idx + 1,
                    }
                  : null,
              ])
              .filter(Boolean),
          };

          console.log("Enviando payload:", payload);

          if (numericId) {
            updateLocation({ id: numericId, data: payload });
          } else {
            message.error("ID de ubicación inválido");
          }
        }}
      >
        {/* Información Básica */}
        <div className="mb-8">
          <div className="mb-4 flex items-center gap-2">
            <BsBuilding className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Información Básica
            </span>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <ProFormText
              name="nombreUbicacion"
              label="Nombre de la Ubicación"
              placeholder="Ej. Sala de emergencias"
              rules={[{ required: true, message: "Este campo es requerido" }]}
            />
            <ProFormText
              name="alias"
              label="Alias"
              placeholder="Ej. Emergencias, ER"
            />
            <ProFormText
              name="tipoFuncion"
              label="Tipo de Función"
              placeholder="Ej. Emergencias, ROOM"
            />
          </div>

          <div className="mt-4">
            <ProFormSelect
              name="estado"
              label="Estado"
              placeholder="Seleccionar estado"
              options={[
                { label: "Activo", value: "active" },
                { label: "Inactivo", value: "inactive" },
                { label: "Suspendido", value: "suspended" },
              ]}
            />
          </div>
          <div className="mt-4">
            <ProFormSelect
              name="modo"
              label="Modo"
              placeholder="Seleccionar modo"
              options={[
                { label: "Instancia", value: "Instance" },
                { label: "Tipo", value: "Kind" },
              ]}
            />
          </div>
          <div className="mt-4">
            <ProFormTextArea
              name="descripcion"
              label="Descripción"
              placeholder="Descripción adicional de la ubicación"
              fieldProps={{ rows: 3 }}
            />
          </div>
        </div>
        {/* Dirección Física */}
        <div className="mb-8 border-t pt-6">
          <div className="mb-4 flex items-center gap-2">
            <BsGeoAltFill className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Dirección física
            </span>
          </div>
          <div className="mb-4">
            <ProFormText
              name="direccion"
              label="Dirección"
              placeholder="Ej. Avenida principal 123"
              rules={[{ required: true, message: "Este campo es requerido" }]}
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <ProFormText
              name="ciudad"
              label="Ciudad"
              placeholder="Ej. San Jose"
            />
            <ProFormText
              name="estadoProvincia"
              label="Estado/Provincia"
              placeholder="Ej. San Jose"
            />
          </div>
          <div className="mt-4 grid grid-cols-2 gap-4">
            <ProFormText
              name="codigoPostal"
              label="Código postal"
              placeholder="Ej. 10110"
            />
            <ProFormText
              name="pais"
              label="País"
              placeholder="Ej. Costa Rica"
            />
          </div>
        </div>
        {/* Información de Contacto */}
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
        {/* Organización y Jerarquía */}
        <div className="border-t pt-6">
          <div className="mb-4 flex items-center gap-2">
            <BsBuilding className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Organización y jerarquía
            </span>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <ProFormText
              name="organizacionResponsable"
              label="Organización Responsable"
              placeholder="Ej. Hospital Nacional"
            />
            <ProFormText
              name="locationPadre"
              label="Parte de (Location Padre)"
              placeholder="Ej. Edificio Principal"
            />
          </div>
        </div>
      </ProForm>
    </div>
  );
}
