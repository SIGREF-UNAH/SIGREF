import { Navigate, Route, Routes } from "react-router";
import { CreatePractitionerPage, PractitionersListPage, UpdatePractitionerPage } from "../pages";
import PractitionerDetailsPage from "../pages/PractitionerDetailsPage";

export const PractitionersRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<PractitionersListPage />} />
      <Route path="/create" element={<CreatePractitionerPage />} />
      <Route path="/update/:id" element={<UpdatePractitionerPage />} />
      <Route path="/details/:id" element={<PractitionerDetailsPage />} />
    </Routes>
  );
};