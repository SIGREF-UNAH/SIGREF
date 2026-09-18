import type { PaginationProps as AntPaginationProps, TablePaginationConfig } from 'antd';

import { DEFAULT_PAGE_SIZE_OPTIONS } from './Pagination';

interface TablePaginationOptions {
  current: number;
  pageSize: number;
  total: number;
  onChange: (page: number, pageSize: number) => void;
  pageSizeOptions?: number[] | string[];
  showSizeChanger?: boolean;
  showQuickJumper?: boolean;
  showTotal?: TablePaginationConfig['showTotal'];
}

type PaginationOptions = Omit<TablePaginationOptions, 'showTotal'> & {
  showTotal?: AntPaginationProps['showTotal'];
};

export function createPaginationConfig({
  current,
  pageSize,
  total,
  onChange,
  pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS,
  showSizeChanger = true,
  showQuickJumper = false,
  showTotal = (totalItems, range) => `${range[0]}-${range[1]} de ${totalItems}`,
}: PaginationOptions): AntPaginationProps {
  return {
    current,
    pageSize,
    total,
    pageSizeOptions,
    showSizeChanger,
    showQuickJumper,
    showTotal,
    onChange: (page, nextPageSize) => onChange(page, Number(nextPageSize)),
  };
}

export function createTablePagination({
  current,
  pageSize,
  total,
  onChange,
  pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS,
  showSizeChanger = true,
  showQuickJumper = false,
  showTotal = (totalItems, range) => `${range[0]}-${range[1]} de ${totalItems}`,
}: TablePaginationOptions): TablePaginationConfig {
  return {
    current,
    pageSize,
    total,
    pageSizeOptions,
    showSizeChanger,
    showQuickJumper,
    showTotal,
    onChange: (page, nextPageSize) => onChange(page, Number(nextPageSize)),
  };
}
