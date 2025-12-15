import { ProList } from "@ant-design/pro-components";
import { Space, Tabs, Tag, Typography, Spin } from "antd";

export const ServiceIncome = ({
  serviciosData,
  servicioFilters,
  setServicioFilter,
  setServicioFilters,
  handleSelectServicio,
  selectedServicio,
  isLoading,
}: any) => {
  // Filtrar servicios basados en los filtros actuales
  const serviciosFiltrados = (serviciosData ?? []).filter((servicio: any) => {
    const searchTerm = servicioFilters.searchServicios?.toLowerCase() || "";
    
    // Búsqueda por nombre o abreviatura
    const matchSearch =
      servicio.name?.toLowerCase().includes(searchTerm) ||
      servicio.abbreviation?.toLowerCase().includes(searchTerm);

    // Filtro por tipo (si existe en specialty o alguna otra propiedad)
    // Por ahora solo filtramos por búsqueda ya que no hay un campo "tipo" explícito
    const matchTipo = servicioFilters.tipoServicio === "todos";

    return matchSearch && matchTipo;
  });

  return (
    <>
      {/* Filtros de Servicios */}
      <Tabs
        activeKey={servicioFilters.tipoServicio}
        onChange={(key) => setServicioFilter("tipoServicio", key)}
        items={[
          { label: "Todos", key: "todos" },
        ]}
        style={{ marginBottom: 16 }}
      />

      {/* Lista de Servicios */}
      {isLoading ? (
        <div style={{ textAlign: "center", padding: "40px 0" }}>
          <Spin size="large" />
        </div>
      ) : (
        <ProList<any>
          rowKey="id"
          headerTitle={
            <Typography.Title level={5}>Lista de servicios</Typography.Title>
          }
          dataSource={serviciosFiltrados}
          pagination={{
            current: servicioFilters.pageServicio,
            pageSize: servicioFilters.pageSizeServicio,
            onChange: (page, pageSize) =>
              setServicioFilters({
                pageServicio: page,
                pageSizeServicio: pageSize,
              }),
            showSizeChanger: true,
            pageSizeOptions: ["5", "10", "20"],
          }}
          search={{
            filterType: "light",
          }}
          metas={{
            title: {
              dataIndex: "name",
              search: true,
              render: (_, record) => (
                <Space>
                  <Tag color="blue">
                    {record.abbreviation || "N/A"}
                  </Tag>
                  <Typography.Text strong>{record.name || "Sin nombre"}</Typography.Text>
                </Space>
              ),
            },
            description: {
              render: (_, record) => {
                // Mostrar specialty si existe
                const specialty = record.specialty?.[0]?.coding?.[0]?.display || 
                                record.specialty?.[0]?.text || 
                                "Sin especialidad";
                return (
                  <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                    {specialty}
                  </Typography.Text>
                );
              },
            },
            subTitle: {
              render: (_, record) => (
                <Typography.Text strong style={{ fontSize: 16, color: '#1890ff' }}>
                  L.{(record.cost || 0).toFixed(2)}
                </Typography.Text>
              ),
            },
          }}
          onItem={(record: any) => ({
            onClick: () => handleSelectServicio(record),
            style: {
              cursor: "pointer",
              border:
                selectedServicio?.id === record.id
                  ? "2px solid #1890ff"
                  : "1px solid #f0f0f0",
              borderRadius: 4,
              marginBottom: 8,
              transition: "all 0.3s",
              backgroundColor:
                selectedServicio?.id === record.id
                  ? "#e6f7ff"
                  : "white",
            },
          })}
        />
      )}
    </>
  );
};

// ============================================
// Componente ListPatient actualizado
// ============================================

import { Avatar, Col, Input, Row, Select } from "antd";
import { ManOutlined, WomanOutlined, UserOutlined } from "@ant-design/icons";

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
    
    const matchSearch = paciente.nombre
      ?.toLowerCase()
      .includes(searchTerm);
    
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
    
    return matchSearch && matchGenero && matchNacionalidad && matchIdentificador;
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
          pagination={{
            current: pacienteFilters.pagePaciente,
            pageSize: pacienteFilters.pageSizePaciente,
            onChange: (page, pageSize) => 
              setPacienteFilters({ 
                pagePaciente: page, 
                pageSizePaciente: pageSize 
              }),
            showSizeChanger: true,
            pageSizeOptions: ["5", "10", "20"],
          }}
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
                <Typography.Text strong>{text || "Desconocido"}</Typography.Text>
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
              borderRadius: 4,
              marginBottom: 8,
              transition: "all 0.3s",
              backgroundColor:
                selectedPaciente?.id === record.id
                  ? "#e6f7ff"
                  : "white",
            },
          })}
        />
      )}
    </>
  );
};