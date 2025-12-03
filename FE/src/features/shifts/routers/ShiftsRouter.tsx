import { Navigate, Route, Routes } from "react-router";
import { ShiftsPage } from "../pages/ShiftsPage";

export const ShiftsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<ShiftsPage />} />
    </Routes>
  );
};