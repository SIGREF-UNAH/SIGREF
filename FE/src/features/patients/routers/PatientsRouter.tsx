import { Navigate, Route, Routes } from "react-router";
import {
  CreatePatient,
  PatientDetails,
  PatientsList,
  UpdatePatient,
} from "../pages";

export const PatientsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/create" element={<CreatePatient />} />
      <Route path="/list" element={<PatientsList />} />
      <Route path="/details/:id" element={<PatientDetails />} />
      <Route path="/update/:id" element={<UpdatePatient />} />
    </Routes>
  );
};
