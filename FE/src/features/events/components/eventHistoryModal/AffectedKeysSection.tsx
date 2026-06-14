// AffectedKeysSection.tsx
import { KeyOutlined } from "@ant-design/icons";
import { ProDescriptions } from "@ant-design/pro-components";
import { Divider, Tag } from "antd";

interface Props {
  keys: string[] | null | undefined;
}

export const AffectedKeysSection = ({ keys }: Props) => (
  <>
    <Divider orientation="left">
      <KeyOutlined /> Campos Afectados
    </Divider>
    <ProDescriptions column={1}>
      <ProDescriptions.Item>
        <div
          style={{
            background: "#f5f5f5",
            padding: "12px",
            borderRadius: "4px",
            fontSize: "12px",
            maxHeight: "200px",
            overflow: "auto",
          }}
        >
          {keys?.map((key, index) => (
            <Tag key={index} color="purple" style={{ marginBottom: 4 }}>
              {key}
            </Tag>
          ))}
        </div>
      </ProDescriptions.Item>
    </ProDescriptions>
  </>
);