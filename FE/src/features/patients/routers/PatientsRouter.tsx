import { Navigate, Route, Routes } from "react-router";

export const PatientsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      {/* <Route path="/list" element={<PatientsListPage />} /> */}
      {/* <Route path="/create" element={<CreatePatientPage />} /> */}
      {/* <Route path="/update/:id" element={<UpdatePatientPage />} /> */}
    </Routes>
  );
};