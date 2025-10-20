import { Navigate, Route, Routes } from "react-router";
import {
  CreateHealthcarePage,
  HealthcaresPage,
  UpdateHealthcarePage,
} from "../pages";

export const HealthcaresRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<HealthcaresPage />} />
      <Route path="/create" element={<CreateHealthcarePage />} />
      <Route path="/update/:id" element={<UpdateHealthcarePage />} />
    </Routes>
  );
};
