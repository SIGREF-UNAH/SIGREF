// JsonPreviewSection.tsx
import { ProDescriptions } from "@ant-design/pro-components";
import { Divider } from "antd";
import type { ReactNode } from "react";

const PRE_STYLE: React.CSSProperties = {
  background: "#f5f5f5",
  padding: "12px",
  borderRadius: "4px",
  fontSize: "12px",
  overflow: "auto",
  maxHeight: "200px",
  margin: 0,
};

const formatJson = (data: any): string => {
  try {
    if (typeof data === "string") return JSON.stringify(JSON.parse(data), null, 2);
    return JSON.stringify(data, null, 2);
  } catch {
    return JSON.stringify(data, null, 2);
  }
};

interface Props {
  icon: ReactNode;
  title: string;
  data: any;
}

export const JsonPreviewSection = ({ icon, title, data }: Props) => (
  <>
    <Divider orientation="left">
      {icon} {title}
    </Divider>
    <ProDescriptions column={1}>
      <ProDescriptions.Item>
        <pre style={PRE_STYLE}>{formatJson(data)}</pre>
      </ProDescriptions.Item>
    </ProDescriptions>
  </>
);