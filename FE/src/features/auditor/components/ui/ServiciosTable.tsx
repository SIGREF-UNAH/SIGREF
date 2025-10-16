import React, { useState } from "react";
import { Table, Button, Space, Input } from "antd";
import { SearchOutlined, EditOutlined, DeleteOutlined } from "@ant-design/icons";

export interface Servicio {
  id: string;
  abreviatura: string;
  nombre: string;
  areaAsistencial: string;
  costo: number;
}

interface ServiciosTableProps {
  servicios: Servicio[];
  loading: boolean;
}

export const ServiciosTable: React.FC<ServiciosTableProps> = ({ servicios, loading }) => {
  const [searchText, setSearchText] = useState("");

  const handleSearch = (value: string) => {
    setSearchText(value);
  };

  const filteredServicios = servicios.filter(
    (servicio) =>
      servicio.nombre.toLowerCase().includes(searchText.toLowerCase()) ||
      servicio.abreviatura.toLowerCase().includes(searchText.toLowerCase()) ||
      servicio.areaAsistencial.toLowerCase().includes(searchText.toLowerCase())
  );

  const columns = [
    {
      title: "Abreviatura",
      dataIndex: "abreviatura",
      key: "abreviatura",
    },
    {
      title: "Nombre",
      dataIndex: "nombre",
      key: "nombre",
    },
    {
      title: "Área Asistencial",
      dataIndex: "areaAsistencial",
      key: "areaAsistencial",
    },
    {
      title: "Costo",
      dataIndex: "costo",
      key: "costo",
      render: (costo: number) => `L. ${costo.toFixed(2)}`,
    },
    {
      title: "Acciones",
      key: "acciones",
      render: (_: any, record: Servicio) => (
        <Space size="middle">
          <Button type="text" icon={<EditOutlined />} />
          <Button type="text" danger icon={<DeleteOutlined />} />
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div className="flex justify-between mb-4">
        <Input
          placeholder="Buscar por nombre o abreviatura"
          prefix={<SearchOutlined />}
          onChange={(e) => handleSearch(e.target.value)}
          style={{ width: 300 }}
        />
      </div>
      
      <Table
        columns={columns}
        dataSource={filteredServicios}
        rowKey="id"
        loading={loading}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50'],
          showTotal: (total) => `1-${total} de ${total}`,
        }}
      />
    </div>
  );
};