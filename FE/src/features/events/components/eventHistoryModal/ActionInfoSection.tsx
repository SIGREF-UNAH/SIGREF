// ActionInfoSection.tsx
import { ApiOutlined } from "@ant-design/icons";
import { ProDescriptions } from "@ant-design/pro-components";
import { Divider, Tag } from "antd";
import type { AuditLog } from "@models/audit/auditLog";
import { ACTION_COLOR_MAP, HTTP_METHOD_COLOR_MAP, STATUS_COLOR_MAP } from "../../constants";

interface Props {
  record: AuditLog;
  httpMethodUpper: string;
  actionLower: string;
}

const getStatusTagColor = (statusCode?: number): string => {
  if (!statusCode) return "default";
  if (STATUS_COLOR_MAP[statusCode]) return STATUS_COLOR_MAP[statusCode];
  if (statusCode >= 200 && statusCode < 300) return "success";
  if (statusCode >= 400) return "error";
  return "default";
};

export const ActionInfoSection = ({ record, httpMethodUpper, actionLower }: Props) => (
  <>
    <Divider orientation="left">
      <ApiOutlined /> Información de la Acción
    </Divider>
    <ProDescriptions column={2}>
      <ProDescriptions.Item label="Acción">
        <Tag color={ACTION_COLOR_MAP[actionLower] || "default"}>
          {record.action || "N/A"}
        </Tag>
      </ProDescriptions.Item>
      <ProDescriptions.Item label="Código de Estado">
        <Tag color={getStatusTagColor(record.statusCode)}>
          {record.statusCode || "N/A"}
        </Tag>
      </ProDescriptions.Item>
      {record.resourceType && (
        <ProDescriptions.Item label="Tipo de Recurso">
          <Tag color="geekblue">{record.resourceType}</Tag>
        </ProDescriptions.Item>
      )}
      {record.resourceId && (
        <ProDescriptions.Item label="ID de Recurso">
          <code>{record.resourceId}</code>
        </ProDescriptions.Item>
      )}
      {httpMethodUpper && (
        <ProDescriptions.Item label="Método HTTP">
          <Tag color={HTTP_METHOD_COLOR_MAP[httpMethodUpper] || "blue"}>
            {httpMethodUpper}
          </Tag>
        </ProDescriptions.Item>
      )}
      {record.statusCode && (
        <ProDescriptions.Item label="Status Code">
          <code>{record.statusCode}</code>
        </ProDescriptions.Item>
      )}
    </ProDescriptions>
  </>
);
