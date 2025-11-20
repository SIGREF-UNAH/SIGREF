import { Navigate, Route, Routes } from "react-router";
import { CreateReportPage, HistoryReportsPage, ReportsControlPage } from "../pages";

export const ReportsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<ReportsControlPage />} /> 
      <Route path="/create" element={<CreateReportPage />} /> 
      <Route path="/history" element={<HistoryReportsPage />} /> 
    </Routes>
  );
};