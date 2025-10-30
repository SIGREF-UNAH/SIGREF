import { Navigate, Route, Routes } from "react-router";
import {
  CreatePatientPage,
  PatientsListPage,
  UpdatePatientPage,
} from "../pages";

export const PatientsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/create" element={<CreatePatientPage />} />
      <Route path="/list" element={<PatientsListPage />} />
      <Route path="/update/:id" element={<UpdatePatientPage />} />
    </Routes>
  );
};
