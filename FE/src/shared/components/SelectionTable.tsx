import { Table, Input } from "antd";
import type { ColumnsType, TablePaginationConfig } from "antd/es/table";

const { Search } = Input;

interface SelectionTableProps<T> {
  dataSource: T[];
  columns: ColumnsType<T>;
  rowKey: string;
  selectedRowKeys: React.Key[];
  onSelectionChange: (selectedKeys: React.Key[], selectedRows: T[]) => void;
  searchPlaceholder?: string;
  searchValue?: string;
  onSearchChange?: (value: string) => void;
  onSearch?: () => void;
  onClearSearch?: () => void;
  loading?: boolean;
  disabled?: boolean;
  pagination?: false | TablePaginationConfig;
  emptyText?: string;
  className?: string;
}

export function SelectionTable<T extends Record<string, any>>({
  dataSource,
  columns,
  rowKey,
  selectedRowKeys,
  onSelectionChange,
  searchPlaceholder = "Buscar...",
  searchValue = "",
  onSearchChange,
  onSearch,
  onClearSearch,
  loading = false,
  disabled = false,
  pagination,
  emptyText = "No hay datos disponibles",
  className = "",
}: SelectionTableProps<T>) {
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onSearchChange?.(e.target.value);
  };

  const handleSearch = () => {
    onSearch?.();
  };

  const handleClear = () => {
    onClearSearch?.();
  };

  const rowSelection = {
    selectedRowKeys,
    onChange: onSelectionChange,
    preserveSelectedRowKeys: true,
  };

  return (
    <div className={className}>
      <Search
        placeholder={searchPlaceholder}
        allowClear
        value={searchValue}
        onChange={handleSearchChange}
        onSearch={handleSearch}
        className="mb-3"
        disabled={disabled || loading}
        enterButton
        onClear={handleClear}
      />

      <div className="border border-gray-300 rounded-lg overflow-hidden">
        <Table<T>
          rowSelection={{
            type: "checkbox",
            ...rowSelection,
          }}
          columns={columns}
          dataSource={dataSource}
          rowKey={rowKey}
          loading={loading}
          pagination={pagination}
          size="small"
          locale={{
            emptyText: emptyText,
          }}
        />
      </div>
    </div>
  );
}