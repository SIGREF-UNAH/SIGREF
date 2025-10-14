import { Navigate, Route, Routes } from "react-router";

export const ReportsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      {/* <Route path="/list" element={<ReportsListPage />} /> */}
      {/* <Route path="/create" element={<CreateReportPage />} /> */}
    </Routes>
  );
};