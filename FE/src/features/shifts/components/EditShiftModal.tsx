import {
  Modal,
  Form,
  Input,
  TimePicker,
  Select,
  Switch,
  message,
  Space,
} from "antd";
import { useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import React from "react";
import type { LocationDto, ShiftDto } from "../../../api/models";
import { getGetApiShiftsQueryKey, usePutApiShiftsId } from "../../../api/shifts/shifts";

interface EditShiftModalProps {
  open: boolean;
  onClose: () => void;
  shift: ShiftDto | null;
  locations: LocationDto[];
}

export const EditShiftModal = ({
  open,
  onClose,
  shift,
  locations,
}: EditShiftModalProps) => {
  const [form] = Form.useForm();
  const queryClient = useQueryClient();
  const [messageApi, contextHolder] = message.useMessage();

  const { mutateAsync: updateShift, isPending } = usePutApiShiftsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiShiftsQueryKey() });
        messageApi.success("Turno actualizado correctamente");
        onClose();
      },
      onError: (error: any) => {
        messageApi.error(
          error?.response?.data?.message || "Error al actualizar el turno"
        );
      },
    },
  });

  // Estado local para el switch
  const [isActive, setIsActive] = React.useState<boolean>(true);

  // Precargar datos
  React.useEffect(() => {
    if (open && shift) {
      const active = shift.isActive ?? true;
      setIsActive(active);

      form.setFieldsValue({
        name: shift.name,
        startTime: shift.startTime ? dayjs(shift.startTime, "HH:mm:ss") : null,
        endTime: shift.endTime ? dayjs(shift.endTime, "HH:mm:ss") : null,
        locationId: shift.locationId,
        isActive: active,
      });
    }
  }, [open, shift, form]);

  const handleSubmit = async (values: any) => {
    if (!shift?.id) return;

    const payload = {
      name: values.name.trim(),
      startTime: values.startTime.format("HH:mm:ss"),
      endTime: values.endTime.format("HH:mm:ss"),
      locationId: values.locationId,
      isActive: isActive,
    };

    await updateShift({ id: shift.id, data: payload });
  };

  const handleCancel = () => {
    if (!isPending) {
      form.resetFields();
      onClose();
    }
  };

  return (
    <>
      {contextHolder}
      <Modal
        title={
          <div className="text-xl font-semibold text-general-primary">
            Editar Turno
          </div>
        }
        open={open}
        onCancel={handleCancel}
        width={620}
        centered
        footer={null}
        destroyOnClose
        closeIcon={isPending ? false : undefined}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          className="mt-6"
        >
          <Form.Item
            label="Nombre del Turno"
            name="name"
            rules={[
              { required: true, message: "El nombre es obligatorio" },
              { min: 3, max: 50, message: "Entre 3 y 50 caracteres" },
            ]}
          >
            <Input
              size="large"
              placeholder="Ej. Turno Mañana"
              disabled={isPending}
            />
          </Form.Item>

          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label="Hora de Inicio"
              name="startTime"
              rules={[
                { required: true, message: "La hora de inicio es obligatoria" },
              ]}
            >
              <TimePicker
                format="H:mm"
                minuteStep={1}
                showNow={false}
                size="large"
                placeholder="8:00"
                style={{ width: "100%" }}
                disabled={isPending}
                className="font-medium text-center"
                allowClear
              />
            </Form.Item>

            <Form.Item
              label="Hora de Fin"
              name="endTime"
              rules={[
                { required: true, message: "La hora de fin es obligatoria" },
              ]}
            >
              <TimePicker
                format="H:mm"
                minuteStep={1}
                showNow={false}
                size="large"
                placeholder="17:00"
                style={{ width: "100%" }}
                disabled={isPending}
                className="font-medium text-center"
                allowClear
              />
            </Form.Item>
          </div>

          {/* Selector de Estado */}
          <Form.Item label="Estado del Turno" className="mb-6">
            <Space direction="vertical" className="w-full">
              <Switch
                checkedChildren="ACTIVO"
                unCheckedChildren="INACTIVO"
                size="small"
                checked={isActive}
                onChange={(checked) => setIsActive(checked)}
                disabled={isPending}
                className="bg-gray-400 data-[state=unchecked]:bg-gray-400"
              />
            </Space>
          </Form.Item>

          <Form.Item
            label="Área / Ubicación"
            name="locationId"
            rules={[{ required: true, message: "Selecciona un área" }]}
          >
            <Select
              size="large"
              placeholder="Buscar área"
              disabled={isPending}
              showSearch
              loading={locations.length === 0}
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.label ?? "")
                  .toLowerCase()
                  .includes(input.toLowerCase())
              }
              options={locations.map((loc) => ({
                label: loc.name,
                value: loc.id,
              }))}
            />
          </Form.Item>

          <div className="flex justify-end gap-4 mt-10">
            <button
              type="button"
              onClick={handleCancel}
              disabled={isPending}
              className="px-8 py-2.5 text-white bg-gray-500 hover:bg-gray-600 rounded-lg disabled:opacity-50 transition font-medium"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isPending}
              className="px-8 py-2.5 text-white bg-green-600 hover:bg-green-700 rounded-lg disabled:opacity-50 transition font-medium flex items-center gap-2"
            >
              {isPending ? "Guardando..." : "Guardar Cambios"}
            </button>
          </div>
        </Form>
      </Modal>
    </>
  );
};
