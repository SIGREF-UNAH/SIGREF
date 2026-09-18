import React, { useState, useCallback, useMemo } from "react";
import useMediaFiles from "../hooks/useMediaFiles";
import { MediaFilesList } from "./MediaFilesList";
import type { MediaFileType } from "@types/media-files";
import { UploadMediaForm } from "./UploadMediaForm";
import { Modal, Button, Input, Space, Tabs, type TabsProps } from "antd";
import {
  PictureOutlined,
  SearchOutlined,
  BankOutlined,
  MedicineBoxOutlined,
} from "@ant-design/icons";

interface LogoSelectorModalProps {
  readonly open: boolean;
  readonly onClose: () => void;
}

type ViewMode = "list" | "upload";

interface TabConfig {
  readonly key: string;
  readonly mediaType: MediaFileType;
  readonly label: React.ReactNode;
}

const TAB_CONFIGS: readonly TabConfig[] = [
  {
    key: "appHospital",
    mediaType: "appHospital" as MediaFileType,
    label: (
      <span>
        <BankOutlined /> Logo del Hospital
      </span>
    ),
  },
  {
    key: "healthGuilt",
    mediaType: "healthGuilt" as MediaFileType,
    label: (
      <span>
        <MedicineBoxOutlined /> Logo de Salud
      </span>
    ),
  },
] as const;

const DEFAULT_TAB = TAB_CONFIGS[0];

export const LogoSelectorModal: React.FC<LogoSelectorModalProps> = ({
  open,
  onClose,
}) => {
  const [viewMode, setViewMode] = useState<ViewMode>("list");
  const [activeTab, setActiveTab] = useState<MediaFileType>(DEFAULT_TAB.mediaType);

  const mediaFilesHook = useMediaFiles(activeTab);

  const handleAssignSuccess = useCallback((): void => {
    setViewMode("list");
    onClose();
  }, [onClose]);

  const handleUploadSuccess = useCallback((): void => {
    setViewMode("list");
  }, []);

  const handleCancel = useCallback((): void => {
    setViewMode("list");
    setActiveTab(DEFAULT_TAB.mediaType);
    onClose();
  }, [onClose]);

  const handleTabChange = useCallback((key: string): void => {
    const selectedTab = TAB_CONFIGS.find(tab => tab.key === key);
    if (selectedTab) {
      setActiveTab(selectedTab.mediaType);
    }
  }, []);

  const handleSwitchToUpload = useCallback((): void => {
    setViewMode("upload");
  }, []);

  const handleSwitchToList = useCallback((): void => {
    setViewMode("list");
  }, []);

  const handleSearch = useCallback((value: string): void => {
    mediaFilesHook.handleSearch(value);
  }, [mediaFilesHook]);

  const tabItems: TabsProps["items"] = useMemo(
    () =>
      TAB_CONFIGS.map((tab) => ({
        key: tab.key,
        label: tab.label,
      })),
    []
  );

  const modalTitle = useMemo((): React.ReactNode => {
    const isUploadMode = viewMode === "upload";

    return (
      <Space>
        <PictureOutlined className="text-blue-600" />
        <span>
          {isUploadMode ? "Subir Nueva Imagen" : "Seleccionar Logotipo"}
        </span>
      </Space>
    );
  }, [viewMode]);

  const renderListView = (): React.ReactNode => (
    <div className="space-y-4">
      <Tabs
        activeKey={activeTab}
        onChange={handleTabChange}
        items={tabItems}
      />

      <div className="flex gap-3 items-center">
        <Input
          placeholder="Buscar imágenes..."
          prefix={<SearchOutlined />}
          value={mediaFilesHook.searchQuery}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleSearch(e.target.value)}
          allowClear
          className="flex-1"
        />
        <Button
          type="primary"
          icon={<PictureOutlined />}
          onClick={handleSwitchToUpload}
        >
          Subir Imagen
        </Button>
      </div>

      <MediaFilesList
        {...mediaFilesHook}
        logoType={activeTab}
        onAssignSuccess={handleAssignSuccess}
      />
    </div>
  );

  const renderUploadView = (): React.ReactNode => (
    <UploadMediaForm
      onSuccess={handleUploadSuccess}
      onCancel={handleSwitchToList}
      uploadMutation={mediaFilesHook.uploadMutation}
      handleUpload={mediaFilesHook.handleUpload}
    />
  );

  const isListView = viewMode === "list";

  return (
    <Modal
      title={modalTitle}
      open={open}
      onCancel={handleCancel}
      width={900}
      footer={null}
      destroyOnHidden
    >
      {isListView ? renderListView() : renderUploadView()}
    </Modal>
  );
};
