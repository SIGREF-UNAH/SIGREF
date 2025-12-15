import { Navigate, Route, Routes } from "react-router";
import { ProtectedRoute } from "../../../shared/components";
import {
  CreateServiceGroupPage,
  ServiceGroupsPage,
  UpdateServiceGroupPage,
} from "../pages";

export const ServiceGroupsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      <Route path="/list" element={
        <ProtectedRoute action="read" subject="service-groups">
          <ServiceGroupsPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/create" element={
        <ProtectedRoute action="create" subject="service-groups">
          <CreateServiceGroupPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/update/:id" element={
        <ProtectedRoute action="update" subject="service-groups">
          <UpdateServiceGroupPage />
        </ProtectedRoute>
        } 
      />
    </Routes>
  );
};
