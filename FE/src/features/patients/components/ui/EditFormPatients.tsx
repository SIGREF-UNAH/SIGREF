import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormRadio,
  ProFormGroup,
} from "@ant-design/pro-components";
import { Button, Collapse, Space, message } from "antd";
import {
  GlobalOutlined,
  IdcardOutlined,
  UserOutlined,
  PhoneOutlined,
  HomeOutlined,
  PlusOutlined,
} from "@ant-design/icons";
import type { CollapseProps } from "antd";
import {
  usePutApiPatientsId,
  useGetApiPatientsId,
} from "../../../../api/patients/patients";
import { useNavigate, useParams } from "react-router-dom";

const { Panel } = Collapse;

export default function EditFormPatient() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [messageApi, contextHolder] = message.useMessage();
  const { data, error } = useGetApiPatientsId(id);
  const { mutate: updatePatient, isLoading } = usePutApiPatientsId({
    mutation: {
      onSuccess: () => {
        messageApi.success("Paciente actualizado correctamente");
        navigate("/patients/list");
      },
      onError: (error) => {
        console.error(error);
        messageApi.error("Error al actualizar el paciente");
      },
    },
  });

  const handleFinish = async (values: any) => {
    console.log("Form values:", values);

    const updatePatientDto = {
      name: [
        {
          use: values.tipoNombre === "legal" ? 0 : 1,
          text: `${values.primerNombre} ${values.apellidos}`,
          family: values.apellidos,
          given: [values.primerNombre, values.segundoNombre || ""].filter(
            Boolean
          ),
          prefix: [],
          suffix: [],
        },
      ],
      gender: Number(values.gender),
      birthDate: values.fechanacimiento,
      active: values.estadoVital === 1,
      telecom: values.telecom?.map((item: any, index: number) => ({
        system: item.system,
        use: item.use,
        value: item.value || "",
        rank: index + 1,
      })),
      address: [
        {
          use: 0,
          type: 0,
          text: values.address?.[0]?.line?.[0] || "",
          line: [values.address?.[0]?.line?.[0] || ""],
          city: values.address?.[0]?.city || "",
          district: "",
          state: values.address?.[0]?.state || "",
          postalCode: "",
          country: values.address?.[0]?.country || "",
        },
      ],
      identifier: [
        {
          use: 0,
          type: {
            coding: [
              {
                system: "http://terminology.hl7.org/CodeSystem/v2-0203",
                version: "2.9",
                code: "ID",
                display: values.tipoIdentificacion,
                userSelected: true,
              },
            ],
            text: values.tipoIdentificacion,
          },
          system: "https://example.com/identifiers",
          value: values.identifier?.[0]?.value || "",
        },
      ],
    };

    updatePatient({
      id: id || "",
      data: updatePatientDto,
    });
  };

  const handleCancel = () => {
    messageApi.info("Operación cancelada");
    navigate("/patients/list");
  };

  const initialValues = data
    ? {
        primerNombre: data.name?.[0]?.given?.[0] || "",
        segundoNombre: data.name?.[0]?.given?.[1] || "",
        apellidos: data.name?.[0]?.family || "",
        gender: data.gender || 0,
        estadoVital: data.active ? 1 : 0,
        fechanacimiento: data.birthDate ? new Date(data.birthDate) : null,
        tipoIdentificacion: data.identifier?.[0]?.type?.text || "",
        identifier: [{ value: data.identifier?.[0]?.value || "" }],
        telecom:
          data.telecom?.map((t) => ({
            system: t.system === "Phone" ? 0 : 1,
            use: t.use?.toLowerCase() || "home",
            value: t.value,
          })) || [],
        address:
          data.address?.map((a) => ({
            country: a.country || "",
            state: a.state || "",
            city: a.city || "",
            line: a.line || [""],
          })) || [],
      }
    : {};

  const nacionalidadContent = (
    <ProFormGroup>
      <ProFormText
        name="paisNacionalidad"
        label="País de Nacionalidad"
        placeholder="Honduras"
        width="md"
      />
      <ProFormSelect
        name="gender"
        label="Género"
        placeholder="Seleccione"
        width="sm"
        rules={[{ required: true, message: "Campo requerido" }]}
        options={[
          { label: "Masculino", value: 1 },
          { label: "Femenino", value: 2 },
          { label: "Otro", value: 3 },
          { label: "Desconocido", value: 0 },
        ]}
      />
      <ProFormSelect
        name="estadoCivil"
        label="Estado Civil"
        placeholder="Seleccione"
        width="sm"
        options={[
          { label: "Soltero/a", value: "soltero" },
          { label: "Casado/a", value: "casado" },
          { label: "Divorciado/a", value: "divorciado" },
          { label: "Viudo/a", value: "viudo" },
        ]}
      />
      <ProFormSelect
        name="estadoVital"
        label="Estado Vital"
        placeholder="VIVO"
        width="sm"
        options={[
          { label: "Vivo", value: 1 },
          { label: "Fallecido", value: 0 },
        ]}
      />
      <ProFormDatePicker
        name="fechanacimiento"
        label="Fecha de Nacimiento"
        placeholder="dd / mm / yyyy"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
    </ProFormGroup>
  );

  const identificacionesContent = (
    <ProFormGroup>
      <ProFormSelect
        name="tipoIdentificacion"
        label="Tipo"
        placeholder="DNI HN"
        width="sm"
        options={[
          { label: "DNI HN", value: "dni_hn" },
          { label: "Pasaporte", value: "pasaporte" },
          { label: "Cédula", value: "cedula" },
        ]}
      />
      <ProFormText
        name={["identifier", 0, "value"]}
        label="Número"
        placeholder="0000000000000"
        width="md"
      />
      <ProFormText name="emisor" label="Emisor" placeholder="SRNP" width="sm" />
      <ProFormDatePicker
        name="fechaExpedicion"
        label="Fecha Expedición"
        placeholder="dd / mm / yyyy"
        width="md"
      />
      <ProFormRadio.Group
        name="identificacionPreferida"
        label=" "
        options={[{ label: "Preferido", value: true }]}
      />
    </ProFormGroup>
  );

  const nombresContent = (
    <ProFormGroup>
      <ProFormSelect
        name="tipoNombre"
        label="Tipo"
        placeholder="Legal"
        width="sm"
        options={[
          { label: "Legal", value: "legal" },
          { label: "Alias", value: "alias" },
        ]}
      />
      <ProFormText
        name="primerNombre"
        label="Primer Nombre"
        placeholder="Nombre"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
      <ProFormText
        name="segundoNombre"
        label="Segundo Nombre"
        placeholder="Nombre"
        width="md"
      />
      <ProFormText
        name="apellidos"
        label="Apellidos"
        placeholder="Apellido Díaz"
        width="md"
      />
      <ProFormDatePicker
        name="fechaInicioNombre"
        label="Fecha de Inicio"
        placeholder="dd / mm / yyyy"
        width="md"
      />
      <ProFormDatePicker
        name="fechaExpiracionNombre"
        label="Fecha de Expiración"
        placeholder="dd / mm / yyyy"
        width="md"
      />
    </ProFormGroup>
  );

  const contactoContent = (
    <ProFormGroup>
      <ProFormSelect
        name={["telecom", 0, "system"]}
        label="Tipo de contacto"
        placeholder="Seleccione"
        width="sm"
        options={[
          { label: "Teléfono", value: 0 },
          { label: "Email", value: 1 },
        ]}
      />
      <ProFormSelect
        name={["telecom", 0, "use"]}
        label="Uso"
        options={[
          { label: "Móvil", value: "mobile" },
          { label: "Casa", value: "home" },
          { label: "Trabajo", value: "work" },
        ]}
      />
      <ProFormText
        name="codigoPais"
        label="Codigo de País"
        placeholder="+504"
        width="md"
      />
      <ProFormText
        name={["telecom", 0, "value"]}
        label="Valor"
        placeholder="9999-9999 / ejemplo@correo.com"
        width="md"
      />
      <ProFormDatePicker
        name="fechaInicioContacto"
        label="Fecha de Inicio"
        placeholder="dd / mm / yyyy"
        width="md"
      />
      <ProFormDatePicker
        name="fechaExpiracionContacto"
        label="Fecha de Expiración"
        placeholder="dd / mm / yyyy"
        width="md"
      />
      <ProFormRadio.Group
        name="contactoPreferido"
        label=" "
        options={[{ label: "Preferido", value: true }]}
      />
    </ProFormGroup>
  );

  const direccionesContent = (
    <ProFormGroup>
      <ProFormSelect
        name="tipoDireccion"
        label="Tipo"
        placeholder="Casa"
        width="sm"
        options={[
          { label: "Casa", value: "casa" },
          { label: "Trabajo", value: "trabajo" },
          { label: "Otro", value: "otro" },
        ]}
      />
      <ProFormText
        name={["address", 0, "country"]}
        label="Pais"
        placeholder="Honduras"
        width="md"
      />
      <ProFormText
        name={["address", 0, "state"]}
        label="Departamento"
        placeholder="Copan"
        width="md"
      />
      <ProFormText
        name={["address", 0, "city"]}
        label="Ciudad"
        placeholder="Santa Rosa"
        width="md"
      />
      <ProFormText
        name={["address", 0, "line", 0]}
        label="Detalle de Ubicación"
        placeholder="Ave 13, Calle 7, Casa 2 planta Azul"
        width="xl"
        fieldProps={{
          style: { width: "100%" },
        }}
      />
      <ProFormDatePicker
        name="fechaRegistro"
        label="Fecha de Registro"
        placeholder="dd / mm / yyyy"
        width="md"
      />
      <ProFormDatePicker
        name="fechaFinalizacion"
        label="Fecha Finalización"
        placeholder="dd / mm / yyyy"
        width="md"
      />
    </ProFormGroup>
  );

  const collapseItems: CollapseProps["items"] = [
    {
      key: "1",
      label: (
        <Space>
          <GlobalOutlined style={{ color: "#6366f1" }} />
          <span>Nacionalidad</span>
        </Space>
      ),
      children: nacionalidadContent,
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation();
            messageApi.info("Agregar nacionalidad");
          }}
        >
          Agregar
        </Button>
      ),
    },
    {
      key: "2",
      label: (
        <Space>
          <IdcardOutlined style={{ color: "#ef4444" }} />
          <span>Identificaciones</span>
        </Space>
      ),
      children: identificacionesContent,
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation();
            messageApi.info("Agregar identificación");
          }}
        >
          Agregar
        </Button>
      ),
    },
    {
      key: "3",
      label: (
        <Space>
          <UserOutlined style={{ color: "#6366f1" }} />
          <span>Nombres</span>
        </Space>
      ),
      children: nombresContent,
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation();
            messageApi.info("Agregar nombre");
          }}
        >
          Agregar
        </Button>
      ),
    },
    {
      key: "4",
      label: (
        <Space>
          <PhoneOutlined style={{ color: "#f97316" }} />
          <span>Contacto</span>
        </Space>
      ),
      children: contactoContent,
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation();
            messageApi.info("Agregar contacto");
          }}
        >
          Agregar
        </Button>
      ),
    },
    {
      key: "5",
      label: (
        <Space>
          <HomeOutlined style={{ color: "#ef4444" }} />
          <span>Direcciones</span>
        </Space>
      ),
      children: direccionesContent,
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation();
            messageApi.info("Agregar dirección");
          }}
        >
          Agregar
        </Button>
      ),
    },
  ];

  return (
    <div className="flex flex-col min-h-screen bg-gray-50">
      {contextHolder}

      <main className="flex-1 w-full p-8">
        <h1 className="text-3xl font-bold text-[#333333] mb-8">
          Gestión de Pacientes
        </h1>

        <ProForm
          onFinish={handleFinish}
          initialValues={initialValues}
          submitter={{
            render: (_, dom) => (
              <div className="flex justify-end gap-3 mt-6">
                <Button size="large" onClick={handleCancel}>
                  Cancelar
                </Button>
                <Button
                  type="primary"
                  size="large"
                  htmlType="submit"
                  loading={isLoading}
                >
                  Guardar Cambios
                </Button>
              </div>
            ),
          }}
        >
          <Collapse
            defaultActiveKey={["1", "2", "3", "4", "5"]}
            items={collapseItems}
            className="mb-6"
          />
        </ProForm>
      </main>
    </div>
  );
}
