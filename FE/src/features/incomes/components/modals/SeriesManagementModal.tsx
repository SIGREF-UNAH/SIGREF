// components/SeriesManagementModal.tsx
import { Modal, Form, Input, InputNumber, Button, Space, Table, Popconfirm } from "antd";
import type { SerieDto } from "../../../../api/models";
import { DeleteOutlined, EditOutlined, PlusOutlined } from "@ant-design/icons";
import { useSeriesManagement } from "../../hooks";

interface SeriesManagementModalProps {
  open: boolean;
  onClose: () => void;
  series: SerieDto[];
  refetch: () => void;
}

export const SeriesManagementModal = ({
  open,
  onClose,
  series,
  refetch,
}: SeriesManagementModalProps) => {
  const {
    form,
    editingId,
    isCreating,
    isUpdating,
    handleSubmit,
    handleEdit,
    handleDelete,
    handleCancelEdit,
  } = useSeriesManagement(refetch);

  const columns = [
    { title: "Nombre", dataIndex: "name", key: "name" },
    { title: "Prefijo", dataIndex: "prefix", key: "prefix", width: 100 },
    { title: "Inicio", dataIndex: "startNumber", key: "startNumber", width: 100 },
    { title: "Fin", dataIndex: "endNumber", key: "endNumber", width: 100 },
    {
      title: "Actual",
      dataIndex: "currentNumber",
      key: "currentNumber",
      width: 100,
      render: (val: number) => val ?? 0,
    },
    {
      title: "Acciones",
      key: "actions",
      width: 120,
      render: (_: any, record: SerieDto) => {
        const id = (record as any).id;
        return (
          <Space>
            <Button
              type="link"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record)}
              size="small"
            />
            <Popconfirm
              title="¿Eliminar serie?"
              description="Esta acción no se puede deshacer"
              onConfirm={() => id && handleDelete(id)}
              okText="Sí"
              cancelText="No"
              disabled={!id}
            >
              <Button type="link" danger icon={<DeleteOutlined />} size="small" />
            </Popconfirm>
          </Space>
        );
      },
    },
  ];

  return (
    <Modal
      title="Gestión de Series"
      open={open}
      onCancel={onClose}
      footer={null}
      width={800}
      onOk={onClose}
    >
      <Space direction="vertical" style={{ width: "100%" }} size="large">
        {/* Formulario */}
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item
            label="Nombre"
            name="name"
            rules={[{ required: true, message: "El nombre es requerido" }]}
          >
            <Input placeholder="Ej: Serie A, Principal, etc." />
          </Form.Item>

          <Form.Item
            label="Prefijo"
            name="prefix"
            rules={[
              { required: true, message: "El prefijo es requerido" },
              { max: 10, message: "Máximo 10 caracteres" },
              { pattern: /^[A-Z0-9]+$/, message: "Solo mayúsculas y números" },
            ]}
          >
            <Input
              placeholder="Ej: A001"
              maxLength={10}
              style={{ textTransform: "uppercase" }}
              onChange={(e) =>
                form.setFieldValue("prefix", e.target.value.toUpperCase())
              }
            />
          </Form.Item>

          <Space style={{ width: "100%" }}>
            <Form.Item
              label="Número Inicial"
              name="startNumber"
              rules={[
                { required: true, message: "Requerido" },
                { type: "number", min: 0, message: "≥ 0" },
              ]}
              initialValue={1}
            >
              <InputNumber min={0} style={{ width: "100%" }} />
            </Form.Item>

            <Form.Item
              label="Número Final"
              name="endNumber"
              rules={[
                { required: true, message: "Requerido" },
                { type: "number", min: 1, message: "> 0" },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || value > getFieldValue("startNumber")) {
                      return Promise.resolve();
                    }
                    return Promise.reject(
                      new Error("Debe ser mayor que el número inicial")
                    );
                  },
                }),
              ]}
              initialValue={9999}
            >
              <InputNumber min={1} style={{ width: "100%" }} />
            </Form.Item>
          </Space>

          <Form.Item>
            <Space>
              <Button
                type="primary"
                htmlType="submit"
                icon={<PlusOutlined />}
                loading={isCreating || isUpdating}
              >
                {editingId ? "Actualizar" : "Crear Serie"}
              </Button>
              {editingId && (
                <Button onClick={handleCancelEdit}>Cancelar</Button>
              )}
            </Space>
          </Form.Item>
        </Form>

        {/* Tabla */}
        <Table
          columns={columns}
          dataSource={series}
          rowKey={(record) => (record as any).id || `temp-${Math.random()}`}
          pagination={false}
          size="small"
          locale={{ emptyText: "No hay series. Crea una para comenzar." }}
        />
      </Space>
    </Modal>
  );
};