import { CopyOutlined, WarningOutlined } from "@ant-design/icons";
import { Alert, Button, Card, Descriptions, Modal, Tag } from "antd";

interface Props {
  setModalAdvertenciaVisible: (visible: boolean) => void;
  modalAdvertenciaVisible: boolean;
  empleadoSeleccionado: {
    responsable: string;
    cargo: string;
    turno: string;
    ubicacion: string;
    fecha: string;
    motivoError?: string;
    montoSistema: number;
    montoFisico: number;
    diferencia: number;
  } | null;
  copiarNombre: (nombre: string) => void;
}

export const ErrorClosingModal = ({
  setModalAdvertenciaVisible,
  modalAdvertenciaVisible,
  empleadoSeleccionado,
  copiarNombre,
}: Props) => {
  return (
    <Modal
      title={
        <div className="flex items-center gap-2">
          <WarningOutlined className="text-orange-500" />
          <span>Advertencia - Cierre con Diferencias</span>
        </div>
      }
      open={modalAdvertenciaVisible}
      onCancel={() => setModalAdvertenciaVisible(false)}
      footer={[
        <Button
          key="close"
          type="primary"
          onClick={() => setModalAdvertenciaVisible(false)}
        >
          Entendido
        </Button>,
      ]}
      width={650}
    >
      {empleadoSeleccionado && (
        <div>
          <Alert
            message="El monto no cuadró correctamente"
            description="Se detectó una diferencia entre el dinero en sistema y el dinero físico en caja. Revise los detalles a continuación."
            type="error"
            showIcon
            className="mb-4"
          />

          <Card className="mb-4 bg-red-50 border-red-200">
            <div className="grid grid-cols-3 gap-4 text-center">
              <div>
                <p className="text-gray-600 text-sm mb-1">Monto en Sistema</p>
                <p className="text-xl font-bold text-blue-600">
                  L. {empleadoSeleccionado.montoSistema?.toLocaleString()}.00
                </p>
              </div>
              <div>
                <p className="text-gray-600 text-sm mb-1">Monto Físico</p>
                <p className="text-xl font-bold text-green-600">
                  L. {empleadoSeleccionado.montoFisico?.toLocaleString()}.00
                </p>
              </div>
              <div>
                <p className="text-gray-600 text-sm mb-1">Diferencia</p>
                <p
                  className={`text-xl font-bold ${
                    empleadoSeleccionado.diferencia < 0
                      ? "text-red-600"
                      : "text-orange-600"
                  }`}
                >
                  L. {empleadoSeleccionado.diferencia?.toLocaleString()}.00
                </p>
              </div>
            </div>
          </Card>

          <Descriptions bordered column={1} size="small" className="mb-4">
            <Descriptions.Item label="Responsable del Cierre">
              <div className="flex items-center justify-between">
                <span className="font-semibold">
                  {empleadoSeleccionado.responsable}
                </span>
                <Button
                  type="link"
                  size="small"
                  icon={<CopyOutlined />}
                  onClick={() => copiarNombre(empleadoSeleccionado.responsable)}
                >
                  Copiar nombre
                </Button>
              </div>
            </Descriptions.Item>
            <Descriptions.Item label="Cargo">
              {empleadoSeleccionado.cargo}
            </Descriptions.Item>
            <Descriptions.Item label="Turno">
              {empleadoSeleccionado.turno}
            </Descriptions.Item>
            <Descriptions.Item label="Ubicación">
              {empleadoSeleccionado.ubicacion}
            </Descriptions.Item>
            <Descriptions.Item label="Fecha del Cierre">
              {empleadoSeleccionado.fecha}
            </Descriptions.Item>
            <Descriptions.Item label="Motivo del Error">
              <Tag color="error">{empleadoSeleccionado.motivoError}</Tag>
            </Descriptions.Item>
          </Descriptions>

          <Alert
            message="Acciones Recomendadas"
            description={
              <ul className="list-disc list-inside space-y-1 mt-2">
                <li>Revisar transacciones del período del cierre</li>
                <li>Verificar comprobantes físicos de pago</li>
                <li>Contactar al auxiliar de caja para aclaración</li>
                {empleadoSeleccionado.diferencia < 0 && (
                  <li className="text-red-600 font-semibold">
                    Faltante de dinero - Requiere investigación inmediata
                  </li>
                )}
              </ul>
            }
            type="warning"
            showIcon
          />
        </div>
      )}
    </Modal>
  );
};
