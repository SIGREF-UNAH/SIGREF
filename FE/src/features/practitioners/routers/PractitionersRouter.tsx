import { Navigate, Route, Routes } from "react-router";

export const PractitionersRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      {/* <Route path="/list" element={<PractitionersListPage />} /> */}
      {/* <Route path="/create" element={<CreatePractitionerPage />} /> */}
      {/* <Route path="/update/:id" element={<UpdatePractitionerPage />} /> */}
    </Routes>
  );
};