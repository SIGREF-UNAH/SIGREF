import React from "react";
import { ModalForm } from "@ant-design/pro-components";
import { Space, Typography } from "antd";
import { ExclamationCircleOutlined } from "@ant-design/icons";

interface DeleteLocationModalProps {
  visible: boolean;
  onVisibleChange: (visible: boolean) => void;
  locationId: number | null;
  locationName?: string;
  onDelete: (locationId: number) => Promise<boolean>;
}

const DeleteLocationModal: React.FC<DeleteLocationModalProps> = ({
  visible,
  onVisibleChange,
  locationId,
  locationName,
  onDelete,
}) => {
  const [loading, setLoading] = React.useState(false);

  if (!locationId) return null;

  const handleFinish = async () => {
    setLoading(true);
    try {
      const success = await onDelete(locationId);
      if (success) {
        onVisibleChange(false);
      }
      return success;
    } finally {
      setLoading(false);
    }
  };

  return (
    <ModalForm
      title={
        <Space align="center">
          <ExclamationCircleOutlined style={{ color: "#f5222d", fontSize: 24 }} />
          <Typography.Title level={4} style={{ margin: 0, color: "#333333" }}>
            Eliminar Ubicación
          </Typography.Title>
        </Space>
      }
      visible={visible}
      onVisibleChange={onVisibleChange}
      onFinish={handleFinish}
      width={600}
      modalProps={{
        destroyOnClose: true,
        bodyStyle: { backgroundColor: "#FAFAFA", padding: "24px", borderRadius: 8 },
      }}
      submitter={{
        submitButtonProps: {
          danger: true,
          size: "middle",
          children: "Eliminar",
          loading: loading,
          style: { backgroundColor: "#f5222d", borderColor: "#f5222d", borderRadius: 4 },
        },
        resetButtonProps: {
          size: "middle",
          children: "Cancelar",
          style: { borderRadius: 4 },
        },
      }}
    >
      <Space direction="vertical" size="middle" style={{ width: "100%" }}>
        <Typography.Text style={{ color: "#616161", fontSize: 16 }}>
          ¿Estás seguro de eliminar la ubicación{" "}
          <strong>{locationName || "seleccionada"}</strong>? Esta acción no se puede deshacer.
        </Typography.Text>
        <div style={{ borderTop: "1px solid #D9D9D9", margin: "16px 0" }} /> 
        <Typography.Text style={{ color: "#616161", fontSize: 14, fontStyle: "italic" }}>
          Nota: Todos los datos asociados a esta ubicación serán eliminados permanentemente.
        </Typography.Text>
      </Space>
    </ModalForm>
  );
};

export default DeleteLocationModal;