import { Route, Routes } from "react-router";
import { CreateIncomePage, ListIncomePage } from "../pages";

export const IncomesRouter = () => {
  return (
    <Routes>
      <Route path="/list" element={<ListIncomePage />} />
      <Route path="/create" element={<CreateIncomePage />} />
    </Routes>
  );
};
