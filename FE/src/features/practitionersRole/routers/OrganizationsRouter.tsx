import { Navigate, Route, Routes } from "react-router";

export const OrganizationsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      {/* <Route path="/list" element={<OrganizationsListPage />} /> */}
      {/* <Route path="/create" element={<CreateOrganizationPage />} /> */}
      {/* <Route path="/update/:id" element={<UpdateOrganizationPage />} /> */}
    </Routes>
  );
};