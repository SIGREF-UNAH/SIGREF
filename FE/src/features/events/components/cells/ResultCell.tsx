import { Space, Tag, Tooltip } from "antd";
import { WarningOutlined } from "@ant-design/icons";

interface Props {
  success?: boolean;
  errorMessage?: string;
}

export const ResultCell = ({ success, errorMessage }: Props) =>
  success ? (
    <Tag color="success">Exitoso</Tag>
  ) : (
    <Space size={4}>
      <Tag color="error">Fallido</Tag>
      {errorMessage && (
        <Tooltip title={errorMessage}>
          <WarningOutlined style={{ color: "#ff4d4f", fontSize: 14 }} />
        </Tooltip>
      )}
    </Space>
  );