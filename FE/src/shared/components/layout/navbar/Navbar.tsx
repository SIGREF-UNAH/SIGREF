import { BiSolidBellRing } from "react-icons/bi";

export const Navbar = () => {
  return (
    <nav className="bg-white border-b-4 border-[#3A6EA5] px-6 py-4">
      <div className="flex items-center justify-between">
        {/* Logo y titulo */}
        <div className="flex items-center space-x-6">
          {/* Logo */}
          <div className="flex items-center">
            <img
              src="/logo.png"
              alt="Logo"
              className="h-14 max-h-16 w-auto object-contain"
            />
          </div>

          {/* Titulo principal */}
          <div className="text-xl font-semibold text-[#163C65] ml-8">
            SIGREF - Panel Administrador TI
          </div>
        </div>

        {/* Navegación central */}
        <div className="flex items-center space-x-8">
          <button className="flex flex-col items-center text-[#163C65] border-b-3 border-[#3A6EA5] pb-1 px-4 font-medium cursor-pointer">
            <span>Gestión de</span>
            <span>Empleados</span>
          </button>

          <button className="flex flex-col items-center text-[#163C65] border-b-3 border-[#3A6EA5] pb-1 px-4 font-medium cursor-pointer">
            <span>Gestión de</span>
            <span>Eventos (Logs)</span>
          </button>

          <button className="flex flex-col items-center text-[#163C65] border-b-3 border-[#3A6EA5] pb-1 px-4 font-medium cursor-pointer">
            <span>Gestión de</span>
            <span>Empresa</span>
          </button>

          <button className="flex flex-col items-center text-[#7BA2D4] border-b-3 border-[#3A6EA5] pb-1 px-4 font-medium cursor-pointer">
            <span>Gestión de</span>
            <span>Organizaciones</span>
          </button>
        </div>

        {/* Area de usuario */}
        <div className="flex items-center space-x-8">
          <div className="relative flex items-center justify-center w-13 h-13 bg-[#D9D9D9] rounded-md">
            <BiSolidBellRing className="w-9 h-9 text-[#3A6EA5]" />
            <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
              10
            </span>
          </div>

          {/* Avatar y info del usuario */}
          <div className="flex items-center space-x-3">
            <div className="w-13 h-13 bg-[#3A6EA5] rounded-full flex items-center justify-center">
              <span className="text-white font-bold text-base">JP</span>
            </div>

            {/* Info */}
            <div className="flex flex-col items-center text-center">
              <div className="text-sm font-medium text-[#163C65]">Administrador TI</div>
              <div className="text-sm font-medium text-[#3A6EA5]">Juan Perez</div>
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
};
