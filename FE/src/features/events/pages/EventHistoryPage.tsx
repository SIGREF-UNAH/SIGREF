// EventHistoryPage.tsx
import { PageContainer } from "@ant-design/pro-components";
import { Typography } from "antd";
import dayjs from "dayjs";
import { EventHistoryModal, EventHistoryFilters, EventHistoryTable } from "../components";
import { useEventHistory } from "../hooks"; // Usamos únicamente el hook principal

export const EventHistoryPage = () => {
  // Extraemos todo el estado unificado desde nuestro hook de negocio centralizado
  const {
    data,
    isLoading,
    form,
    formValues,
    filters,
    selectedRecord,
    modalOpen,
    setFilters,
    setFormValues,
    handleSearch,
    handleClearFilters,
    handleViewDetails,
    setModalOpen,
  } = useEventHistory(); 

  const handlePageChange = (page: number, pageSize: number) => {
    setFilters((prev) => ({
      ...prev,
      CurrentPage: page,
      PageSize: pageSize,
    }));
  };

  return (
    <PageContainer
      title={
        <Typography.Title level={2} style={{ margin: 0 }}>
          Historial de Eventos
        </Typography.Title>
      }
    >
      <div className="primary-card">
        <EventHistoryFilters
          form={form}
          formValues={formValues}
          setFormValues={setFormValues}
          onSearch={handleSearch}
          onClear={handleClearFilters}
        />
        <EventHistoryTable
          data={data}
          filters={filters}
          isLoading={isLoading}
          onPageChange={handlePageChange}
          onViewDetails={handleViewDetails}
        />
      </div>

      <EventHistoryModal
        selectedRecord={selectedRecord}
        setModalOpen={setModalOpen}
        modalOpen={modalOpen}
        dayjs={dayjs}
      />
    </PageContainer>
  );
};