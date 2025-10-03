import { Route, Routes } from "react-router";
import { Home } from "../features/auth/pages";
import { Layout } from "../shared/components";


export const AppRouter = () => {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="*" element={<Home />} />
      </Route>
    </Routes>
  );
};
