import { Navigate, Route, Routes } from "react-router";
import CreateOrganizationsPage from "../pages/CreateOrganizationsPage";
import OrganizationsListPage from "../pages/ListOrganizationsPage";
import { UpdateOrganizationPage } from "../pages/UpdateOrganizationsPage";

export const OrganizationsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      {/* Ruta de en listar organizaciones*/}
      <Route path="/list" element={<OrganizationsListPage />} />
      {/* Ruta para crear una nueva organización */}
      <Route path="/create" element={<CreateOrganizationsPage />} />
      {/* Ruta para actualizar una organización existente */}
      <Route path="/update/:id" element={<UpdateOrganizationPage />} />
    </Routes>
  );
};