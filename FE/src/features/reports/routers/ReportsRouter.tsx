import { Navigate, Route, Routes } from "react-router";
import { CreateReportPage, HistoryReportsPage, ReportsControlPage } from "../pages";
import { ProtectedRoute } from "../../../shared/components";

export const ReportsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      <Route path="/list" element={
        <ProtectedRoute action="read" subject="reports">
          <ReportsControlPage />
        </ProtectedRoute>
        } 
      /> 
      
      <Route path="/create" element={
        <ProtectedRoute action="create" subject="reports">
          <CreateReportPage />
        </ProtectedRoute>
        } 
      /> 
      
      <Route path="/history" element={
        <ProtectedRoute action="read" subject="reports">
          <HistoryReportsPage />
        </ProtectedRoute>
        } 
      /> 
    </Routes>
  );
};