import React, { useState, useCallback, useEffect } from "react";
import {
  Form,
  Input,
  Button,
  Upload,
  Space,
  Radio,
  Image,
  type UploadFile,
  type UploadProps,
  type RadioChangeEvent,
} from "antd";
import {
  InboxOutlined,
  ArrowLeftOutlined,
  UploadOutlined,
  BankOutlined,
  MedicineBoxOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import { MediaFileType } from "../../../api/models";
import type { UseMutationResult } from "@tanstack/react-query";

const { TextArea } = Input;

interface UploadMediaFormValues {
  readonly type: MediaFileType;
  readonly description?: string;
}

interface UploadMediaFormProps {
  readonly uploadMutation: UseMutationResult<
    unknown,
    Error,
    { data: FormData },
    unknown
  >;
  readonly onSuccess: () => void;
  readonly onCancel: () => void;
  readonly handleUpload: (
    file: File,
    type: MediaFileType,
    description?: string
  ) => void;
}

interface RadioOption {
  readonly value: MediaFileType;
  readonly label: React.ReactNode;
}

const RADIO_OPTIONS: readonly RadioOption[] = [
  {
    value: MediaFileType.appHospital,
    label: (
      <Space>
        <BankOutlined className="text-blue-600" />
        <div>
          <div className="font-medium">Logo del Hospital</div>
        </div>
      </Space>
    ),
  },
  {
    value: MediaFileType.healthGuilt,
    label: (
      <Space>
        <MedicineBoxOutlined className="text-green-600" />
        <div>
          <div className="font-medium">Logo de Salud</div>
        </div>
      </Space>
    ),
  },
] as const;

const MAX_FILE_SIZE_MB = 5;
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;
const MAX_DESCRIPTION_LENGTH = 200;
const ACCEPTED_IMAGE_TYPES = "image/*";

const DEFAULT_FORM_VALUES: UploadMediaFormValues = {
  type: MediaFileType.appHospital,
};

export const UploadMediaForm: React.FC<UploadMediaFormProps> = ({
  uploadMutation,
  onSuccess,
  onCancel,
  handleUpload,
}) => {
  const [form] = Form.useForm<UploadMediaFormValues>();
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [previewUrl, setPreviewUrl] = useState<string>("");

  const handleRemoveFile = useCallback((): void => {
    setFileList([]);
    setPreviewUrl("");
  }, []);

  const handleBeforeUpload = useCallback((file: File): boolean | typeof Upload.LIST_IGNORE => {
    const isImage = file.type.startsWith("image/");
    if (!isImage) {
      return Upload.LIST_IGNORE;
    }

    const isWithinSizeLimit = file.size <= MAX_FILE_SIZE_BYTES;
    if (!isWithinSizeLimit) {
      return Upload.LIST_IGNORE;
    }

    const reader = new FileReader();
    reader.onload = (e: ProgressEvent<FileReader>): void => {
      if (e.target?.result) {
        setPreviewUrl(e.target.result as string);
      }
    };
    reader.readAsDataURL(file);

    setFileList([file as unknown as UploadFile]);
    return false;
  }, []);

  const uploadProps: UploadProps = {
    name: "file",
    multiple: false,
    accept: ACCEPTED_IMAGE_TYPES,
    maxCount: 1,
    fileList,
    beforeUpload: handleBeforeUpload,
    onRemove: handleRemoveFile,
  };

  const handleSubmit = useCallback(
    (values: UploadMediaFormValues): void => {
      if (fileList.length === 0) {
        return;
      }

      const uploadFile = fileList[0];
      const file = (uploadFile.originFileObj || uploadFile) as File | undefined;

      if (!file) {
        console.error("No se pudo obtener el archivo");
        return;
      }

      handleUpload(file, values.type, values.description);
    },
    [fileList, handleUpload]
  );

  const handleTypeChange = useCallback((e: RadioChangeEvent): void => {
    form.setFieldsValue({ type: e.target.value as MediaFileType });
  }, [form]);

  // Resetear el formulario cuando la mutación sea exitosa
  useEffect(() => {
    if (uploadMutation.isSuccess) {
      form.resetFields();
      setFileList([]);
      setPreviewUrl("");
      onSuccess();
      uploadMutation.reset();
    }
  }, [uploadMutation.isSuccess, form, onSuccess, uploadMutation]);

  const isPending = uploadMutation.isPending;
  const hasNoFiles = fileList.length === 0;

  return (
    <div className="border-0">
      <Form<UploadMediaFormValues>
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        initialValues={DEFAULT_FORM_VALUES}
      >
        {/* Imagen */}
        <Form.Item
          label="Imagen"
          required
          rules={[
            {
              validator: (): Promise<void> => {
                if (fileList.length === 0) {
                  return Promise.reject(new Error("Por favor seleccione una imagen"));
                }
                return Promise.resolve();
              },
            },
          ]}
        >
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
                      objectFit: "contain",
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
            <Upload.Dragger {...uploadProps}>
              <p className="ant-upload-drag-icon">
                <InboxOutlined className="text-blue-500" />
              </p>
              <p className="ant-upload-text">
                Click o arrastra una imagen aquí
              </p>
              <p className="ant-upload-hint">
                Formatos permitidos: JPG, PNG, GIF, SVG
                <br />
                Tamaño máximo: {MAX_FILE_SIZE_MB}MB
              </p>
            </Upload.Dragger>
          )}
        </Form.Item>

        {/* Descripción */}
        <Form.Item
          name="description"
          label="Descripción"
          rules={[
            {
              max: MAX_DESCRIPTION_LENGTH,
              message: `Máximo ${MAX_DESCRIPTION_LENGTH} caracteres`,
            },
          ]}
        >
          <TextArea
            rows={3}
            placeholder="Descripción de la imagen (opcional)"
            showCount
            maxLength={MAX_DESCRIPTION_LENGTH}
          />
        </Form.Item>

        {/* Tipo de logo */}
        <Form.Item
          name="type"
          label="Tipo de Logo"
          rules={[{ required: true, message: "Seleccione el tipo de logo" }]}
        >
          <Radio.Group className="w-full" onChange={handleTypeChange}>
            <Space direction="vertical" className="w-full">
              {RADIO_OPTIONS.map((option) => (
                <Radio
                  key={option.value}
                  value={option.value}
                  className="w-full"
                >
                  {option.label}
                </Radio>
              ))}
            </Space>
          </Radio.Group>
        </Form.Item>

        {/* Botones */}
        <Form.Item>
          <Space className="w-full justify-end">
            <Button
              icon={<ArrowLeftOutlined />}
              onClick={onCancel}
              disabled={isPending}
            >
              Cancelar
            </Button>
            <Button
              type="primary"
              icon={<UploadOutlined />}
              htmlType="submit"
              loading={isPending}
              disabled={hasNoFiles}
            >
              Subir Imagen
            </Button>
          </Space>
        </Form.Item>
      </Form>
    </div>
  );
};