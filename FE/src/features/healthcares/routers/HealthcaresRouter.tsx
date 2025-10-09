import { Route, Routes } from "react-router";
import {
  CreateHealthcarePage,
  HealthcaresPage,
  UpdateHealthcarePage,
} from "../pages";

export const HealthcaresRouter = () => {
  return (
    <Routes>
      <Route path="/*" element={<HealthcaresPage />} />
      <Route path="/create" element={<CreateHealthcarePage />} />
      <Route path="/update/:id" element={<UpdateHealthcarePage />} />
    </Routes>
  );
};
