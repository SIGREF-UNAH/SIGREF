import { Route, Routes } from "react-router";
import { AuthRouter } from "../features/auth/routers";

export const AppRouter = () => {
  return (
    <Routes>
      <Route>
        <Route path="/*" element={<AuthRouter />} />
      </Route>
    </Routes>
  );
};
