import { Modal, Descriptions, Tag, Spin } from "antd";
import type { ShiftDto } from "../../../api/models";

interface ShiftDetailsModalProps {
  open: boolean;
  onClose: () => void;
  shift: ShiftDto | null;
  loading?: boolean;
}

export const ShiftDetailsModal = ({
  open,
  onClose,
  shift,
  loading = false,
}: ShiftDetailsModalProps) => {
  if (!shift && !loading) {
    return null;
  }

  return (
    <Modal
      title={
        <div className="text-xl font-semibold text-general-primary flex items-center gap-3">
          Detalles del Turno
        </div>
      }
      open={open}
      onCancel={onClose}
      footer={null}
      width={650}
      centered
      destroyOnClose
      closeIcon={loading ? false : undefined}
    >
      {loading ? (
        <div className="flex justify-center py-12">
          <Spin size="large" />
        </div>
      ) : (
        <div className="mt-4">
          <Descriptions bordered column={1} size="middle">
            <Descriptions.Item label="Nombre del Turno" span={1}>
              <span className="font-medium text-gray-900">
                {shift?.name || "—"}
              </span>
            </Descriptions.Item>

            <Descriptions.Item label="Área / Ubicación">
              <span className="font-medium text-blue-600">
                {shift?.nameLocation || "Sin área asignada"}
              </span>
            </Descriptions.Item>

            <Descriptions.Item label="Horario">
              <div className="flex items-center gap-4">
                <div className="flex items-center gap-2">
                  <span className="text-gray-500">Inicio:</span>
                  <Tag color="blue" className="text-lg font-semibold">
                    {shift?.startTime ? shift.startTime.substring(0, 5) : "—"}
                  </Tag>
                </div>
                <span className="text-gray-400">→</span>
                <div className="flex items-center gap-2">
                  <span className="text-gray-500">Fin:</span>
                  <Tag color="purple" className="text-lg font-semibold">
                    {shift?.endTime ? shift.endTime.substring(0, 5) : "—"}
                  </Tag>
                </div>
              </div>
            </Descriptions.Item>

            <Descriptions.Item label="Estado">
              <Tag
                color={shift?.isActive ? "green" : "red"}
                className="text-base px-4 py-1"
              >
                {shift?.isActive ? "ACTIVO" : "INACTIVO"}
              </Tag>
            </Descriptions.Item>
          </Descriptions>
        </div>
      )}
    </Modal>
  );
};
