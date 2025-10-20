import { Navigate, Route, Routes } from "react-router";
import { CreateIncomePage, ListIncomePage } from "../pages";

export const IncomesRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<ListIncomePage />} />
      <Route path="/create" element={<CreateIncomePage />} />
    </Routes>
  );
};
