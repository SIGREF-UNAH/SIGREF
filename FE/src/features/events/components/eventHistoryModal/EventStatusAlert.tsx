// EventStatusAlert.tsx
import { CheckCircleOutlined, CloseCircleOutlined } from "@ant-design/icons";
import { Alert, Space } from "antd";

interface Props {
  success: boolean | undefined;
}

export const EventStatusAlert = ({ success }: Props) => (
  <div style={{ marginBottom: 16 }}>
    <Alert
      message={
        <Space>
          {success ? <CheckCircleOutlined /> : <CloseCircleOutlined />}
          <span>{success ? "Operación Exitosa" : "Operación Fallida"}</span>
        </Space>
      }
      type={success ? "success" : "error"}
      showIcon={false}
    />
  </div>
);