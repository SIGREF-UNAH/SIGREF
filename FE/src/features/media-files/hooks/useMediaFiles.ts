import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import type { MediaFileType } from "../../../api/models";
import { getGetApiMediaFilesQueryKey, useDeleteApiMediaFilesId, useGetApiMediaFiles, usePostApiMediaFilesMediaIdAssign, usePostApiMediaFilesUpload } from "../../../api/media-files/media-files";
import { getGetApiHospitalPropertiesDetailsQueryKey } from "../../../api/hospital-properties/hospital-properties";

export default function useMediaFiles(logoType: MediaFileType = 0) {
  const queryClient = useQueryClient();
  const msg = useMessage();

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(12);
  const [searchQuery, setSearchQuery] = useState("");

  // Query para obtener las imágenes según el tipo de logo
  const { data: response, isLoading } = useGetApiMediaFiles({
    Type: logoType,
    Search: searchQuery || undefined,
    PageNumber: currentPage,
    PageSize: pageSize,
  });

  // Extraer los datos e información de paginación
  const responseData = response as any;
  const mediaFiles = responseData?.data?.items || [];
  const pagination = responseData?.data?.pagination;

  // Mutation para subir imagen
  const uploadMutation = usePostApiMediaFilesUpload({
    mutation: {
      onSuccess: () => {
        msg.success("Imagen subida exitosamente");
        // Invalidar queries de media files y hospital details
        queryClient.invalidateQueries({
          queryKey: getGetApiMediaFilesQueryKey(),
        });
        queryClient.invalidateQueries({
          queryKey: getGetApiHospitalPropertiesDetailsQueryKey(),
        });
      },
      onError: (error) => {
        console.error("Error al subir imagen:", error);
        msg.error("Error al subir la imagen");
      },
    },
  });

  // Mutation para eliminar imagen
  const deleteMutation = useDeleteApiMediaFilesId({
    mutation: {
      onSuccess: () => {
        msg.success("Imagen eliminada exitosamente");
        queryClient.invalidateQueries({
          queryKey: getGetApiMediaFilesQueryKey(),
        });
      },
      onError: (error) => {
        console.error("Error al eliminar imagen:", error);
        msg.error("Error al eliminar la imagen");
      },
    },
  });

  // Mutation para asignar logo al hospital
  const assignMutation = usePostApiMediaFilesMediaIdAssign({
    mutation: {
      onSuccess: () => {
        msg.success("Logotipo asignado exitosamente");
        queryClient.invalidateQueries({
          queryKey: getGetApiHospitalPropertiesDetailsQueryKey(),
        });
      },
      onError: (error) => {
        console.error("Error al asignar logotipo:", error);
        msg.error("Error al asignar el logotipo");
      },
    },
  });

  const handleUpload = (
    file: File,
    type: MediaFileType,
    description?: string
  ) => {
    // Asegurar que type sea número
    const numericType = Number(type) as MediaFileType;

    uploadMutation.mutate({
      data: {
        File: file,
        Type: numericType,
        Description: description || undefined,
      },
    });
  };

  const handleDelete = (id: string) => {
    deleteMutation.mutate({ id });
  };

  const handleAssign = (mediaId: string, type: MediaFileType) => {
    assignMutation.mutate({
      mediaId,
      params: { type },
    });
  };

  const handleSearch = (value: string) => {
    setSearchQuery(value);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number, size?: number) => {
    setCurrentPage(page);
    if (size) setPageSize(size);
  };

  // Función helper para construir URLs de media
  const getMediaUrl = (relativePath?: string | null): string => {
    if (!relativePath) return '';
    
    const API_BASE_URL = import.meta.env.VITE_API_URL || window.location.origin;
    const baseUrl = API_BASE_URL.endsWith('/') 
      ? API_BASE_URL.slice(0, -1) 
      : API_BASE_URL;
    
    if (relativePath.startsWith('http://') || relativePath.startsWith('https://')) {
      return relativePath;
    }
    
    if (relativePath.startsWith('/files/') || relativePath.startsWith('/media/')) {
      return `${baseUrl}${relativePath}`;
    }
    
    return `${baseUrl}${relativePath.startsWith('/') ? '' : '/'}${relativePath}`;
  };

  return {
    mediaFiles,
    pagination,
    isLoading,
    uploadMutation,
    deleteMutation,
    assignMutation,
    currentPage,
    pageSize,
    searchQuery,
    handleUpload,
    handleDelete,
    handleAssign,
    handleSearch,
    handlePageChange,
    getMediaUrl,
  };
}
