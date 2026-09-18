import { Space, Tag, Typography } from "antd";
import { ApiOutlined } from "@ant-design/icons";

const { Text } = Typography;

interface Props {
  resourceType?: string | null;
  resourceId?: string | null;
}

export const ResourceCell = ({ resourceType, resourceId }: Props) => (
  <Space direction="vertical" size={0}>
    {resourceType && (
      <Tag color="geekblue" icon={<ApiOutlined />}>
        {resourceType}
      </Tag>
    )}
    {resourceId && (
      <Text type="secondary" style={{ fontSize: 11 }} ellipsis copyable={{ text: resourceId }}>
        {resourceId.length > 20 ? `${resourceId.substring(0, 20)}...` : resourceId}
      </Text>
    )}
  </Space>
);