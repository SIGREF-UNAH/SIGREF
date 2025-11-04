import { Navigate, Route, Routes } from "react-router";
import CreateOrganizationsPage from "../pages/CreateOrganizationsPage";
import OrganizationsListPage from "../pages/OrganizationsListPage";

export const OrganizationsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<OrganizationsListPage />} />
      <Route path="/create" element={<CreateOrganizationsPage />} />
      {/* <Route path="/update/:id" element={<UpdateOrganizationPage />} /> */}
    </Routes>
  );
};