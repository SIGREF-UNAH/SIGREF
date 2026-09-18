// EventHistoryTable.tsx
import { ProTable } from "@ant-design/pro-components";
import type { AuditLog, GetAuditLogsParams, PagedResultDtoOfAuditLog as AuditLogPagedResultDto } from "@models/audit";
import { PAGE_SIZE_OPTIONS } from "../constants";
import { useTableColumns, useColorUtils } from "../hooks";
import { createTablePagination } from "../../../shared/components/ui";

interface Props {
  data: AuditLogPagedResultDto; 
  filters: GetAuditLogsParams; 
  isLoading: boolean;
  onPageChange: (page: number, pageSize: number) => void;
  onViewDetails: (record: AuditLog) => void;
}

export const EventHistoryTable = ({ data, filters, isLoading, onPageChange, onViewDetails }: Props) => {
  const { getActionColor, getStatusColor, getHttpMethodColor } = useColorUtils();

  const columns = useTableColumns({
    getActionColor,
    getStatusColor,
    getHttpMethodColor,
    onViewDetails,
  });

  return (
    <ProTable<AuditLog>
      className="secondary-card"
      bordered
      rowKey="id"
      columns={columns}
      dataSource={data?.items || []} // Protección opcional mediante optional chaining
      search={false}
      loading={isLoading}
      scroll={{ x: 1600 }}
      pagination={createTablePagination({
        current: data?.pagination?.currentPage ?? filters.CurrentPage ?? 1,
        pageSize: data?.pagination?.pageSize ?? filters.PageSize ?? 10,
        total: data?.pagination?.totalItems ?? 0,
        showTotal: (total) => `Total ${total} registros`,
        pageSizeOptions: PAGE_SIZE_OPTIONS,
        onChange: onPageChange,
        showQuickJumper: true,
      })}
    />
  );
};
