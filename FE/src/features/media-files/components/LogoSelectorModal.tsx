import React, { useState } from "react";
import useMediaFiles from "../hooks/useMediaFiles";
import { MediaFilesList } from "./MediaFilesList";
import type { MediaFileType } from "../../../api/models";
import { UploadMediaForm } from "./UploadMediaForm";
import { Modal, Button, Input, Space, Tabs } from "antd";
import {
  PictureOutlined,
  SearchOutlined,
  BankOutlined,
  MedicineBoxOutlined,
} from "@ant-design/icons";

interface LogoSelectorModalProps {
  open: boolean;
  onClose: () => void;
}

type ViewMode = "list" | "upload";

export const LogoSelectorModal: React.FC<LogoSelectorModalProps> = ({
  open,
  onClose,
}) => {
  const [viewMode, setViewMode] = useState<ViewMode>("list");
  const [activeTab, setActiveTab] = useState<string>("0");

  const logoType = Number(activeTab) as MediaFileType;
  const mediaFilesHook = useMediaFiles(logoType);

  const handleAssignSuccess = () => {
    setViewMode("list");
    onClose();
  };

  const handleUploadSuccess = () => {
    setViewMode("list");
  };

  const handleCancel = () => {
    setViewMode("list");
    setActiveTab("0");
    onClose();
  };

  const getModalTitle = () => {
    if (viewMode === "upload") {
      return (
        <Space>
          <PictureOutlined className="text-blue-600" />
          <span>Subir Nueva Imagen</span>
        </Space>
      );
    }

    return (
      <Space>
        <PictureOutlined className="text-blue-600" />
        <span>Seleccionar Logotipo</span>
      </Space>
    );
  };

  return (
    <Modal
      title={getModalTitle()}
      open={open}
      onCancel={handleCancel}
      width={900}
      footer={null}
      destroyOnHidden
    >
      {viewMode === "list" ? (
        <div className="space-y-4">
          {/* Tabs para seleccionar tipo de logo */}
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={[
              {
                key: "0",
                label: (
                  <span>
                    <BankOutlined /> Logo del Hospital
                  </span>
                ),
              },
              {
                key: "1",
                label: (
                  <span>
                    <MedicineBoxOutlined /> Logo de Salud
                  </span>
                ),
              },
            ]}
          />

          {/* Barra de búsqueda y botón de subir */}
          <div className="flex gap-3 items-center">
            <Input
              placeholder="Buscar imágenes..."
              prefix={<SearchOutlined />}
              value={mediaFilesHook.searchQuery}
              onChange={(e) => mediaFilesHook.handleSearch(e.target.value)}
              allowClear
              className="flex-1"
            />
            <Button
              type="primary"
              icon={<PictureOutlined />}
              onClick={() => setViewMode("upload")}
            >
              Subir Imagen
            </Button>
          </div>

          {/* Lista de imágenes */}
          <MediaFilesList
            {...mediaFilesHook}
            logoType={logoType}
            onAssignSuccess={handleAssignSuccess}
          />
        </div>
      ) : (
        <UploadMediaForm
          onSuccess={handleUploadSuccess}
          onCancel={() => setViewMode("list")}
          uploadMutation={mediaFilesHook.uploadMutation}
          handleUpload={mediaFilesHook.handleUpload}
        />
      )}
    </Modal>
  );
};
