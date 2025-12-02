import { Navigate, Route, Routes } from "react-router";
import {
  CreateHospitalPage,
  HospitalDetailsPage,
  UpdateHospitalPage,
} from "../pages";

export const HospitalRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="details" replace />} />
      <Route path="/create" element={<CreateHospitalPage />} />
      <Route path="/update" element={<UpdateHospitalPage />} />
      <Route path="/details" element={<HospitalDetailsPage />} />
    </Routes>
  );
};
