import { useState } from "react";
import { Table, Input } from "antd";
import type { ColumnsType, TablePaginationConfig } from "antd/es/table";

interface SelectionTableProps<T> {
  dataSource: T[];
  columns: ColumnsType<T>;
  rowKey: string;
  selectedRowKeys: React.Key[];
  onSelectionChange: (selectedKeys: React.Key[], selectedRows: T[]) => void;
  searchPlaceholder?: string;
  onSearch?: (value: string) => void;
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
  onSearch,
  loading = false,
  disabled = false,
  pagination,
  emptyText = "No hay datos disponibles",
  className = "",
}: SelectionTableProps<T>) {
  const [searchText, setSearchText] = useState("");

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSearchText(value);
    onSearch?.(value);
  };

  const handleClear = () => {
    setSearchText("");
    onSearch?.("");
  };

  const rowSelection = {
    selectedRowKeys,
    onChange: onSelectionChange,
    preserveSelectedRowKeys: true,
  };

  return (
    <div className={className}>
      <Input.Search
        placeholder={searchPlaceholder}
        allowClear
        value={searchText}
        onChange={handleSearch}
        onSearch={onSearch}
        className="mb-3"
        disabled={disabled || loading}
        enterButton={false}
        onReset={handleClear}
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
