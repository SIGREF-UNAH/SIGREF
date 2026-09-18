import { useState } from "react";
import type { AuditLog } from "@models/audit/auditLog";

export const useEventHistoryModal = () => {
  const [selectedRecord, setSelectedRecord] = useState<AuditLog | null>(null);
  const [modalOpen, setModalOpen] = useState(false);

  const handleViewDetails = (record: AuditLog) => {
    setSelectedRecord(record);
    setModalOpen(true);
  };

  return { selectedRecord, modalOpen, setModalOpen, handleViewDetails };
};
