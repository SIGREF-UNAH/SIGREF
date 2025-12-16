import { Navigate, Route, Routes } from "react-router";
import { ProtectedRoute } from "../../../shared/components";
import {
  CreatePatientPage,
  PatientsListPage,
  UpdatePatientPage,
} from "../pages";

export const PatientsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      <Route path="/list" element={
        <ProtectedRoute action="read" subject="patients">
          <PatientsListPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/create" element={
        <ProtectedRoute action="create" subject="patients">
          <CreatePatientPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/update/:id" element={
        <ProtectedRoute action="update" subject="patients">
          <UpdatePatientPage />
        </ProtectedRoute>
        } 
      />
    </Routes>
  );
};
