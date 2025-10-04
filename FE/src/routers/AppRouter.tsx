import { Route, Routes } from "react-router";
import { Home } from "../features/auth/pages";
import { Layout } from "../shared/components";
import { HealthcaresRouter } from "../features/healthcares/routers";


export const AppRouter = () => {
  return (
    <Routes>
      <Route element={<Layout />}>

        {/* Pagina de inicio */}
        <Route path="*" element={<Home />} />

        {/* Servicios Médicos */}
        <Route path="/healthcares/*" element={<HealthcaresRouter />} />

      </Route>
    </Routes>
  );
};
