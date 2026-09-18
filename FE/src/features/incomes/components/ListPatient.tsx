import {
  Avatar,
  Col,
  Input,
  Row,
  Select,
  Space,
  Spin,
  Tag,
  Typography,
} from "antd";
import { ManOutlined, WomanOutlined, UserOutlined } from "@ant-design/icons";
import { ProList } from "@ant-design/pro-components";
import { createPaginationConfig } from "../../../shared/components/ui";

export const ListPatient = ({
  pacientesData,
  setSelectedPaciente,
  selectedPaciente,
  pacienteFilters,
  setPacienteFilter,
  setPacienteFilters,
  isLoading,
}: any) => {
  // Filtrar pacientes basados en los filtros locales
  const pacientesFiltrados = (pacientesData ?? []).filter((paciente: any) => {
    const searchTerm = pacienteFilters.searchPaciente?.toLowerCase() || "";

    const matchSearch = paciente.nombre?.toLowerCase().includes(searchTerm);

    const matchGenero =
      pacienteFilters.genero === "todos" ||
      paciente.genero === pacienteFilters.genero ||
      (pacienteFilters.genero === "M" && paciente.genero === "Masculino") ||
      (pacienteFilters.genero === "F" && paciente.genero === "Femenino");

    const matchNacionalidad =
      pacienteFilters.nacionalidad === "todos" ||
      paciente.nacionalidad === pacienteFilters.nacionalidad;

    const matchIdentificador =
      !pacienteFilters.identificador ||
      paciente.identificador
        ?.toLowerCase()
        .includes(pacienteFilters.identificador.toLowerCase());

    return (
      matchSearch && matchGenero && matchNacionalidad && matchIdentificador
    );
  });

  return (
    <>
      {/* Filtros de Paciente */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col span={6}>
          <Typography.Text strong>Tipo de Identificador</Typography.Text>
          <Select
            value={pacienteFilters.tipoIdentificador}
            onChange={(value) => setPacienteFilter("tipoIdentificador", value)}
            style={{ width: "100%", marginTop: 8 }}
          >
            <Select.Option value="DNI">DNI</Select.Option>
            <Select.Option value="PPN">Pasaporte</Select.Option>
            <Select.Option value="NI">ID Nacional</Select.Option>
          </Select>
        </Col>
        <Col span={6}>
          <Typography.Text strong>Género</Typography.Text>
          <Select
            value={pacienteFilters.genero}
            onChange={(value) => setPacienteFilter("genero", value)}
            style={{ width: "100%", marginTop: 8 }}
          >
            <Select.Option value="todos">Todos</Select.Option>
            <Select.Option value="Masculino">Masculino</Select.Option>
            <Select.Option value="Femenino">Femenino</Select.Option>
            <Select.Option value="Otro">Otro</Select.Option>
          </Select>
        </Col>
        <Col span={6}>
          <Typography.Text strong>Nacionalidad</Typography.Text>
          <Select
            value={pacienteFilters.nacionalidad}
            onChange={(value) => setPacienteFilter("nacionalidad", value)}
            style={{ width: "100%", marginTop: 8 }}
          >
            <Select.Option value="todos">Todos</Select.Option>
            <Select.Option value="Hondureña">Honduras</Select.Option>
            <Select.Option value="Guatemalteca">Guatemala</Select.Option>
            <Select.Option value="Salvadoreña">El Salvador</Select.Option>
          </Select>
        </Col>
        <Col span={6}>
          <Typography.Text strong>Identificador</Typography.Text>
          <Input
            value={pacienteFilters.identificador}
            onChange={(e) => setPacienteFilter("identificador", e.target.value)}
            style={{ marginTop: 8 }}
            placeholder="DNI/Pasaporte"
          />
        </Col>
      </Row>

      {/* Lista de Pacientes */}
      {isLoading ? (
        <div style={{ textAlign: "center", padding: "40px 0" }}>
          <Spin size="large" />
        </div>
      ) : (
        <ProList<any>
          rowKey="id"
          headerTitle={
            <Typography.Title level={5}>Datos del paciente</Typography.Title>
          }
          dataSource={pacientesFiltrados}
          pagination={createPaginationConfig({
            current: pacienteFilters.pagePaciente,
            pageSize: pacienteFilters.pageSizePaciente,
            total: pacientesFiltrados.length,
            onChange: (page, pageSize) =>
              setPacienteFilters({
                pagePaciente: page,
                pageSizePaciente: pageSize,
              }),
            pageSizeOptions: ["5", "10", "20"],
          })}
          metas={{
            avatar: {
              render: (_, record) => (
                <Avatar
                  icon={
                    record.genero === "Masculino" ? (
                      <ManOutlined />
                    ) : record.genero === "Femenino" ? (
                      <WomanOutlined />
                    ) : (
                      <UserOutlined />
                    )
                  }
                  style={{
                    backgroundColor:
                      record.genero === "Masculino"
                        ? "#1890ff"
                        : record.genero === "Femenino"
                          ? "#eb2f96"
                          : "#52c41a",
                  }}
                />
              ),
            },
            title: {
              dataIndex: "nombre",
              search: true,
              render: (text) => (
                <Typography.Text strong>
                  {text || "Desconocido"}
                </Typography.Text>
              ),
            },
            description: {
              render: (_, record) => (
                <Space direction="vertical" size={0}>
                  <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                    {record.identificadorTipo}: {record.identificador}
                  </Typography.Text>
                  <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                    {record.nacionalidad} • Nacimiento: {record.nacimiento}
                  </Typography.Text>
                  {record.contacto && record.contacto !== "-" && (
                    <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                      Contacto: {record.contacto}
                    </Typography.Text>
                  )}
                </Space>
              ),
            },
            subTitle: {
              render: (_, record) => (
                <Tag color={record.estadoVital === "Vivo" ? "green" : "red"}>
                  {record.estadoVital}
                </Tag>
              ),
            },
          }}
          onItem={(record: any) => ({
            onClick: () => setSelectedPaciente(record),
            style: {
              cursor: "pointer",
              border:
                selectedPaciente?.id === record.id
                  ? "2px solid #1890ff"
                  : "1px solid #f0f0f0",
              borderRadius: 8,
              marginBottom: 12,
              padding: "12px 16px",
              transition: "all 0.3s",
              backgroundColor:
                selectedPaciente?.id === record.id ? "#e6f7ff" : "white",
              boxShadow:
                selectedPaciente?.id === record.id
                  ? "0 4px 12px rgba(24, 144, 255, 0.15)"
                  : "none",
            },
          })}
        />
      )}
    </>
  );
};
