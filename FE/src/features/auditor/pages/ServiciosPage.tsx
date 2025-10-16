import React from "react";
import { Button } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { ServiciosTable } from "../components";
import { useServicios } from "../hooks";

export const ServiciosPage: React.FC = () => {
  const { servicios, loading } = useServicios();

  return (
    <div className="p-4">
      <div className="flex flex-col">
        <div className="mb-4">
          <h1 className="text-2xl font-bold mb-1">Gestión de Servicios</h1>
          <p className="text-gray-600">Genere, visualice y edite los servicios disponibles que ofrece el hospital</p>
        </div>
        
        <div className="flex justify-end mb-4">
          <Button type="primary" icon={<PlusOutlined />}>
            Nuevo Servicio
          </Button>
        </div>
        
        <ServiciosTable servicios={servicios} loading={loading} />
      </div>
    </div>
  );
};