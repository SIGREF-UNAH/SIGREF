import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  CopyOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { Button, Descriptions, Divider, Modal, Progress, Tag } from "antd";

interface Props {
  modalDetalleVisible: boolean;
  setModalDetalleVisible: (visible: boolean) => void;
  empleadoSeleccionado: {
    responsable: string;
    cargo: string;
    estado: string;
    confiabilidad: number;
    fecha: string;
    turno: string;
    ubicacion: string;
    apertura: string;
    cierre: string;
  } | null;
  copiarNombre: (nombre: string) => void;
  getConfiabilidadColor: (confiabilidad: number) => string;
}

export const EmployeeDetailsModal = ({
  modalDetalleVisible,
  setModalDetalleVisible,
  empleadoSeleccionado,
  copiarNombre,
  getConfiabilidadColor,
}: Props) => {
  return (
    <Modal
      title={
        <div className="flex items-center gap-2">
          <UserOutlined className="text-blue-500" />
          <span>Detalles del Auxiliar de Caja</span>
        </div>
      }
      open={modalDetalleVisible}
      onCancel={() => setModalDetalleVisible(false)}
      footer={[
        <Button key="close" onClick={() => setModalDetalleVisible(false)}>
          Cerrar
        </Button>,
      ]}
      width={600}
    >
      {empleadoSeleccionado && (
        <div>
          <Descriptions bordered column={1} size="small">
            <Descriptions.Item label="Nombre del Empleado">
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
                  Copiar
                </Button>
              </div>
            </Descriptions.Item>
            <Descriptions.Item label="Cargo">
              {empleadoSeleccionado.cargo}
            </Descriptions.Item>
            <Descriptions.Item label="Estado">
              <Tag
                color={
                  empleadoSeleccionado.estado === "Activo"
                    ? "success"
                    : "default"
                }
                icon={
                  empleadoSeleccionado.estado === "Activo" ? (
                    <CheckCircleOutlined />
                  ) : (
                    <CloseCircleOutlined />
                  )
                }
              >
                {empleadoSeleccionado.estado}
              </Tag>
            </Descriptions.Item>
          </Descriptions>

          <Divider />

          <div className="mb-2">
            <span className="font-semibold text-gray-700">
              Índice de Confiabilidad
            </span>
          </div>
          <Progress
            percent={empleadoSeleccionado.confiabilidad}
            strokeColor={getConfiabilidadColor(
              empleadoSeleccionado.confiabilidad
            )}
            format={(percent) => `${percent}%`}
            size="default"
          />

          <Divider />

          <div className="bg-gray-50 p-4 rounded">
            <h4 className="font-semibold mb-3 text-gray-700">
              Información del Cierre
            </h4>
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div>
                <span className="text-gray-600">Fecha:</span>
                <p className="font-semibold">{empleadoSeleccionado.fecha}</p>
              </div>
              <div>
                <span className="text-gray-600">Turno:</span>
                <p className="font-semibold">{empleadoSeleccionado.turno}</p>
              </div>
              <div>
                <span className="text-gray-600">Ubicación:</span>
                <p className="font-semibold">
                  {empleadoSeleccionado.ubicacion}
                </p>
              </div>
              <div>
                <span className="text-gray-600">Horario:</span>
                <p className="font-semibold">
                  {empleadoSeleccionado.apertura} -{" "}
                  {empleadoSeleccionado.cierre}
                </p>
              </div>
            </div>
          </div>
        </div>
      )}
    </Modal>
  );
};
