import {
  ProForm,
  ProFormText,
  ProFormDatePicker,
  ProFormSwitch,
} from "@ant-design/pro-components";
import { FaUser } from "react-icons/fa";
import { BsPersonVcardFill, BsShieldLock } from "react-icons/bs";
import { useParams, useNavigate } from "react-router-dom";
import { Button, Spin } from "antd";
import { useGetApiPractitionerId } from "../../../api/practitioner/practitioner";

type EmployeeDetail = {
  id: { value: string };
  meta: { lastUpdated: { value: string }, versionId: { value: string } };
  identifier: {
    use: { value: string };
    type: { text: { value: string } };
    system: { value: string };
    value: { value: string };
  }[];
  active: { value: boolean };
  name: {
    use: { value: string };
    text: { value: string };
    family: { value: string };
    given: { value: string }[];
    prefix?: { value: string }[];
    suffix?: { value: string }[];
  }[];
  telecom: {
    system: { value: string };
    value: { value: string };
    use: { value: string };
    rank: { value: number };
  }[];
  gender: { value: "male" | "female" | "other" };
  birthDate: { value: string };
};


export default function EmployeeDetailsPage() {
  const { id } = useParams(); // <- el id del empleado desde la URL
  const navigate = useNavigate();

const { data, isLoading } = useGetApiPractitionerId<EmployeeDetail, any>(id || "");


  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <Spin size="large" tip="Cargando datos del empleado..." />
      </div>
    );
  }

  const employee = data;

  const name = employee?.name?.[0];
  const telecom = employee?.telecom || [];
  const phone = telecom.find((t: any) => t.system?.value === "fax")?.value?.value;
  const email = telecom.find((t: any) => t.system?.value === "email")?.value?.value;
  const identifier = employee?.identifier?.[0];
  const genderMap: Record<string, string> = {
    male: "Masculino",
    female: "Femenino",
    other: "Otro",
  };

  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      {/* Encabezado */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center gap-3">
          <FaUser className="w-10 h-10 text-blue-500" />
          <span className="text-xl font-semibold text-[#333333]">
            Detalles del Empleado
          </span>
        </div>
        <Button onClick={() => navigate("/employees")}>Volver</Button>
      </div>

      <ProForm
        submitter={false}
        initialValues={{
          firstName: name?.given?.[0]?.value,
          middleName: name?.given?.[1]?.value,
          lastName: name?.family?.value,
          dni: identifier?.value?.value,
          idType: identifier?.type?.text?.value,
          phone: phone,
          email: email,
          gender: genderMap[employee?.gender?.value] || "N/A",
          birthDate: employee?.birthDate?.value,
          active: employee?.active?.value,
        }}
        readonly
      >
        {/* Datos Personales */}
        <section className="mb-8">
          <div className="flex items-center gap-3 mb-6">
            <BsPersonVcardFill className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Datos Personales
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <ProFormText name="firstName" label="Primer Nombre" />
            <ProFormText name="middleName" label="Segundo Nombre" />
            <ProFormText name="lastName" label="Apellidos" />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <ProFormText name="dni" label="Identificador" />
            <ProFormText name="idType" label="Tipo de Identificador" />
            <ProFormText name="phone" label="Teléfono" />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
            <ProFormText name="gender" label="Género" />
            <ProFormDatePicker name="birthDate" label="Fecha de Nacimiento" />
            <ProFormSwitch name="active" label="Activo" />
          </div>
        </section>

        {/* Datos de Usuario (simples) */}
        <section>
          <div className="flex items-center gap-3 mb-6">
            <BsShieldLock className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Datos de Usuario
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <ProFormText name="email" label="Correo Electrónico" />
            <ProFormText name="id" label="Código Interno" initialValue={employee?.id?.value} />
            <ProFormText
              name="lastUpdated"
              label="Última Actualización"
              initialValue={employee?.meta?.lastUpdated?.value?.split("T")[0]}
            />
          </div>
        </section>
      </ProForm>
    </div>
  );
}
