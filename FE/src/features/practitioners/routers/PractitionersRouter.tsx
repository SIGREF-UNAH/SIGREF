import { Navigate, Route, Routes } from "react-router";
import { UnderConstructionPage } from "../../../shared/pages";

export const PractitionersRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<UnderConstructionPage />} /> {/* Reemplazar página */}
      <Route path="/create" element={<UnderConstructionPage />} /> {/* Reemplazar página */}
      <Route path="/update/:id" element={<UnderConstructionPage />} /> {/* Reemplazar página */}
      <Route path="/details/:id" element={<UnderConstructionPage />} /> {/* Reemplazar página */}
    </Routes>
  );
};