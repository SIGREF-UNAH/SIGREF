import { Skeleton } from "antd";

export const HealthcaresPageSkeleton = () => {
  return (
    <div>
      {/* Contenido Skeleton */}
      <div className="p-4 border-2 bg-card border-primary shadow-md rounded-lg">
        {/* Filtros Skeleton */}
        <div className="flex justify-end gap-3 mb-4">
          <Skeleton.Input style={{ width: 300, height: 36 }} active />
          <Skeleton.Input style={{ width: 200, height: 36 }} active />
          <Skeleton.Input style={{ width: 150, height: 36 }} active />
        </div>

        {/* Tabla Skeleton */}
        <div className="border rounded">
          {/* Header de la tabla */}
          <div className="bg-gray-50 border-b p-3 flex gap-2">
            <Skeleton.Input style={{ width: 120, height: 20 }} active />
            <Skeleton.Input style={{ width: 200, height: 20 }} active />
            <Skeleton.Input style={{ width: 200, height: 20 }} active />
            <Skeleton.Input style={{ width: 120, height: 20 }} active />
            <Skeleton.Input style={{ width: 60, height: 20 }} active />
            <Skeleton.Input style={{ width: 120, height: 20 }} active />
          </div>

          {/* Filas de la tabla */}
          {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((item) => (
            <div key={item} className="border-b p-3 flex gap-2 items-center">
              <Skeleton.Input style={{ width: 120, height: 16 }} active />
              <Skeleton.Input style={{ width: 200, height: 16 }} active />
              <Skeleton.Input style={{ width: 200, height: 16 }} active />
              <Skeleton.Input style={{ width: 120, height: 16 }} active />
              <Skeleton.Button
                style={{ width: 60, height: 24 }}
                active
                size="small"
              />
              <div className="flex gap-1" style={{ width: 120 }}>
                <Skeleton.Button
                  style={{ width: 32, height: 32 }}
                  active
                  shape="circle"
                />
                <Skeleton.Button
                  style={{ width: 32, height: 32 }}
                  active
                  shape="circle"
                />
              </div>
            </div>
          ))}

          {/* Paginación Skeleton */}
          <div className="p-3 flex justify-between items-center">
            <Skeleton.Input style={{ width: 100, height: 20 }} active />
            <div className="flex gap-2">
              <Skeleton.Button style={{ width: 32, height: 32 }} active />
              <Skeleton.Button style={{ width: 32, height: 32 }} active />
              <Skeleton.Button style={{ width: 32, height: 32 }} active />
              <Skeleton.Button style={{ width: 32, height: 32 }} active />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
