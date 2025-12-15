import { Navigate, Route, Routes } from "react-router";
import { ProtectedRoute } from "../../../shared/components";
import {
  CreateHealthcarePage,
  HealthcaresPage,
  UpdateHealthcarePage,
} from "../pages";

export const HealthcaresRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      <Route path="/list" element={
        <ProtectedRoute action="read" subject="healthcares">
          <HealthcaresPage />
        </ProtectedRoute>
        } 
      />
      
      <Route path="/create" element={
        <ProtectedRoute action="create" subject="healthcares">
          <CreateHealthcarePage />
        </ProtectedRoute>
        } 
      />
      
      <Route path="/update/:id" element={
        <ProtectedRoute action="update" subject="healthcares">
          <UpdateHealthcarePage />
        </ProtectedRoute>
        } 
      />
    </Routes>
  );
};
