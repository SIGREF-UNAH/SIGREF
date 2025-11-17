import { Navigate, Route, Routes } from "react-router";
import { EventHistoryPage } from "../pages";

export const EventsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<EventHistoryPage/>}/>
    </Routes>
  );
};