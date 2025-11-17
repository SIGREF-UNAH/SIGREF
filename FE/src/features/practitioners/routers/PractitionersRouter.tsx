import { Navigate, Route, Routes } from "react-router";
import { PractitionersListPage } from "../pages/PractitionersListPage";
import { CreatePractitionerPage } from "../pages/CreatePractitionerPage";
import { EditPractitionerPage } from "../pages/EditPractitionerPage";
import PractitionerDetailsPage from "../pages/PractitionerDetailsPage";

export const PractitionersRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<PractitionersListPage />} />
      <Route path="/create" element={<CreatePractitionerPage />} />
      <Route path="/update/:id" element={<EditPractitionerPage />} />
      <Route path="/details/:id" element={<PractitionerDetailsPage />} />
    </Routes>
  );
};