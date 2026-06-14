// TechnicalInfoSection.tsx
import { ClockCircleOutlined, CodeOutlined, LinkOutlined } from "@ant-design/icons";
import { ProDescriptions } from "@ant-design/pro-components";
import { Divider, Space, Tag } from "antd";
import type { AuditLog } from "../../../../api/models/auditLog";
import { HTTP_METHOD_COLOR_MAP } from "../../constants";

interface Props {
  record: AuditLog;
  httpMethodUpper: string;
  dayjs: (date: string | Date) => any;
}

export const TechnicalInfoSection = ({ record, httpMethodUpper, dayjs }: Props) => (
  <>
    <Divider orientation="left">
      <CodeOutlined /> Información Técnica
    </Divider>
    <ProDescriptions column={1}>
      <ProDescriptions.Item label="Endpoint">
        {httpMethodUpper && (
          <Tag color={HTTP_METHOD_COLOR_MAP[httpMethodUpper] || "blue"}>
            {httpMethodUpper}
          </Tag>
        )}{" "}
        <code>{record.endpoint || "N/A"}</code>
      </ProDescriptions.Item>
      <ProDescriptions.Item
        label={
          <Space>
            <ClockCircleOutlined />
            Fecha y Hora
          </Space>
        }
      >
        {record.timestamp
          ? dayjs(record.timestamp).format("DD/MM/YYYY HH:mm:ss")
          : "N/A"}
      </ProDescriptions.Item>
      {record.traceId && (
        <ProDescriptions.Item
          label={
            <Space>
              <LinkOutlined />
              Trace ID
            </Space>
          }
        >
          <code>{record.traceId}</code>
        </ProDescriptions.Item>
      )}
    </ProDescriptions>
  </>
);