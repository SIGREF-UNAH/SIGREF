// UserInfoSection.tsx
import { GlobalOutlined, UserOutlined } from "@ant-design/icons";
import { ProDescriptions } from "@ant-design/pro-components";
import { Divider, Space } from "antd";
import type { AuditLog } from "../../../../api/models/auditLog";

interface Props {
  record: AuditLog;
}

export const UserInfoSection = ({ record }: Props) => (
  <>
    <Divider orientation="left">
      <UserOutlined /> Información del Usuario
    </Divider>
    <ProDescriptions column={2}>
      <ProDescriptions.Item label="Nombre de Usuario">
        <strong>{record.userName || "N/A"}</strong>
      </ProDescriptions.Item>
      <ProDescriptions.Item label="ID de Usuario">
        <code>{record.userId || "N/A"}</code>
      </ProDescriptions.Item>
      {record.ipAddress && (
        <ProDescriptions.Item label="Dirección IP" span={2}>
          <Space>
            <GlobalOutlined />
            <code>{record.ipAddress}</code>
          </Space>
        </ProDescriptions.Item>
      )}
    </ProDescriptions>
  </>
);