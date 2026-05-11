import {
  ApiOutlined,
  ClockCircleOutlined,
  CodeOutlined,
  EyeOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  UserOutlined,
  GlobalOutlined,
  LinkOutlined,
  FilterOutlined,
  KeyOutlined,
  InfoCircleOutlined,
} from "@ant-design/icons";
import { ProDescriptions } from "@ant-design/pro-components";
import { Button, Divider, Modal, Space, Tag, Alert } from "antd";
import type { AuditLog } from "../../../api/models/auditLog";

interface Props {
  modalOpen: boolean;
  selectedRecord: AuditLog | null;
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
              <strong>{selectedRecord.userName || "N/A"}</strong>
            </ProDescriptions.Item>
            <ProDescriptions.Item label="ID de Usuario">
              <code>{selectedRecord.userId || "N/A"}</code>
            </ProDescriptions.Item>
            {selectedRecord.ipAddress && (
              <ProDescriptions.Item label="Dirección IP" span={2}>
                <Space>
                  <GlobalOutlined />
                  <code>{selectedRecord.ipAddress}</code>
                </Space>
              </ProDescriptions.Item>
            )}
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
              <ProDescriptions.Item label="Tipo de Recurso">
                <Tag color="geekblue">{selectedRecord.resourceType}</Tag>
              </ProDescriptions.Item>
            )}
            {selectedRecord.resourceId && (
              <ProDescriptions.Item label="ID de Recurso">
                <code>{selectedRecord.resourceId}</code>
              </ProDescriptions.Item>
            )}
            {selectedRecord.httpMethod && (
              <ProDescriptions.Item label="Método HTTP">
                <Tag color="blue">{selectedRecord.httpMethod.toUpperCase()}</Tag>
              </ProDescriptions.Item>
            )}
            {selectedRecord.statusCode && (
              <ProDescriptions.Item label="Status Code">
                <code>{selectedRecord.statusCode}</code>
              </ProDescriptions.Item>
            )}
          </ProDescriptions>

          {/* Información Técnica */}
          <Divider orientation="left">
            <CodeOutlined /> Información Técnica
          </Divider>
          <ProDescriptions column={1}>
            <ProDescriptions.Item label="Endpoint">
              <Tag color="blue">{selectedRecord.httpMethod}</Tag>{" "}
              <code>{selectedRecord.endpoint || "N/A"}</code>
            </ProDescriptions.Item>
            <ProDescriptions.Item
              label={
                <Space>
                  <ClockCircleOutlined />
                  Fecha y Hora
                </Space>
              }
            >
              {selectedRecord.timestamp
                ? dayjs(selectedRecord.timestamp).format("DD/MM/YYYY HH:mm:ss")
                : "N/A"}
            </ProDescriptions.Item>
            {selectedRecord.traceId && (
              <ProDescriptions.Item
                label={
                  <Space>
                    <LinkOutlined />
                    Trace ID
                  </Space>
                }
              >
                <code>{selectedRecord.traceId}</code>
              </ProDescriptions.Item>
            )}
          </ProDescriptions>

          {/* Error Message */}
          {selectedRecord.errorMessage && (
            <>
              <Divider orientation="left">
                <InfoCircleOutlined /> Mensaje de Error
              </Divider>
              <ProDescriptions column={1}>
                <ProDescriptions.Item>
                  <Alert
                    message={selectedRecord.errorMessage}
                    type="error"
                    showIcon
                  />
                </ProDescriptions.Item>
              </ProDescriptions>
            </>
          )}

          {/* Affected Keys (rutas JSON modificadas) */}
          {selectedRecord.affectedKeys &&
            selectedRecord.affectedKeys.length > 0 && (
              <>
                <Divider orientation="left">
                  <KeyOutlined /> Campos Afectados
                </Divider>
                <ProDescriptions column={1}>
                  <ProDescriptions.Item>
                    <div
                      style={{
                        background: "#f5f5f5",
                        padding: "12px",
                        borderRadius: "4px",
                        fontSize: "12px",
                        maxHeight: "200px",
                        overflow: "auto",
                      }}
                    >
                      {selectedRecord.affectedKeys.map((key, index) => (
                        <Tag
                          key={index}
                          color="purple"
                          style={{ marginBottom: 4 }}
                        >
                          {key}
                        </Tag>
                      ))}
                    </div>
                  </ProDescriptions.Item>
                </ProDescriptions>
              </>
            )}

          {/* Filters Used (parámetros de consulta) */}
          {selectedRecord.filtersUsed &&
            Object.keys(selectedRecord.filtersUsed).length > 0 && (
              <>
                <Divider orientation="left">
                  <FilterOutlined /> Filtros Utilizados
                </Divider>
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
                      {formatJson(selectedRecord.filtersUsed)}
                    </pre>
                  </ProDescriptions.Item>
                </ProDescriptions>
              </>
            )}

          {/* Additional Info */}
          {selectedRecord.additionalInfo &&
            Object.keys(selectedRecord.additionalInfo).length > 0 && (
              <>
                <Divider orientation="left">
                  <InfoCircleOutlined /> Información Adicional
                </Divider>
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