import {
  EditOutlined,
  EnvironmentOutlined,
  PhoneOutlined,
  MailOutlined,
  GlobalOutlined,
  BankOutlined,
  DollarOutlined,
  UserOutlined,
  UserSwitchOutlined,
  IdcardOutlined,
  ReconciliationOutlined,
} from "@ant-design/icons";
import React from "react";
import { Button, Descriptions, Space, Spin, Result, Typography } from "antd";
import useHospitalDetails from "../hooks/useHospitalDetails";

const { Title, Text } = Typography;

export const HospitalDetailsPage: React.FC = () => {
  const { hospital, isLoading, isError, error, navigate } = useHospitalDetails();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" />
      </div>
    );
  }

  if (isError) {
    // Si no hay información registrada
    if (error?.status === 404 || !hospital) {
      return (
        <div className="flex flex-col gap-3">
          <div className="text-center">
            <Title level={2} className="mb-2">
              Información del Hospital
            </Title>
            <Text type="secondary" className="text-base">
              Actualmente no existe información registrada del hospital. <br />
              Por favor, complete el formulario para definir los datos
              institucionales.
            </Text>
          </div>
          <div className="text-center">
              <Button
                type="link"
                size="large"
                icon={<BankOutlined />}
                onClick={() => navigate("/hospital/create")}
              >
                Registrar Información del Hospital
              </Button>
          </div>
        </div>
      );
    }

    // Otro tipo de error
    return (
      <div className="max-w-4xl mx-auto p-6">
        <Result
          status="error"
          title="Error al cargar la información"
          subTitle="Ocurrió un error al cargar los datos del hospital. Por favor, intente nuevamente."
          extra={
            <Button type="primary" onClick={() => window.location.reload()}>
              Reintentar
            </Button>
          }
        />
      </div>
    );
  }

  return (
    <div className="primary-card">
      <div>
        {/* Encabezado */}
        <div className="flex items-center justify-between flex-wrap gap-4 mb-4">
          <Space>
            <span className="text-2xl font-bold">Información del Hospital</span>
          </Space>
          <Button
            type="primary"
            icon={<EditOutlined />}
            onClick={() => navigate("/hospital/update")}
          >
            Editar Información
          </Button>
        </div>

        {/* Información del Hospital */}
        <Descriptions bordered column={2} size="middle">
          <Descriptions.Item
            label={
              <>
                <BankOutlined /> Nombre del Hospital
              </>
            }
            span={2}
          >
            <span className="font-semibold text-lg">{hospital?.name}</span>
          </Descriptions.Item>

          <Descriptions.Item
            label={
              <>
                <ReconciliationOutlined /> Código
              </>
            }
          >
            <span className="font-mono bg-blue-50 px-2 py-1 rounded">
              {hospital?.hospitalCode}
            </span>
          </Descriptions.Item>

          <Descriptions.Item
            label={
              <>
                <IdcardOutlined /> RTN
              </>
            }
          >
            <span className="font-mono bg-blue-50 px-2 py-1 rounded">
              {hospital?.rtn}
            </span>
          </Descriptions.Item>

          <Descriptions.Item
            label={
              <>
                <UserOutlined /> Director/a
              </>
            }
          >
            {hospital?.director}
          </Descriptions.Item>

          <Descriptions.Item
            label={
              <>
                <UserSwitchOutlined /> Subdirector/a
              </>
            }
          >
            {hospital?.subdirector}
          </Descriptions.Item>

          <Descriptions.Item
            label={
              <>
                <PhoneOutlined /> Teléfono
              </>
            }
          >
            {hospital?.phoneNumber}
          </Descriptions.Item>

          <Descriptions.Item
            label={
              <>
                <MailOutlined /> Correo
              </>
            }
          >
            <a href={`mailto:${hospital?.email}`} className="text-blue-600">
              {hospital?.email}
            </a>
          </Descriptions.Item>

          <Descriptions.Item
            label={
              <>
                <GlobalOutlined /> Sitio Web
              </>
            }
          >
            <a
              href={`https://${hospital?.website}`}
              target="_blank"
              rel="noopener noreferrer"
              className="text-blue-600"
            >
              {hospital?.website}
            </a>
          </Descriptions.Item>

          <Descriptions.Item
            label={
              <>
                <DollarOutlined /> Moneda
              </>
            }
          >
            <span className="font-semibold">{hospital?.currency}</span>
          </Descriptions.Item>
          
          <Descriptions.Item
            label={
              <>
                <EnvironmentOutlined /> Ubicación
              </>
            }
            span={2}
          >
            {hospital?.ubication}
          </Descriptions.Item>
        </Descriptions>
      </div>
    </div>
  );
};
