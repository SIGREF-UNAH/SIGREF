import { Space, Typography } from "antd";
import { UserOutlined } from "@ant-design/icons";

const { Text } = Typography;

interface Props {
  userName?: string;
  userId?: string;
}

export const UserCell = ({ userName, userId }: Props) => (
  <Space direction="vertical" size={0}>
    <Space>
      <UserOutlined />
      <span style={{ fontWeight: 500 }}>{userName || "N/A"}</span>
    </Space>
    {userId && (
      <Text type="secondary" style={{ fontSize: 11, marginLeft: 24 }} ellipsis>
        {userId.substring(0, 20)}...
      </Text>
    )}
  </Space>
);