// EventHistoryModal.tsx
import {
  EyeOutlined,
  FilterOutlined,
  InfoCircleOutlined,
} from "@ant-design/icons";
import { Alert, Button, Divider, Modal, Descriptions, Space } from "antd";
import type { AuditLog } from "../../../api/models/auditLog";
import { EventStatusAlert } from "./eventHistoryModal/EventStatusAlert";
import { UserInfoSection } from "./eventHistoryModal/UserInfoSection";
import { TechnicalInfoSection } from "./eventHistoryModal/TechnicalInfoSection";
import { AffectedKeysSection } from "./eventHistoryModal/AffectedKeysSection";
import { JsonPreviewSection } from "./eventHistoryModal/JsonPreviewSection";
import { ActionInfoSection } from "./eventHistoryModal/ActionInfoSection";

interface Props {
  modalOpen: boolean;
  selectedRecord: AuditLog | null | undefined;
  setModalOpen: (open: boolean) => void;
  dayjs: (date: string | Date) => any;
}

export const EventHistoryModal = ({
  modalOpen,
  selectedRecord,
  setModalOpen,
  dayjs,
}: Props) => {
  const httpMethodUpper = selectedRecord?.httpMethod?.toUpperCase() || "";
  const actionLower = selectedRecord?.action?.toLowerCase() || "";

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
          <EventStatusAlert success={selectedRecord.success} />

          <UserInfoSection record={selectedRecord} />

          <ActionInfoSection
            record={selectedRecord}
            httpMethodUpper={httpMethodUpper}
            actionLower={actionLower}
          />

          <TechnicalInfoSection
            record={selectedRecord}
            httpMethodUpper={httpMethodUpper}
            dayjs={dayjs}
          />

          {selectedRecord.errorMessage && (
            <>
              <Divider orientation="left">
                <InfoCircleOutlined /> Mensaje de Error
              </Divider>
              <Descriptions column={1}>
                <Descriptions.Item>
                  <Alert
                    message={selectedRecord.errorMessage}
                    type="error"
                    showIcon
                  />
                </Descriptions.Item>
              </Descriptions>
            </>
          )}

          {(selectedRecord.affectedKeys?.length ?? 0) > 0 && (
            <AffectedKeysSection keys={selectedRecord.affectedKeys} />
          )}

          {selectedRecord.filtersUsed &&
            Object.keys(selectedRecord.filtersUsed).length > 0 && (
              <JsonPreviewSection
                icon={<FilterOutlined />}
                title="Filtros Utilizados"
                data={selectedRecord.filtersUsed}
              />
            )}

          {selectedRecord.additionalInfo &&
            Object.keys(selectedRecord.additionalInfo).length > 0 && (
              <JsonPreviewSection
                icon={<InfoCircleOutlined />}
                title="Información Adicional"
                data={selectedRecord.additionalInfo}
              />
            )}

          <Divider orientation="left">Identificador</Divider>
          <Descriptions column={1}>
            <Descriptions.Item label="ID del Registro">
              <code>{selectedRecord.id || "N/A"}</code>
            </Descriptions.Item>
          </Descriptions>
        </>
      )}
    </Modal>
  );
};
