import React, { useState } from "react";
import {
  Form,
  Input,
  Button,
  Upload,
  Space,
  Radio,
  Image,
} from "antd";
import {
  InboxOutlined,
  ArrowLeftOutlined,
  UploadOutlined,
  BankOutlined,
  MedicineBoxOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import type { UploadFile, UploadProps } from "antd";
import type { MediaFileType } from "../../../api/models";

const { TextArea } = Input;

interface UploadMediaFormProps {
  uploadMutation: any;
  onSuccess: () => void;
  onCancel: () => void;
  handleUpload: (file: File, type: MediaFileType, description?: string) => void;
}

export const UploadMediaForm: React.FC<UploadMediaFormProps> = ({
  uploadMutation,
  onSuccess,
  onCancel,
  handleUpload,
}) => {
  const [form] = Form.useForm();
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [previewUrl, setPreviewUrl] = useState<string>("");

  const uploadProps: UploadProps = {
    name: "file",
    multiple: false,
    accept: "image/*",
    maxCount: 1,
    fileList,
    beforeUpload: (file) => {
      const isImage = file.type.startsWith("image/");
      if (!isImage) {
        return Upload.LIST_IGNORE;
      }
      const isLt5M = file.size / 1024 / 1024 < 5;
      if (!isLt5M) {
        return Upload.LIST_IGNORE;
      }

      // Crear preview URL
      const reader = new FileReader();
      reader.onload = (e) => {
        setPreviewUrl(e.target?.result as string);
      };
      reader.readAsDataURL(file);

      setFileList([file as UploadFile]);
      return false;
    },
    onRemove: () => {
      setFileList([]);
      setPreviewUrl("");
    },
  };

  const handleSubmit = async (values: any) => {
    if (fileList.length === 0) {
      return;
    }

    // Obtener el archivo correctamente
    const uploadFile = fileList[0];
    const file = (uploadFile.originFileObj || uploadFile) as File;

    if (!file) {
      console.error("No se pudo obtener el archivo");
      return;
    }

    const type = Number(values.type) as MediaFileType;

    // Llamar handleUpload
    handleUpload(file, type, values.description);
  };

  // Resetear el formulario cuando la mutación sea exitosa
  React.useEffect(() => {
    if (uploadMutation.isSuccess) {
      form.resetFields();
      setFileList([]);
      setPreviewUrl("");
      onSuccess();
      // Resetear el estado de la mutación para permitir nuevas subidas
      uploadMutation.reset();
    }
  }, [uploadMutation.isSuccess, form, onSuccess]);

  const handleRemoveFile = () => {
    setFileList([]);
    setPreviewUrl("");
  };

  return (
    <div className="border-0">
      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        initialValues={{ type: 0 }} 
      >
        {/* Imagen */}
        <Form.Item
          label="Imagen"
          required
          rules={[
            {
              validator: () => {
                if (fileList.length === 0) {
                  return Promise.reject("Por favor seleccione una imagen");
                }
                return Promise.resolve();
              },
            },
          ]}
        >
          {/* Mostrar solo vista previa cuando hay imagen */}
          {previewUrl ? (
            <div className="border border-gray-200 rounded-lg p-4 bg-gray-50">
              <div className="flex flex-col items-center justify-center">
                <div className="relative mb-4">
                  <Image
                    src={previewUrl}
                    alt="Vista previa"
                    style={{ 
                      maxHeight: 200, 
                      maxWidth: "100%",
                      objectFit: "contain"
                    }}
                    preview={{
                      mask: "Ver imagen completa",
                    }}
                  />
                </div>
                <Button
                  danger
                  icon={<DeleteOutlined />}
                  onClick={handleRemoveFile}
                >
                  Eliminar
                </Button>
              </div>
            </div>
          ) : (
            // Mostrar contenedor de subida solo cuando NO hay imagen
            <Upload.Dragger {...uploadProps}>
              <p className="ant-upload-drag-icon">
                <InboxOutlined className="text-blue-500" />
              </p>
              <p className="ant-upload-text">Click o arrastra una imagen aquí</p>
              <p className="ant-upload-hint">
                Formatos permitidos: JPG, PNG, GIF, SVG
                <br />
                Tamaño máximo: 5MB
              </p>
            </Upload.Dragger>
          )}
        </Form.Item>

        {/* Descripción */}
        <Form.Item
          name="description"
          label="Descripción"
          rules={[{ max: 200, message: "Máximo 200 caracteres" }]}
        >
          <TextArea
            rows={3}
            placeholder="Descripción de la imagen (opcional)"
            showCount
            maxLength={200}
          />
        </Form.Item>

        {/* Tipo de logo */}
        <Form.Item
          name="type"
          label="Tipo de Logo"
          rules={[{ required: true, message: "Seleccione el tipo de logo" }]}
        >
          <Radio.Group className="w-full">
            <Space direction="vertical" className="w-full">
              <Radio value={0} className="w-full">
                <Space>
                  <BankOutlined className="text-blue-600" />
                  <div>
                    <div className="font-medium">Logo del Hospital</div>
                  </div>
                </Space>
              </Radio>
              <Radio value={1} className="w-full">
                <Space>
                  <MedicineBoxOutlined className="text-green-600" />
                  <div>
                    <div className="font-medium">Logo de Salud</div>
                  </div>
                </Space>
              </Radio>
            </Space>
          </Radio.Group>
        </Form.Item>

        {/* Botones */}
        <Form.Item>
          <Space className="w-full justify-end">
            <Button
              icon={<ArrowLeftOutlined />}
              onClick={onCancel}
              disabled={uploadMutation.isPending}
            >
              Cancelar
            </Button>
            <Button
              type="primary"
              icon={<UploadOutlined />}
              htmlType="submit"
              loading={uploadMutation.isPending}
              disabled={fileList.length === 0}
            >
              Subir Imagen
            </Button>
          </Space>
        </Form.Item>
      </Form>
    </div>
  );
};