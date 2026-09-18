import { Button, Tag, Typography } from "antd";
import { EyeOutlined } from "@ant-design/icons";
import type { ProColumns } from "@ant-design/pro-components";
import dayjs from "dayjs";
import type { AuditLog } from "@models/audit/auditLog";
import { TraceIdCell, UserCell, ActionCell, ResourceCell, ResultCell } from "../../components";
const { Text } = Typography;

interface Params {
  getActionColor: (action: string) => string;
  getStatusColor: (statusCode?: number) => string;
  getHttpMethodColor: (method: string) => string;
  onViewDetails: (record: AuditLog) => void;
}


export const useTableColumns = ({
  getActionColor,
  getStatusColor,
  getHttpMethodColor,
  onViewDetails,
}: Params): ProColumns<AuditLog>[] => [
  {
    title: "Trace ID",
    dataIndex: "traceId",
    key: "traceId",
    width: 120,
    ellipsis: true,
    render: (_, record) => <TraceIdCell traceId={record.traceId} />,
  },
  {
    title: "Usuario",
    dataIndex: "userName",
    key: "userName",
    width: 170,
    render: (_, record) => <UserCell userName={record.userName} userId={record.userId} />,
  },
  {
    title: "HTTP",
    dataIndex: "httpMethod",
    key: "httpMethod",
    width: 80,
    align: "center",
    render: (_, record) =>
      record.httpMethod ? (
        <Tag color={getHttpMethodColor(record.httpMethod)}>{record.httpMethod.toUpperCase()}</Tag>
      ) : (
        <Tag>N/A</Tag>
      ),
  },
  {
    title: "Acción / Endpoint",
    dataIndex: "action",
    key: "action",
    width: 220,
    render: (_, record) => (
      <ActionCell
        action={record.action}
        resourceType={record.resourceType}
        endpoint={record.endpoint}
        httpMethod={record.httpMethod}
        getActionColor={getActionColor}
      />
    ),
  },
  {
    title: "Recurso",
    dataIndex: "resourceType",
    key: "resourceType",
    width: 140,
    render: (_, record) => <ResourceCell resourceType={record.resourceType} resourceId={record.resourceId} />,
  },
  {
    title: "IP",
    dataIndex: "ipAddress",
    key: "ipAddress",
    width: 130,
    ellipsis: true,
    render: (_, record) => (
      <Text copyable={!!record.ipAddress} style={{ fontSize: 12, fontFamily: "monospace" }}>
        {record.ipAddress || "N/A"}
      </Text>
    ),
  },
  {
    title: "Código",
    dataIndex: "statusCode",
    key: "statusCode",
    width: 100,
    align: "center",
    render: (_, record) => (
      <Tag color={getStatusColor(record.statusCode)}>{record.statusCode || "N/A"}</Tag>
    ),
  },
  {
    title: "Resultado",
    dataIndex: "success",
    key: "success",
    width: 100,
    align: "center",
    render: (_, record) => <ResultCell success={record.success} errorMessage={record.errorMessage} />,
  },
  {
    title: "Fecha",
    dataIndex: "timestamp",
    key: "timestamp",
    width: 180,
    sorter: true,
    render: (_, record) =>
      record.timestamp ? dayjs(record.timestamp).format("YYYY-MM-DD HH:mm:ss") : "N/A",
  },
  {
    title: "Acciones",
    key: "actions",
    width: 80,
    align: "center",
    fixed: "right",
    render: (_, record) => (
      <Button type="link" icon={<EyeOutlined />} onClick={() => onViewDetails(record)}>
        Ver
      </Button>
    ),
  },
];
