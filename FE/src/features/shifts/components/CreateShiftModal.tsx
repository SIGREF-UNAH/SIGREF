import { Modal, Form, Input, TimePicker, Select, message } from "antd";
import { useQueryClient } from "@tanstack/react-query";
import type { LocationDto } from "../../../api/models";
import { getGetShiftListQueryKey, useCreateShift } from "../../../api/shifts/shifts";

interface CreateShiftModalProps {
  open: boolean;
  onClose: () => void;
  locations: LocationDto[];
}

export const CreateShiftModal = ({
  open,
  onClose,
  locations,
}: CreateShiftModalProps) => {
  const [form] = Form.useForm();
  const queryClient = useQueryClient();
  const [messageApi, contextHolder] = message.useMessage();

  const { mutateAsync: createShift, isPending } = useCreateShift({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetShiftListQueryKey() });
        messageApi.success("Turno creado correctamente");
        form.resetFields();
        onClose();
      },
      onError: (error: any) => {
        messageApi.error(
          error?.response?.data?.message || "Error al crear el turno"
        );
      },
    },
  });

  const handleSubmit = async (values: any) => {
    const payload = {
      name: values.name,
      startTime: values.startTime?.format("HH:mm:ss"),
      endTime: values.endTime?.format("HH:mm:ss"),
      locationId: values.locationId,
      isActive: true,
    };

    await createShift({ data: payload });
  };

  const handleCancel = () => {
    form.resetFields();
    onClose();
  };

  return (
    <>
      {contextHolder}
      <Modal
        title={
          <div className="text-xl font-semibold text-general-primary">
            Crear Nuevo Turno
          </div>
        }
        open={open}
        onCancel={handleCancel}
        width={600}
        centered
        footer={null}
        destroyOnClose
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
              { min: 3, message: "Mínimo 3 caracteres" },
              { max: 50, message: "Máximo 50 caracteres" },
            ]}
          >
            <Input
              size="large"
              placeholder="Ej. Turno Mañana, Turno Tarde"
              disabled={isPending}
            />
          </Form.Item>

          <div className="grid grid-cols-2 gap-4">
            <div className="grid grid-cols-2 gap-4">
              <Form.Item
                label="Hora de Inicio"
                name="startTime"
                rules={[
                  {
                    required: true,
                    message: "La hora de inicio es obligatoria",
                  },
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
          </div>

          <Form.Item
            label="Área / Ubicación"
            name="locationId"
            rules={[{ required: true, message: "Debe seleccionar un área" }]}
          >
            <Select
              size="large"
              placeholder="Seleccionar área"
              disabled={isPending}
              showSearch
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

          <div className="flex justify-end gap-3 mt-8">
            <button
              type="button"
              onClick={handleCancel}
              disabled={isPending}
              className="px-6 py-2.5 text-white bg-red-500 hover:bg-red-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isPending}
              className="px-8 py-2.5 text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition flex items-center gap-2"
            >
              {isPending ? <>Procesando...</> : <>Crear Turno</>}
            </button>
          </div>
        </Form>
      </Modal>
    </>
  );
};
