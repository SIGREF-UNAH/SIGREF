// TODO: Organizacion no muestra todos los elementos de identificadores existentes para este archivo, igualmente con otras props

import { Modal, Descriptions, Tag, Empty } from "antd";
import type { OrganizationDto } from "../../../api/models";
import { OrganizationTypeEnum } from "../../../api/models";

interface OrganizationDetailsModalProps {
  open: boolean;
  organization: OrganizationDto | null;
  onClose: () => void;
}

export const OrganizationDetailsModal = ({
  open,
  organization,
  onClose,
}: OrganizationDetailsModalProps) => {
  if (!organization) {
    return (
      <Modal
        title="Detalle de la Organización"
        open={open}
        onCancel={onClose}
        footer={null}
        width={700}
      >
        <Empty description="No hay datos disponibles" />
      </Modal>
    );
  }

  // Extraer contactos (teléfono y correo si existen)
  const telecom = organization.contact?.flatMap((c) => c.telecom || []) || [];
  const phone =
    telecom.find((t) => String(t.system).toLowerCase() === "phone")?.value ||
    "No registrado";
  const email =
    telecom.find((t) => String(t.system).toLowerCase() === "email")?.value ||
    "No registrado";

  // Extraer dirección (solo la primera si hay varias)
  const address = organization.contact?.[0]?.address;
  const fullAddress =
    [address?.country, address?.state, address?.city, ...(address?.line || [])]
      .filter(Boolean)
      .join(", ") || "No registrada";

  // Función para obtener el label del tipo de organización
  const getOrganizationTypeLabel = (typeValue: string): string => {
    const entry = Object.entries(OrganizationTypeEnum).find(
      ([_, value]) => value === typeValue
    );
    return entry ? entry[1] : typeValue;
  };

  // Obtener los tipos de organización
  const organizationTypes = organization.type?.map(item => {
    // Si el item tiene alguna propiedad que podamos mostrar
    const values = Object.values(item).filter(v => v !== null && v !== undefined);
    return values.length > 0 ? values.join(", ") : "No especificado";
  }) || [];

  return (
    <Modal
      title={
        <div className="text-xl font-semibold text-general">
          Detalle de la Organización
        </div>
      }
      open={open}
      onCancel={onClose}
      footer={null}
      width={800}
      centered
    >
      <div className="py-4">
        {/* Información principal */}
        <Descriptions
          bordered
          column={2}
          size="middle"
          labelStyle={{ fontWeight: 600, backgroundColor: "#fafafa" }}
        >
          <Descriptions.Item label="Nombre" span={2}>
            <span className="text-base">{organization.name}</span>
          </Descriptions.Item>

          <Descriptions.Item label="Identificador">
            <Tag color="blue" className="text-sm px-3 py-1">
              {organization.identifier?.[0]?.value || "No disponible"}
            </Tag>
          </Descriptions.Item>

          <Descriptions.Item label="Estado">
            {organization.active ? (
              <Tag color="success" className="text-sm px-3 py-1">
                ✓ Activo
              </Tag>
            ) : (
              <Tag color="error" className="text-sm px-3 py-1">
                ✗ Inactivo
              </Tag>
            )}
          </Descriptions.Item>

          {organization.type && organization.type.length > 0 && (
            <Descriptions.Item label="Tipo" span={2}>
              <div className="flex flex-wrap gap-2">
                {organizationTypes.map((type, index) => (
                  <Tag key={index} color="purple" className="text-sm px-3 py-1">
                    {getOrganizationTypeLabel(type)}
                  </Tag>
                ))}
              </div>
            </Descriptions.Item>
          )}

          <Descriptions.Item label="Teléfono">
            <span className="text-gray-700">{phone}</span>
          </Descriptions.Item>

          <Descriptions.Item label="Correo electrónico">
            <span className="text-gray-700">{email}</span>
          </Descriptions.Item>

          <Descriptions.Item label="Dirección" span={2}>
            <div className="text-gray-700">{fullAddress}</div>
          </Descriptions.Item>

          {organization.description && (
            <Descriptions.Item label="Descripción" span={2}>
              <div
                className="text-gray-700 whitespace-pre-wrap"
                dangerouslySetInnerHTML={{
                  __html: organization.description || "",
                }}
              />
            </Descriptions.Item>
          )}
        </Descriptions>

        {/* Información adicional */}
        {organization.id && (
          <div className="mt-6 pt-4 border-t border-gray-200">
            <div className="text-xs text-gray-500">
              <span className="font-medium">ID de la Organización:</span>{" "}
              <span className="font-mono">{organization.id}</span>
            </div>
            {organization.lastUpdated && (
              <div className="text-xs text-gray-500 mt-1">
                <span className="font-medium">Última actualización:</span>{" "}
                {new Date(organization.lastUpdated).toLocaleString("es-HN")}
              </div>
            )}
          </div>
        )}
      </div>
    </Modal>
  );
};