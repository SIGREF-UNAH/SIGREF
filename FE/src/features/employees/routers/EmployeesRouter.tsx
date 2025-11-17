import { Route, Routes } from "react-router";
import { CreateEmployeePage, ManageEmployeesPage } from "../pages";
import { EditEmployeePage } from "../pages/EditEmployeePage";
import EmployeeDetailsPage from "../pages/EmployeeDetailsPage";

export const EmployeesRouter = () => {
  return (
    <Routes>
      <Route path="/*" element={<ManageEmployeesPage />} />
      <Route path="/create" element={<CreateEmployeePage />} />
      <Route path="/edit" element={<EditEmployeePage />} />
      <Route path="/:id" element={<EmployeeDetailsPage />} />
    </Routes>
  );
};
