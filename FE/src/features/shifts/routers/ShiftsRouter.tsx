import { Navigate, Route, Routes } from "react-router";
import { ShiftsPage } from "../pages/ShiftsPage";
import { ProtectedRoute } from "../../../shared/components";

export const ShiftsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      <Route path="/list" element={
        <ProtectedRoute action="read" subject="shifts">
          <ShiftsPage />
        </ProtectedRoute>
        } 
      />
    </Routes>
  );
};