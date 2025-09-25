import { Navigate, Route, Routes } from "react-router"
import LoginPage from "../pages/LoginPage"

const AuthRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<LoginPage/>} />
    </Routes>
  )
}

export default AuthRouter