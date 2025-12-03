import { Navigate, Route, Routes } from "react-router";
import {
  CreateServiceGroupPage,
  ServiceGroupsPage,
  UpdateServiceGroupPage,
} from "../pages";

export const ServiceGroupsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<ServiceGroupsPage />} />
      <Route path="/create" element={<CreateServiceGroupPage />} />
      <Route path="/update/:id" element={<UpdateServiceGroupPage />} />
    </Routes>
  );
};
