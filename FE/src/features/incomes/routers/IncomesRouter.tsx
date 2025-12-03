import { Navigate, Route, Routes } from "react-router";
import { CashClosingPage, CreateIncomePage, HistoryClosingPage, ListIncomePage } from "../pages";

export const IncomesRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<ListIncomePage />} />
      <Route path="/create" element={<CreateIncomePage />} />
      <Route path="/close" element={<CashClosingPage />}/>
      <Route path="/history" element={<HistoryClosingPage />}/> 
    </Routes>
  );
};
