// hooks/useSeriesManagement.ts
import { Form, message, type FormInstance } from "antd";
import type { SerieDto } from "../../../api/models";
import { useState } from "react";
import {
  useDeleteApiSeriesId,
  usePostApiSeries,
  usePutApiSeriesId,
} from "../../../api/series/series";
import { useQueryClient } from "@tanstack/react-query";

interface UseSeriesManagementResult {
  form: FormInstance;
  editingId: string | null;
  isCreating: boolean;
  isUpdating: boolean;
  handleSubmit: (values: any) => void;
  handleEdit: (record: SerieDto) => void;
  handleDelete: (id: string) => void;
  handleCancelEdit: () => void;
}

export const useSeriesManagement = (
  refetch: () => void,
): UseSeriesManagementResult => {
  const [form] = Form.useForm();
  const [editingId, setEditingId] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const invalidateSeriesCache = () => {
    queryClient.invalidateQueries({ queryKey: ["/api/Series"] });
    setTimeout(() => refetch(), 100);
  };

  const { mutate: createSerie, isPending: isCreating } = usePostApiSeries({
    mutation: {
      onSuccess: () => {
        message.success("Serie creada exitosamente");
        form.resetFields();
        setEditingId(null);
        invalidateSeriesCache();
      },
      onError: (error: any) => {
        const msg =
          error?.response?.data?.message ||
          error?.response?.data?.title ||
          error?.message ||
          "Error al crear la serie";
        message.error(msg, 5);
      },
    },
  });

  const { mutate: updateSerie, isPending: isUpdating } = usePutApiSeriesId({
    mutation: {
      onSuccess: () => {
        message.success("Serie actualizada exitosamente");
        setEditingId(null);
        form.resetFields();
        invalidateSeriesCache();
      },
      onError: (error: any) => {
        message.error(
          error?.response?.data?.message || "Error al actualizar la serie",
        );
      },
    },
  });

  const { mutate: deleteSerie } = useDeleteApiSeriesId({
    mutation: {
      onSuccess: () => {
        message.success("Serie eliminada exitosamente");
        invalidateSeriesCache();
      },
      onError: (error: any) => {
        message.error(
          error?.response?.data?.message || "Error al eliminar la serie",
        );
      },
    },
  });

  const handleSubmit = (values: any) => {
    const serieData = {
      name: values.name,
      prefix: values.prefix,
      startNumber: values.startNumber,
      endNumber: values.endNumber,
    };

    if (editingId) {
      updateSerie({ id: editingId, data: serieData });
    } else {
      createSerie({ data: serieData });
    }
  };

  const handleEdit = (record: SerieDto) => {
    const id = (record as any).id;
    if (!id) {
      message.error("No se puede editar: falta el ID");
      return;
    }

    setEditingId(id);
    form.setFieldsValue({
      name: record.name,
      prefix: record.prefix,
      startNumber: record.startNumber,
      endNumber: record.endNumber,
    });
  };

  const handleDelete = (id: string) => {
    // Ahora solo pasamos el string
    deleteSerie(id);
  };

  const handleCancelEdit = () => {
    setEditingId(null);
    form.resetFields();
  };

  return {
    form,
    editingId,
    isCreating,
    isUpdating,
    handleSubmit,
    handleEdit,
    handleDelete,
    handleCancelEdit,
  };
};
