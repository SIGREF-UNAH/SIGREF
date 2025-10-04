import { Table, Button, Input, Select, Space } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { Healthcare } from "../store";
import { useHealthcares } from "../hooks";
import {
  EditOutlined,
  DeleteOutlined,
  PlusOutlined,
  FilterOutlined,
} from "@ant-design/icons";

const { Search } = Input;

export const HealthcaresPage = () => {
  const {
    filters,
    departments,
    filteredData,
    paginationConfig,
    handleCreate,
    handleEdit,
    handleDelete,
    setFilter,
  } = useHealthcares();

  // Columnas de la tabla
  const columns: ColumnsType<Healthcare> = [
    {
      title: "Abreviatura",
      dataIndex: "abbreviation",
      key: "abbreviation",
      width: 120,
    },
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
      width: 200,
    },
    {
      title: "Area Asistencial",
      key: "area",
      width: 200,
      render: (_, record) => record.location?.[0]?.display || "-",
    },
    {
      title: "Costo",
      dataIndex: "cost",
      key: "cost",
      width: 120,
      render: (cost: number) => `L. ${cost.toFixed(2)}`,
    },
    {
      title: "Acciones",
      key: "actions",
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record.id)}
          />
          <Button
            type="text"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDelete(record.id)}
          />
        </Space>
      ),
    },
  ];

  return (
    <div>
      {/* Encabezado */}
      <div className="flex mb-4 items-start justify-between">
        <div>
          <h1 className="text-general-primary text-3xl font-bold mb-1">
            Gestión de Servicios
          </h1>
          <p className="text-general-secondary">
            Visualice, cree y edite los servicios médicos disponibles que ofrece
            el hospital.
          </p>
        </div>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleCreate}
          size="large"
          style={{ backgroundColor: "var(--color-primary)" }}
        >
          Nuevo Servicio
        </Button>
      </div>

      {/* Contenido */}
      <div className="p-4 border-2 bg-card border-primary shadow-md rounded-lg">
        {/* Busqueda y filtros */}
        <div className="flex justify-end gap-3 mb-4">
          <Search
            placeholder="Buscar por nombre o abreviatura"
            allowClear
            style={{ width: 300 }}
            value={filters.search}
            onChange={(e) => setFilter("search", e.target.value)}
            onSearch={(value) => setFilter("search", value)}
          />
          <Select
            placeholder="Por Departamento"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 200, height: 36 }}
            value={filters.department}
            onChange={(value) => setFilter("department", value)}
            options={departments.map((dept) => ({ label: dept, value: dept }))}
          />
        </div>

        {/* Lista de servicios */}
        <Table
          columns={columns}
          dataSource={filteredData}
          rowKey="id"
          pagination={paginationConfig}
          bordered
        />
      </div>
    </div>
  );
};
