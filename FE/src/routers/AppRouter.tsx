import { Route, Routes } from "react-router";
import { Home } from "../features/auth/pages";

export const AppRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Home />} />
    </Routes>
  );
};  
