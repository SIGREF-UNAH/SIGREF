import { Space, Tooltip, Typography } from "antd";
import { LinkOutlined } from "@ant-design/icons";

const { Text } = Typography;

interface Props {
  traceId?: string | null;
}

export const TraceIdCell = ({ traceId }: Props) => {
  const display = traceId
    ? traceId.length > 12
      ? `${traceId.substring(0, 12)}...`
      : traceId
    : "N/A";

  return (
    <Tooltip title={traceId}>
      <Space>
        <LinkOutlined style={{ fontSize: 12 }} />
        <Text copyable={{ text: traceId ?? "" }} style={{ fontSize: 12 }} ellipsis>
          {display}
        </Text>
      </Space>
    </Tooltip>
  );
};