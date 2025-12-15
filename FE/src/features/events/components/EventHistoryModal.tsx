import {
  ApiOutlined,
  ClockCircleOutlined,
  CodeOutlined,
  EyeOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { ProDescriptions } from "@ant-design/pro-components";
import { Button, Divider, Modal, Space, Tag, Alert } from "antd";
import type { AuditLogDto } from "../../../api/models";

interface Props {
  modalOpen: boolean;
  selectedRecord: AuditLogDto | null;
  setModalOpen: (open: boolean) => void;
  getStatusColor: (statusCode: number | undefined) => string;
  getActionColor: (action: string | null | undefined) => string;
  dayjs: (date: string | Date) => any;
}

export const EventHistoryModal = ({
  modalOpen,
  selectedRecord,
  setModalOpen,
  getStatusColor,
  getActionColor,
  dayjs,
}: Props) => {
  const formatJson = (data: any) => {
    try {
      if (typeof data === "string") {
        return JSON.stringify(JSON.parse(data), null, 2);
      }
      return JSON.stringify(data, null, 2);
    } catch {
      return JSON.stringify(data, null, 2);
    }
  };

  return (
    <Modal
      title={
        <Space>
          <EyeOutlined />
          <span>Detalles del Evento</span>
        </Space>
      }
      open={modalOpen}
      onCancel={() => setModalOpen(false)}
      footer={[
        <Button key="close" type="primary" onClick={() => setModalOpen(false)}>
          Cerrar
        </Button>,
      ]}
      width={900}
    >
      {selectedRecord && (
        <>
          {/* Estado del evento */}
          <div style={{ marginBottom: 16 }}>
            <Alert
              message={
                <Space>
                  {selectedRecord.success ? (
                    <CheckCircleOutlined />
                  ) : (
                    <CloseCircleOutlined />
                  )}
                  <span>
                    {selectedRecord.success
                      ? "Operación Exitosa"
                      : "Operación Fallida"}
                  </span>
                </Space>
              }
              type={selectedRecord.success ? "success" : "error"}
              showIcon={false}
            />
          </div>

          {/* Información del Usuario */}
          <Divider orientation="left">
            <UserOutlined /> Información del Usuario
          </Divider>
          <ProDescriptions column={2}>
            <ProDescriptions.Item label="Nombre de Usuario">
              <strong>{(selectedRecord as any).userName || "N/A"}</strong>
            </ProDescriptions.Item>
            <ProDescriptions.Item label="ID de Usuario">
              <code>{selectedRecord.userId || "N/A"}</code>
            </ProDescriptions.Item>
          </ProDescriptions>

          {/* Información de la Acción */}
          <Divider orientation="left">
            <ApiOutlined /> Información de la Acción
          </Divider>
          <ProDescriptions column={2}>
            <ProDescriptions.Item label="Acción">
              <Tag color={getActionColor(selectedRecord.action)}>
                {selectedRecord.action || "N/A"}
              </Tag>
            </ProDescriptions.Item>
            <ProDescriptions.Item label="Código de Estado">
              <Tag color={getStatusColor(selectedRecord.statusCode)}>
                {selectedRecord.statusCode || "N/A"}
              </Tag>
            </ProDescriptions.Item>
            {selectedRecord.resourceType && (
              <ProDescriptions.Item label="Tipo de Recurso" span={2}>
                {selectedRecord.resourceType}
              </ProDescriptions.Item>
            )}
            {selectedRecord.resourceId && (
              <ProDescriptions.Item label="ID de Recurso" span={2}>
                <code>{selectedRecord.resourceId}</code>
              </ProDescriptions.Item>
            )}
            {selectedRecord.errorMessage && (
              <ProDescriptions.Item label="Mensaje de Error" span={2}>
                <Tag color="error">{selectedRecord.errorMessage}</Tag>
              </ProDescriptions.Item>
            )}
          </ProDescriptions>

          {/* Información Técnica */}
          <Divider orientation="left">
            <CodeOutlined /> Información Técnica
          </Divider>
          <ProDescriptions column={2}>
            <ProDescriptions.Item label="Endpoint" span={2}>
              <Tag color="blue">{selectedRecord.httpMethod}</Tag>{" "}
              <code>{selectedRecord.endpoint}</code>
            </ProDescriptions.Item>
            <ProDescriptions.Item
              label={
                <Space>
                  <ClockCircleOutlined />
                  Fecha y Hora
                </Space>
              }
              span={2}
            >
              {selectedRecord.timestamp
                ? dayjs(selectedRecord.timestamp).format("DD/MM/YYYY HH:mm:ss")
                : "N/A"}
            </ProDescriptions.Item>
          </ProDescriptions>

          {/* Data Before */}
          {selectedRecord.dataBefore && (
            <>
              <Divider orientation="left">Datos Anteriores</Divider>
              <ProDescriptions column={1}>
                <ProDescriptions.Item>
                  <pre
                    style={{
                      background: "#f5f5f5",
                      padding: "12px",
                      borderRadius: "4px",
                      fontSize: "12px",
                      overflow: "auto",
                      maxHeight: "300px",
                      margin: 0,
                    }}
                  >
                    {formatJson(selectedRecord.dataBefore)}
                  </pre>
                </ProDescriptions.Item>
              </ProDescriptions>
            </>
          )}

          {/* Data After */}
          {selectedRecord.dataAfter && (
            <>
              <Divider orientation="left">Datos Posteriores</Divider>
              <ProDescriptions column={1}>
                <ProDescriptions.Item>
                  <pre
                    style={{
                      background: "#f5f5f5",
                      padding: "12px",
                      borderRadius: "4px",
                      fontSize: "12px",
                      overflow: "auto",
                      maxHeight: "300px",
                      margin: 0,
                    }}
                  >
                    {formatJson(selectedRecord.dataAfter)}
                  </pre>
                </ProDescriptions.Item>
              </ProDescriptions>
            </>
          )}

          {/* Additional Info */}
          {selectedRecord.additionalInfo &&
            Object.keys(selectedRecord.additionalInfo).length > 0 && (
              <>
                <Divider orientation="left">Información Adicional</Divider>
                <ProDescriptions column={1}>
                  <ProDescriptions.Item>
                    <pre
                      style={{
                        background: "#f5f5f5",
                        padding: "12px",
                        borderRadius: "4px",
                        fontSize: "12px",
                        overflow: "auto",
                        maxHeight: "200px",
                        margin: 0,
                      }}
                    >
                      {formatJson(selectedRecord.additionalInfo)}
                    </pre>
                  </ProDescriptions.Item>
                </ProDescriptions>
              </>
            )}

          {/* Identificador */}
          <Divider orientation="left">Identificador</Divider>
          <ProDescriptions column={1}>
            <ProDescriptions.Item label="ID del Registro">
              <code>{selectedRecord.id || "N/A"}</code>
            </ProDescriptions.Item>
          </ProDescriptions>
        </>
      )}
    </Modal>
  );
};