import { Navigate, Route, Routes } from "react-router";
import { Home } from "../features/auth/pages";
import { ExampleRouter } from "../features/example/routers";

export const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/example/*" element={<ExampleRouter />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};  
