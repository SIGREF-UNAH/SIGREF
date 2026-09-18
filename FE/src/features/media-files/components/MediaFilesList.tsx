import React from 'react'
import type { MediaFileDto } from '@models'
import type { MediaFileType } from '../../../api/generated/schemas/types/media-files/mediaFileType'
import { formatFileSize, getMediaUrl } from '../utils'
import { Card, Image, Button, Spin, Empty, Popconfirm, Tooltip, Typography } from 'antd'
import { Pagination } from '../../../shared/components/ui'
import { DeleteOutlined, CheckOutlined, FileImageOutlined } from '@ant-design/icons'

const { Text } = Typography

interface MediaFilesListProps {
  mediaFiles: MediaFileDto[]
  pagination?: MediaFilesPagination
  logoType: MediaFileType
  isLoading: boolean
  currentPage: number
  pageSize: number
  handleDelete: (id: string) => void
  handleAssign: (mediaId: string, type: MediaFileType) => void
  handlePageChange: (page: number, size?: number) => void
  onAssignSuccess: () => void
  deleteMutation: MutationStatus
  assignMutation: MutationStatus
}

interface MediaFilesPagination {
  currentPage?: number
  pageSize?: number
  totalItems?: number
}

interface MutationStatus {
  isPending: boolean
}

export const MediaFilesList: React.FC<MediaFilesListProps> = ({
  mediaFiles,
  pagination,
  logoType,
  isLoading,
  currentPage,
  pageSize,
  handleDelete,
  handleAssign,
  handlePageChange,
  onAssignSuccess,
  deleteMutation,
  assignMutation,
}) => {
  const handleAssignClick = (mediaId: string) => {
    handleAssign(mediaId, logoType)
    onAssignSuccess()
  }

  if (isLoading) {
    return (
      <div className="flex justify-center items-center py-20">
        <Spin size="large" tip="Cargando imágenes..." />
      </div>
    )
  }

  if (!mediaFiles || mediaFiles.length === 0) {
    return (
      <Empty
        image={Empty.PRESENTED_IMAGE_SIMPLE}
        description="No hay imágenes disponibles"
        className="py-10"
      />
    )
  }

  return (
    <div className="space-y-4">
      {/* Grid de imágenes */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        {mediaFiles.map((file) => {
          // Construir URL completa - priorizar file.url sobre relativePath
          const imageUrl = getMediaUrl(file.relativePath)

          return (
            <Card
              key={file.id}
              className="hover:shadow-lg transition-shadow"
              cover={
                <div className="h-40 bg-gray-100 flex items-center justify-center overflow-hidden">
                  {imageUrl ? (
                    <Image
                      src={imageUrl}
                      alt={file.fileName || 'Imagen'}
                      className="object-cover w-full h-full"
                      preview={{
                        mask: 'Vista previa',
                      }}
                      fallback="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mN8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg=="
                    />
                  ) : (
                    <FileImageOutlined className="text-4xl text-gray-400" />
                  )}
                </div>
              }
              actions={[
                <Tooltip key="assign" title="Asignar como logotipo">
                  <Button
                    type="text"
                    icon={<CheckOutlined />}
                    onClick={() => handleAssignClick(file.id!)}
                    loading={assignMutation.isPending}
                    className="text-green-600 hover:text-green-700"
                  >
                    Asignar
                  </Button>
                </Tooltip>,
                <Popconfirm
                  key="delete"
                  title="¿Eliminar imagen?"
                  description="Esta acción no se puede deshacer"
                  onConfirm={() => handleDelete(file.id!)}
                  okText="Eliminar"
                  cancelText="Cancelar"
                  okButtonProps={{ danger: true }}
                >
                  <Tooltip title="Eliminar imagen">
                    <Button
                      type="text"
                      icon={<DeleteOutlined />}
                      danger
                      loading={deleteMutation.isPending}
                    >
                      Eliminar
                    </Button>
                  </Tooltip>
                </Popconfirm>,
              ]}
            >
              <Card.Meta
                title={
                  <Tooltip title={file.fileName}>
                    <div className="truncate text-sm font-medium">
                      {file.fileName || 'Sin nombre'}
                    </div>
                  </Tooltip>
                }
                description={
                  <div className="space-y-1">
                    {file.description && (
                      <Text className="text-xs text-gray-600 block truncate">
                        {file.description}
                      </Text>
                    )}
                    <Text className="text-xs text-gray-400">{formatFileSize(file.sizeBytes)}</Text>
                  </div>
                }
              />
            </Card>
          )
        })}
      </div>

      {/* Paginación */}
      <div className="flex justify-center pt-4">
        <Pagination
          current={pagination?.currentPage || currentPage}
          pageSize={pagination?.pageSize || pageSize}
          total={pagination?.totalItems || 0}
          onChange={handlePageChange}
          showSizeChanger
          showTotal={(total) => `Total ${total} imágenes`}
          pageSizeOptions={['12', '24', '48']}
        />
      </div>
    </div>
  )
}
