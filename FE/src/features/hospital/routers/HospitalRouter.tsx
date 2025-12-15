import { Navigate, Route, Routes } from "react-router";
import { ProtectedRoute } from "../../../shared/components";
import {
  CreateHospitalPage,
  HospitalDetailsPage,
  UpdateHospitalPage,
} from "../pages";

export const HospitalRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="details" replace />} />

      <Route path="/create" element={
        <ProtectedRoute action="create" subject="hospital">
          <CreateHospitalPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/update" element={
        <ProtectedRoute action="update" subject="hospital">
          <UpdateHospitalPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/details" element={
        <ProtectedRoute action="read" subject="hospital">
          <HospitalDetailsPage />
        </ProtectedRoute>
        } 
      />
    </Routes>
  );
};
