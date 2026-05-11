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
  PictureOutlined,
} from "@ant-design/icons";

import { useState } from "react";
import { Button, Descriptions, Space, Spin, Result, Typography, Image, Card } from "antd";
import useHospitalDetails from "../hooks/useHospitalDetails";
import { LogoSelectorModal } from "../../media-files/components";
import { getMediaUrl } from "../../media-files/utils";
import { useAbility } from "../../../config";

const { Title, Text } = Typography;

export const HospitalDetailsPage = () => {
  const { hospital, isLoading, isError, navigate } = useHospitalDetails();
  const [logoModalOpen, setLogoModalOpen] = useState(false);
  const ability = useAbility();

  // Pantalla de carga
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" />
      </div>
    );
  }

  // Error o sin datos (404)
  if (isError || !hospital) {
    return (
      <div className="flex flex-col gap-4 max-w-3xl mx-auto">
        <div className="text-center">
          <Title level={2} className="mb-2">
            Información del Hospital
          </Title>
          <Text type="secondary" className="text-base">
            No existe información registrada del hospital. <br />
            Complete el formulario para definir los datos institucionales.
          </Text>
        </div>

        <div className="text-center">
          {ability.can("create", "hospital") && (
            <Button
              type="primary"
              icon={<BankOutlined />}
              size="large"
              onClick={() => navigate("/hospital/create")}
            >
              Registrar Información
            </Button>
          )}
        </div>
      </div>
    );
  }

  // Datos del hospital (no nulo a partir de aquí)
  const logoUrl = hospital.urlLogo ? getMediaUrl(hospital.urlLogo) : null;
  const logoHealthUrl = hospital.urlLogoHealth ? getMediaUrl(hospital.urlLogoHealth) : null;

  // Helper para verificar si un valor es válido
  const has = (v: any): boolean => v !== null && v !== undefined && v !== "";

  return (
    <div className="max-w-5xl mx-auto fade-in">
      <div className="primary-card">
        {/* Encabezado */}
        <div className="flex items-center justify-between flex-wrap gap-4 mb-6">
          <Title level={3} className="mb-0! text-gray-700">
            Información del Hospital
          </Title>

          {ability.can("update", "hospital") && (
            <Space>
              <Button
                type="default"
                icon={<PictureOutlined />}
                onClick={() => setLogoModalOpen(true)}
              >
                Logotipos
              </Button>

              <Button
                type="primary"
                icon={<EditOutlined />}
                onClick={() => navigate("/hospital/update")}
              >
                Editar
              </Button>
            </Space>
          )}
        </div>

        {/* Logos */}
        {(logoUrl || logoHealthUrl) && (
          <div className="mb-6 grid grid-cols-1 md:grid-cols-2 gap-4">
            {logoUrl && (
              <Card className="shadow-sm text-center py-4 border-gray-100">
                <Title level={5} className="text-gray-500 pb-2!">
                  Logo del Hospital
                </Title>
                <Image src={logoUrl} alt="Logo" height={120} preview />
              </Card>
            )}

            {logoHealthUrl && (
              <Card className="shadow-sm text-center py-4 border-gray-100">
                <Title level={5} className="text-gray-500 pb-2!">
                  Logo de Salud
                </Title>
                <Image src={logoHealthUrl} alt="Logo Salud" height={120} preview />
              </Card>
            )}
          </div>
        )}

        {/* Información */}
        <Descriptions
          bordered
          column={2}
          size="middle"
          labelStyle={{ fontWeight: 600 }}
          className="rounded-xl overflow-hidden"
        >
          {/* Nombre */}
          {has(hospital.name) && (
            <Descriptions.Item
              label={
                <Space>
                  <BankOutlined />
                  Nombre
                </Space>
              }
              span={2}
            >
              <span className="font-semibold text-lg">{hospital.name}</span>
            </Descriptions.Item>
          )}

          {/* Código */}
          {has(hospital.hospitalCode) && (
            <Descriptions.Item
              label={
                <Space>
                  <ReconciliationOutlined />
                  Código
                </Space>
              }
            >
              <span className="font-mono bg-blue-50 px-2 py-1 rounded">
                {hospital.hospitalCode}
              </span>
            </Descriptions.Item>
          )}

          {/* RTN */}
          {has(hospital.rtn) && (
            <Descriptions.Item
              label={
                <Space>
                  <IdcardOutlined />
                  RTN
                </Space>
              }
            >
              <span className="font-mono bg-blue-50 px-2 py-1 rounded">
                {hospital.rtn}
              </span>
            </Descriptions.Item>
          )}

          {/* Director */}
          {has(hospital.director) && (
            <Descriptions.Item
              label={
                <Space>
                  <UserOutlined />
                  Director/a
                </Space>
              }
            >
              {hospital.director}
            </Descriptions.Item>
          )}

          {/* Subdirector */}
          {has(hospital.subdirector) && (
            <Descriptions.Item
              label={
                <Space>
                  <UserSwitchOutlined />
                  Subdirector/a
                </Space>
              }
            >
              {hospital.subdirector}
            </Descriptions.Item>
          )}

          {/* Teléfono */}
          {has(hospital.phoneNumber) && (
            <Descriptions.Item
              label={
                <Space>
                  <PhoneOutlined />
                  Teléfono
                </Space>
              }
            >
              {hospital.phoneNumber}
            </Descriptions.Item>
          )}

          {/* Correo */}
          {has(hospital.email) && (
            <Descriptions.Item
              label={
                <Space>
                  <MailOutlined />
                  Correo
                </Space>
              }
            >
              <a href={`mailto:${hospital.email}`} className="text-blue-600">
                {hospital.email}
              </a>
            </Descriptions.Item>
          )}

          {/* Sitio Web */}
          {has(hospital.website) && (
            <Descriptions.Item
              label={
                <Space>
                  <GlobalOutlined />
                  Sitio Web
                </Space>
              }
            >
              <a
                href={`https://${hospital.website}`}
                target="_blank"
                rel="noopener noreferrer"
                className="text-blue-600"
              >
                {hospital.website}
              </a>
            </Descriptions.Item>
          )}

          {/* Moneda */}
          {has(hospital.currency) && (
            <Descriptions.Item
              label={
                <Space>
                  <DollarOutlined />
                  Moneda
                </Space>
              }
            >
              <strong>{hospital.currency}</strong>
            </Descriptions.Item>
          )}

          {/* Ubicación */}
          {has(hospital.location) && (
            <Descriptions.Item
              label={
                <Space>
                  <EnvironmentOutlined />
                  Ubicación
                </Space>
              }
              span={2}
            >
              {hospital.location}
            </Descriptions.Item>
          )}
        </Descriptions>
      </div>

      {/* Modal de logotipos */}
      <LogoSelectorModal
        open={logoModalOpen}
        onClose={() => setLogoModalOpen(false)}
      />
    </div>
  );
};