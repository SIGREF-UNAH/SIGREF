import { ProList } from "@ant-design/pro-components";
import { Avatar, Col, Input, Row, Select, Space, Typography } from "antd";
import { ManOutlined, WomanOutlined } from "@ant-design/icons";

export const ListPatient = ({pacientesData, setSelectedPaciente, selectedPaciente, pacienteFilters, setPacienteFilter, setPacienteFilters}:any) => {

    const pacientesFiltrados = pacientesData.filter((paciente: { nombre: string; genero: string; nacionalidad: string; identificador: string; }) => {
    const matchSearch = paciente.nombre
      .toLowerCase()
      .includes(pacienteFilters.searchPaciente.toLowerCase());
    const matchGenero =
      pacienteFilters.genero === "todos" ||
      paciente.genero === pacienteFilters.genero;
    const matchNacionalidad =
      pacienteFilters.nacionalidad === "todos" ||
      paciente.nacionalidad === pacienteFilters.nacionalidad;
    const matchIdentificador =
      !pacienteFilters.identificador ||
      paciente.identificador
        .toLowerCase()
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
                  onChange={(value) =>
                    setPacienteFilter("tipoIdentificador", value)
                  }
                  style={{ width: "100%", marginTop: 8 }}
                >
                  <Select.Option value="DNI">DNI</Select.Option>
                  <Select.Option value="Pasaporte">Pasaporte</Select.Option>
                </Select>
              </Col>
              <Col span={6}>
                <Typography.Text strong>Genero</Typography.Text>
                <Select
                  value={pacienteFilters.genero}
                  onChange={(value) => setPacienteFilter("genero", value)}
                  style={{ width: "100%", marginTop: 8 }}
                >
                  <Select.Option value="todos">Todos</Select.Option>
                  <Select.Option value="M">Masculino</Select.Option>
                  <Select.Option value="F">Femenino</Select.Option>
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
                  <Select.Option value="HN">Honduras</Select.Option>
                  <Select.Option value="GT">Guatemala</Select.Option>
                  <Select.Option value="SV">El Salvador</Select.Option>
                </Select>
              </Col>
              <Col span={6}>
                <Typography.Text strong>Identificador</Typography.Text>
                <Input
                  value={pacienteFilters.identificador}
                  onChange={(e) =>
                    setPacienteFilter("identificador", e.target.value)
                  }
                  style={{ marginTop: 8 }}
                  placeholder="DNI/Pasaporte"
                />
              </Col>
            </Row>

            {/* List para Pacientes */}

            <ProList<any>
              rowKey="id"
              headerTitle={<Typography.Title level={5}>Datos del paciente</Typography.Title>}
              dataSource={pacientesFiltrados}
              pagination={{
                current: pacienteFilters.page,
                pageSize: pacienteFilters.pageSize,
                onChange: (page, pageSize) => setPacienteFilters({page, pageSize}), 
                showSizeChanger: true,
                pageSizeOptions: ["5", "10", "20"],
              }}
              metas={{
                avatar: {
                  render: (_, record) => (
                    <Avatar
                      icon={
                        record.genero === "M" ? (
                          <ManOutlined />
                        ) : (
                          <WomanOutlined />
                        )
                      }
                      style={{
                        backgroundColor:
                          record.genero === "M" ? "#1890ff" : "#eb2f96",
                      }}
                    />
                  ),
                },
                title: {
                  dataIndex: "nombre",
                  search: true,
                  render: (text) => <Typography.Text strong>{text}</Typography.Text>,
                },
                description: {
                  render: (_, record) => (
                    <Space direction="vertical" size={0}>
                      <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                        {record.identificador}
                      </Typography.Text>
                      <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                        {record.nacionalidad} • Nacimiento: {record.nacimiento}
                      </Typography.Text>
                    </Space>
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
                      : undefined,
                  borderRadius: 4,
                },
              })}
            />
    </>
  );
};