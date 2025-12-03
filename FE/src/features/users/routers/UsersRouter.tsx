import { Navigate, Route, Routes } from "react-router"
import CreateUsersPage from "../pages/CreateUsersPage"

export const UsersRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="create" replace />} />
      <Route path="/create" element={<CreateUsersPage />} />
    </Routes>
  )
}

