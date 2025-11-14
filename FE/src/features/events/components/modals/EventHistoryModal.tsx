import {
  ApiOutlined,
  ClockCircleOutlined,
  CodeOutlined,
  EyeOutlined,
  GlobalOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { ProDescriptions } from "@ant-design/pro-components";
import { Button, Divider, Modal, Space, Tag } from "antd";

interface Props {
  modalOpen: boolean;
  selectedRecord: any;
  setModalOpen: (open: boolean) => void;
  getStatusColor: (statusCode: number) => string;
  getActionTypeColor: (actionType: string) => string;
  dayjs: (date: string | Date) => any;
}

export const EventHistoryModal = ({
  modalOpen,
  selectedRecord,
  setModalOpen,
  getStatusColor,
  getActionTypeColor,
  dayjs,
}: Props) => {
  return (
    <Modal
      title={
        <Space>
          <EyeOutlined />
          <span>Detalles</span>
        </Space>
      }
      open={modalOpen}
      onCancel={() => setModalOpen(false)}
      footer={[
        <Button key="close" type="primary" onClick={() => setModalOpen(false)}>
          Cerrar
        </Button>,
      ]}
      width={800}
    >
      {selectedRecord && (
        <>
          <Divider orientation="left">
            <UserOutlined /> Información del Usuario
          </Divider>
          <ProDescriptions column={2}>
            <ProDescriptions.Item label="ID de Usuario">
              {selectedRecord.UserId}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="Nombre de Usuario">
              {selectedRecord.Username}
            </ProDescriptions.Item>
          </ProDescriptions>

          <Divider orientation="left">
            <ApiOutlined /> Información de la Acción
          </Divider>
          <ProDescriptions column={2}>
            <ProDescriptions.Item label="Acción" span={2}>
              {selectedRecord.Action}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="Tipo de Acción">
              <Tag color={getActionTypeColor(selectedRecord.ActionType)}>
                {selectedRecord.ActionType}
              </Tag>
            </ProDescriptions.Item>
            <ProDescriptions.Item label="Código de Estado">
              <Tag color={getStatusColor(selectedRecord.StatusCode)}>
                {selectedRecord.StatusCode}
              </Tag>
            </ProDescriptions.Item>
            {selectedRecord.AdditionalData && (
              <ProDescriptions.Item label="Datos Adicionales" span={2}>
                {selectedRecord.AdditionalData}
              </ProDescriptions.Item>
            )}
            {selectedRecord.ErrorMessage && (
              <ProDescriptions.Item label="Mensaje de Error" span={2}>
                <Tag color="error">{selectedRecord.ErrorMessage}</Tag>
              </ProDescriptions.Item>
            )}
          </ProDescriptions>

          <Divider orientation="left">
            <CodeOutlined /> Información Técnica
          </Divider>
          <ProDescriptions column={2}>
            <ProDescriptions.Item label="Endpoint" span={2}>
              <Tag color="blue">{selectedRecord.HttpMethod}</Tag>{" "}
              <code>{selectedRecord.Endpoint}</code>
            </ProDescriptions.Item>
            <ProDescriptions.Item label="User Agent" span={2}>
              {selectedRecord.UserAgent}
            </ProDescriptions.Item>
            {selectedRecord.RequestBody && (
              <ProDescriptions.Item label="Request Body" span={2}>
                <pre
                  style={{
                    background: "#f5f5f5",
                    padding: "8px",
                    borderRadius: "4px",
                    fontSize: "12px",
                    overflow: "auto",
                    maxHeight: "200px",
                  }}
                >
                  {JSON.stringify(
                    JSON.parse(selectedRecord.RequestBody),
                    null,
                    2
                  )}
                </pre>
              </ProDescriptions.Item>
            )}
            {selectedRecord.ResponseBody && (
              <ProDescriptions.Item label="Response Body" span={2}>
                <pre
                  style={{
                    background: "#f5f5f5",
                    padding: "8px",
                    borderRadius: "4px",
                    fontSize: "12px",
                    overflow: "auto",
                    maxHeight: "200px",
                  }}
                >
                  {JSON.stringify(
                    JSON.parse(selectedRecord.ResponseBody),
                    null,
                    2
                  )}
                </pre>
              </ProDescriptions.Item>
            )}
          </ProDescriptions>

          <Divider orientation="left">
            <GlobalOutlined /> Información de Red
          </Divider>
          <ProDescriptions column={2}>
            <ProDescriptions.Item label="Dirección IP">
              {selectedRecord.IpAddress}
            </ProDescriptions.Item>
            <ProDescriptions.Item
              label={
                <Space>
                  <ClockCircleOutlined />
                  Fecha y Hora
                </Space>
              }
            >
              {dayjs(selectedRecord.Timestamp).format("DD/MM/YYYY HH:mm:ss")}
            </ProDescriptions.Item>
          </ProDescriptions>

          <Divider orientation="left">Identificador</Divider>
          <ProDescriptions column={1}>
            <ProDescriptions.Item label="ID del Registro">
              <code>{selectedRecord._id}</code>
            </ProDescriptions.Item>
          </ProDescriptions>
        </>
      )}
    </Modal>
  );
};
