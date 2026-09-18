import { ProList } from '@ant-design/pro-components'
import { createPaginationConfig } from '../../../shared/components/ui/tablePagination'
import { Space, Tag, Typography, Spin, Input, Badge } from 'antd'
import { SearchOutlined, AppstoreOutlined } from '@ant-design/icons'
import { useState } from 'react'
import type { ServiceGroupDto } from '@models'

interface ServiceGroupIncomeFilters {
  searchServiceGroups: string
  pageServiceGroup: number
  pageSizeServiceGroup: number
}

type ServiceGroupIncomeFilterKey = keyof ServiceGroupIncomeFilters

type ServiceGroupIncomeProps = {
  serviceGroupsData: ServiceGroupDto[]
  serviceGroupFilters: ServiceGroupIncomeFilters
  setServiceGroupFilter: <K extends ServiceGroupIncomeFilterKey>(
    key: K,
    value: ServiceGroupIncomeFilters[K],
  ) => void
  setServiceGroupFilters: (filters: Partial<ServiceGroupIncomeFilters>) => void
  handleSelectServiceGroup: (serviceGroup: ServiceGroupDto) => void
  selectedServiceGroup: ServiceGroupDto | null
  isLoading: boolean
  showCost?: boolean
}

export const ServiceGroupIncome = ({
  serviceGroupsData,
  serviceGroupFilters,
  setServiceGroupFilter,
  setServiceGroupFilters,
  handleSelectServiceGroup,
  selectedServiceGroup,
  isLoading,
}: ServiceGroupIncomeProps) => {
  const [searchText, setSearchText] = useState(serviceGroupFilters.searchServiceGroups || '')

  // Filtrado inteligente
  const serviceGroupsFiltrados = (serviceGroupsData ?? []).filter((group) => {
    const term = searchText.toLowerCase().trim()
    if (!term) return true

    const title = group.title?.toLowerCase() || ''
    const code = group.code?.coding?.[0]?.code?.toLowerCase() || ''

    return title.includes(term) || code.includes(term)
  })

  return (
    <>
      {/* Barra de busqueda */}
      <div style={{ marginBottom: 16 }}>
        <Input
          placeholder="Buscar por nombre o código del paquete..."
          prefix={<SearchOutlined style={{ color: '#aaa' }} />}
          size="large"
          allowClear
          value={searchText}
          onChange={(e) => {
            const value = e.target.value
            setSearchText(value)
            setServiceGroupFilter('searchServiceGroups', value)
          }}
          style={{ borderRadius: 8 }}
        />
      </div>

      {/* Lista de Paquetes */}
      {isLoading ? (
        <div style={{ textAlign: 'center', padding: '60px 0' }}>
          <Spin size="large" tip="Cargando paquetes..." />
        </div>
      ) : serviceGroupsFiltrados.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '60px 0', color: '#999' }}>
          <Typography.Text type="secondary">
            {searchText
              ? `No se encontraron paquetes con "${searchText}"`
              : 'No hay paquetes disponibles'}
          </Typography.Text>
        </div>
      ) : (
        <ProList<ServiceGroupDto>
          rowKey="id"
          dataSource={serviceGroupsFiltrados}
          pagination={createPaginationConfig({
            current: serviceGroupFilters.pageServiceGroup || 1,
            pageSize: serviceGroupFilters.pageSizeServiceGroup || 10,
            total: serviceGroupsFiltrados.length,
            onChange: (page, pageSize) =>
              setServiceGroupFilters({
                pageServiceGroup: page,
                pageSizeServiceGroup: pageSize,
              }),
            pageSizeOptions: ['10', '20', '50'],
            showTotal: (total) => `Total: ${total} paquetes`,
          })}
          metas={{
            title: {
              render: (_, record) => (
                <Space>
                  <Tag color="purple" style={{ fontWeight: 'bold' }}>
                    <AppstoreOutlined /> {record.code?.coding?.[0]?.code || 'S/C'}
                  </Tag>
                  <Typography.Text strong>{record.title || 'Paquete sin título'}</Typography.Text>
                  {record.items && record.items.length > 0 && (
                    <Badge
                      count={record.items.length}
                      style={{ backgroundColor: '#52c41a' }}
                      title={`${record.items.length} servicios incluidos`}
                    />
                  )}
                </Space>
              ),
            },

            description: {
              render: (_, record) => (
                <Typography.Text type="secondary" style={{ fontSize: 13 }}>
                  {record.description || 'Sin descripción'}
                </Typography.Text>
              ),
            },

            subTitle: {
              render: (_, record) => (
                <div style={{ textAlign: 'right' }}>
                  {record.totalPrice !== null && record.totalPrice !== undefined ? (
                    <Typography.Text strong type="success" style={{ fontSize: 18 }}>
                      L. {record.totalPrice.toFixed(2)}
                    </Typography.Text>
                  ) : record.totalPrice ? (
                    <Typography.Text type="secondary" style={{ fontSize: 14 }}>
                      Sin costo
                    </Typography.Text>
                  ) : null}
                </div>
              ),
            },
          }}
          onItem={(record) => ({
            onClick: () => handleSelectServiceGroup(record),
            style: {
              cursor: 'pointer',
              border:
                selectedServiceGroup?.id === record.id ? '2px solid #722ed1' : '1px solid #f0f0f0',
              borderRadius: 8,
              marginBottom: 12,
              padding: '12px 16px',
              transition: 'all 0.3s',
              backgroundColor: selectedServiceGroup?.id === record.id ? '#f9f0ff' : 'white',
              boxShadow:
                selectedServiceGroup?.id === record.id
                  ? '0 4px 12px rgba(114, 46, 209, 0.15)'
                  : 'none',
            },
          })}
        />
      )}
    </>
  )
}
