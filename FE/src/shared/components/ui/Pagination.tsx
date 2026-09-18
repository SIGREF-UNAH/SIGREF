import { Pagination as AntPagination } from 'antd';
import type { PaginationProps as AntPaginationProps } from 'antd';

export interface PaginationProps {
  current: number;
  pageSize: number;
  total: number;
  onChange: (page: number, pageSize: number) => void;
  pageSizeOptions?: number[] | string[];
  showSizeChanger?: boolean;
  showTotal?: AntPaginationProps['showTotal'];
  disabled?: boolean;
  className?: string;
}

const DEFAULT_PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

export function Pagination({
  current,
  pageSize,
  total,
  onChange,
  pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS,
  showSizeChanger = true,
  showTotal = (totalItems, range) => `${range[0]}-${range[1]} de ${totalItems}`,
  disabled = false,
  className,
}: PaginationProps) {
  return (
    <AntPagination
      current={current}
      pageSize={pageSize}
      total={total}
      onChange={(page, nextPageSize) => onChange(page, Number(nextPageSize))}
      pageSizeOptions={pageSizeOptions}
      showSizeChanger={showSizeChanger}
      showTotal={showTotal}
      disabled={disabled}
      className={className}
    />
  );
}

export { DEFAULT_PAGE_SIZE_OPTIONS };
