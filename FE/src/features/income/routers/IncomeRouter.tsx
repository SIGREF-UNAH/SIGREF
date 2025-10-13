import { Route, Routes } from "react-router";
import { CreateIncomePage } from "../pages/Create";
import { ListIncomePage } from "../pages/list";

export const IncomeRouter = () => {
  return (
    <Routes>
      <Route path="/create" element={<CreateIncomePage />} />
      <Route path="/list" element={<ListIncomePage />} />
    </Routes>
  );
};
