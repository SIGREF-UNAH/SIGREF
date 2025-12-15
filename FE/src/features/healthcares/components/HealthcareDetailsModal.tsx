import { Modal, Descriptions, Tag, Empty } from "antd";
import type { HealthcareDto } from "../../../api/models";

interface HealthcareDetailModalProps {
  open: boolean;
  healthcare: HealthcareDto | null;
  onClose: () => void;
}

export const HealthcareDetailsModal = ({
  open,
  healthcare,
  onClose,
}: HealthcareDetailModalProps) => {
  if (!healthcare) {
    return (
      <Modal
        title="Detalle del Servicio"
        open={open}
        onCancel={onClose}
        footer={null}
        width={700}
      >
        <Empty description="No hay datos disponibles" />
      </Modal>
    );
  }

  return (
    <Modal
      title={
        <div className="text-xl font-semibold text-general">
          Detalle del Servicio Médico
        </div>
      }
      open={open}
      onCancel={onClose}
      footer={null}
      width={800}
      centered
    >
      <div className="py-4">
        {/* Información Principal */}
        <Descriptions
          bordered
          column={2}
          size="middle"
          labelStyle={{ fontWeight: 600, backgroundColor: "#fafafa" }}
        >
          <Descriptions.Item label="Nombre" span={2}>
            <span className="text-base">{healthcare.name}</span>
          </Descriptions.Item>

          <Descriptions.Item label="Abreviatura">
            <Tag color="blue" className="text-sm px-3 py-1">
              {healthcare.abbreviation}
            </Tag>
          </Descriptions.Item>

          <Descriptions.Item label="Estado">
            {healthcare.active ? (
              <Tag color="success" className="text-sm px-3 py-1">
                ✓ Activo
              </Tag>
            ) : (
              <Tag color="error" className="text-sm px-3 py-1">
                ✗ Inactivo
              </Tag>
            )}
          </Descriptions.Item>

          <Descriptions.Item label="Costo" span={2}>
            <span className="text-lg font-semibold text-green-600">
              L. {healthcare.cost?.toFixed(2) || "0.00"}
            </span>
          </Descriptions.Item>

          {healthcare.providedBy && (
            <Descriptions.Item label="Organización" span={2}>
              <Tag color="purple" className="text-sm px-3 py-1">
                {healthcare.providedBy.display}
              </Tag>
            </Descriptions.Item>
          )}
          
          {healthcare.comment && (
            <Descriptions.Item label="Descripción" span={2}>
              <div className="text-gray-700 whitespace-pre-wrap">
                {healthcare.comment}
              </div>
            </Descriptions.Item>
          )}
        </Descriptions>

        {/* Ubicaciones */}
        {healthcare.location && healthcare.location.length > 0 && (
          <div className="mt-6">
            <div className="mb-3 text-base font-semibold text-general">
              Ubicaciones donde se encuentra disponible este servicio:
            </div>
            <div className="border border-gray-200 rounded-lg p-4 bg-gray-50">
              <div className="flex flex-wrap gap-2">
                {healthcare.location.map((loc, index) => (
                  <Tag
                    key={index}
                    color="geekblue"
                    className="text-sm px-3 py-1 m-0"
                  >
                    📍 {loc.display}
                  </Tag>
                ))}
              </div>
            </div>
          </div>
        )}

        {/* Información adicional */}
        {healthcare.id && (
          <div className="mt-6 pt-4 border-t border-gray-200">
            <div className="text-xs text-gray-500">
              <span className="font-medium">ID del Servicio:</span>{" "}
              <span className="font-mono">{healthcare.id}</span>
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
};
