import { Skeleton } from "antd";

export const DashboardSkeleton = () => {
  return (
    <div className="min-h-screen p-3 space-y-6">

      {/* Header */}
      <div className="flex justify-between items-center flex-wrap gap-4">
        <Skeleton.Input active size="large" className="!w-64" />
        <div className="flex gap-3">
          <Skeleton.Input active size="large" className="!w-64" />
        </div>
      </div>

      {/* Cards superiores */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="bg-white p-6 rounded-lg shadow-sm">
            <div className="flex justify-between items-center">
              <div className="space-y-2">
                <Skeleton.Input active size="small" className="!w-32" />
                <Skeleton.Input active size="large" className="!w-24" />
                <Skeleton.Input active size="small" className="!w-20" />
              </div>
              <Skeleton.Avatar active size={48} shape="square" />
            </div>
          </div>
        ))}
      </div>

      {/* Sección servicios / paquetes */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {[...Array(3)].map((_, i) => (
          <div key={i} className="bg-white p-6 rounded-lg shadow-md">
            <Skeleton.Input active size="default" className="!w-48 mb-4" />
            <div className="space-y-3">
              {[...Array(5)].map((_, j) => (
                <Skeleton.Input key={j} active size="small" className="!w-full" />
              ))}
            </div>
            <Skeleton.Input active size="small" className="!w-32 mt-4" />
          </div>
        ))}
      </div>

      {/* Gráficos principales */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {[...Array(2)].map((_, i) => (
          <div key={i} className="bg-white p-6 rounded-lg shadow-md">
            <Skeleton.Input active size="default" className="!w-48 mb-4" />
            <Skeleton.Node active className="!w-full !h-[300px] rounded-lg">
              <div className="w-full h-full bg-gray-100 rounded-lg" />
            </Skeleton.Node>

            {i === 0 && (
              <div className="mt-4 p-3 bg-gray-50 rounded-lg space-y-2">
                <Skeleton.Input active size="small" className="!w-40" />
                <Skeleton.Input active size="large" className="!w-32" />
              </div>
            )}

            <Skeleton.Input active size="small" className="!w-32 mt-4" />
          </div>
        ))}
      </div>

      {/* Gráficos inferiores */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {[...Array(2)].map((_, i) => (
          <div key={i} className="bg-white p-6 rounded-lg shadow-md">
            <Skeleton.Input active size="default" className="!w-48 mb-4" />
            <Skeleton.Node active className="!w-full !h-[300px] rounded-lg">
              <div className="w-full h-full bg-gray-100 rounded-lg" />
            </Skeleton.Node>
            <Skeleton.Input active size="small" className="!w-32 mt-4" />
          </div>
        ))}
      </div>

    </div>
  );
};
