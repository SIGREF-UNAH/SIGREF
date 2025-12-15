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

import React, { useState } from "react";
import { Button, Descriptions, Space, Spin, Result, Typography, Image, Card } from "antd";
import useHospitalDetails from "../hooks/useHospitalDetails";
import { LogoSelectorModal } from "../../media-files/components";
import { getMediaUrl } from "../../media-files/utils";
import { Can } from "@casl/react";
import { useAbility } from "../../../config";

const { Title, Text } = Typography;

export const HospitalDetailsPage: React.FC = () => {
  const { hospital, isLoading, isError, error, navigate } = useHospitalDetails();
  const [logoModalOpen, setLogoModalOpen] = useState(false);
  const ability = useAbility();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" />
      </div>
    );
  }

  if (isError) {
    if (error?.status === 404 || !hospital) {
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
            <Can I="create" a="hospital" ability={ability}>
              <Button
                type="primary"
                icon={<BankOutlined />}
                size="large"
                onClick={() => navigate("/hospital/create")}
              >
                Registrar Información
              </Button>
            </Can>
          </div>
        </div>
      );
    }

    return (
      <div className="max-w-4xl mx-auto p-6">
        <Result
          status="error"
          title="Error al cargar la información"
          subTitle="Ocurrió un error al cargar los datos del hospital."
          extra={<Button type="primary" onClick={() => window.location.reload()}>Reintentar</Button>}
        />
      </div>
    );
  }

  const logoUrl = hospital?.urlLogo ? getMediaUrl(hospital.urlLogo) : null;
  const logoHealthUrl = hospital?.urlLogoHealth ? getMediaUrl(hospital.urlLogoHealth) : null;

  // Función para ocultar cualquier campo vacío
  const has = (v: any) => v !== null && v !== undefined && v !== "";

  return (
    <div className="max-w-5xl mx-auto fade-in">
      <div className="primary-card">
        {/* Encabezado */}
        <div className="flex items-center justify-between flex-wrap gap-4 mb-6">
          <Title level={3} className="mb-0! text-gray-700">
            Información del Hospital
          </Title>

          <Can I="update" a="hospital" ability={ability}>
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
          </Can>
        </div>

        {/* Logos */}
        {(logoUrl || logoHealthUrl) && (
          <div className="mb-6 grid grid-cols-1 md:grid-cols-2 gap-4">
            {logoUrl && (
              <Card className="shadow-sm text-center py-4 border-gray-100">
                <Title level={5} className="text-gray-500 pb-2!">Logo del Hospital</Title>
                <Image src={logoUrl} alt="Logo" height={120} preview />
              </Card>
            )}

            {logoHealthUrl && (
              <Card className="shadow-sm text-center py-4 border-gray-100">
                <Title level={5} className="text-gray-500 pb-2!">Logo de Salud</Title>
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
          {has(hospital?.name) && (
            <Descriptions.Item label={<><BankOutlined /> Nombre</>} span={2}>
              <span className="font-semibold text-lg">{hospital.name}</span>
            </Descriptions.Item>
          )}

          {has(hospital?.hospitalCode) && (
            <Descriptions.Item label={<><ReconciliationOutlined /> Código</>}>
              <span className="font-mono bg-blue-50 px-2 py-1 rounded">
                {hospital.hospitalCode}
              </span>
            </Descriptions.Item>
          )}

          {has(hospital?.rtn) && (
            <Descriptions.Item label={<><IdcardOutlined /> RTN</>}>
              <span className="font-mono bg-blue-50 px-2 py-1 rounded">
                {hospital.rtn}
              </span>
            </Descriptions.Item>
          )}

          {has(hospital?.director) && (
            <Descriptions.Item label={<><UserOutlined /> Director/a</>}>
              {hospital.director}
            </Descriptions.Item>
          )}

          {has(hospital?.subdirector) && (
            <Descriptions.Item label={<><UserSwitchOutlined /> Subdirector/a</>}>
              {hospital.subdirector}
            </Descriptions.Item>
          )}

          {has(hospital?.phoneNumber) && (
            <Descriptions.Item label={<><PhoneOutlined /> Teléfono</>}>
              {hospital.phoneNumber}
            </Descriptions.Item>
          )}

          {has(hospital?.email) && (
            <Descriptions.Item label={<><MailOutlined /> Correo</>}>
              <a href={`mailto:${hospital.email}`} className="text-blue-600">
                {hospital.email}
              </a>
            </Descriptions.Item>
          )}

          {has(hospital?.website) && (
            <Descriptions.Item label={<><GlobalOutlined /> Sitio Web</>}>
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

          {has(hospital?.currency) && (
            <Descriptions.Item label={<><DollarOutlined /> Moneda</>}>
              <strong>{hospital.currency}</strong>
            </Descriptions.Item>
          )}

          {has(hospital?.ubication) && (
            <Descriptions.Item label={<><EnvironmentOutlined /> Ubicación</>} span={2}>
              {hospital.ubication}
            </Descriptions.Item>
          )}
        </Descriptions>
      </div>

      {/* Modal */}
      <LogoSelectorModal open={logoModalOpen} onClose={() => setLogoModalOpen(false)} />
    </div>
  );
};
