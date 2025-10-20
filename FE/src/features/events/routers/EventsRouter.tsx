import { Navigate, Route, Routes } from "react-router";

export const EventsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      {/* <Route path="/list" element={<EventsListPage />} /> */}
    </Routes>
  );
};