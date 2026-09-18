import { Tag, Tooltip } from "antd";

interface Props {
  action?: string | null;
  resourceType?: string | null;
  endpoint?: string | null;
  httpMethod?: string | null;
  getActionColor: (action: string) => string;
}

export const ActionCell = ({ action, resourceType, endpoint, httpMethod, getActionColor }: Props) => (
  <div>
    <div>
      <Tag color={getActionColor(action || "")}>{action || "N/A"}</Tag>
      {resourceType && <Tag color="geekblue">{resourceType}</Tag>}
    </div>
    <Tooltip title={endpoint}>
      <code
        style={{
          fontSize: 10,
          color: "#8c8c8c",
          display: "block",
          marginTop: 4,
          overflow: "hidden",
          textOverflow: "ellipsis",
          whiteSpace: "nowrap",
          maxWidth: 220,
        }}
      >
        {httpMethod?.toUpperCase() || ""} {endpoint || ""}
      </code>
    </Tooltip>
  </div>
);