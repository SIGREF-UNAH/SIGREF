import { Navigate, Route, Routes } from "react-router";
import CreateOrganizationsPage from "../pages/CreateOrganizationsPage";
import OrganizationsListPage from "../pages/ListOrganizationsPage";
import { UpdateOrganizationPage } from "../pages/UpdateOrganizationsPage";
import { ProtectedRoute } from "../../../shared/components";

export const OrganizationsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      
      <Route path="/list" element={
        <ProtectedRoute action="read" subject="organizations">
          <OrganizationsListPage />
        </ProtectedRoute>
        } 
      />
      
      <Route path="/create" element={
        <ProtectedRoute action="create" subject="organizations">
          <CreateOrganizationsPage />
        </ProtectedRoute>
        } 
      />
      
      <Route path="/update/:id" element={
        <ProtectedRoute action="update" subject="organizations">
          <UpdateOrganizationPage />
        </ProtectedRoute>
        } 
      />
    </Routes>
  );
};