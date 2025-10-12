import { Skeleton } from "antd";

export const HealthcareFormSkeleton = () => {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      {/* Columna Izquierda - Formulario */}
      <div className="space-y-4">
        {/* Nombre */}
        <div>
          <Skeleton.Input
            active
            size="small"
            block
            style={{ width: 80, marginBottom: 8 }}
          />
          <Skeleton.Input active size="large" block />
        </div>

        {/* Abreviatura */}
        <div>
          <Skeleton.Input
            active
            size="small"
            block
            style={{ width: 100, marginBottom: 8 }}
          />
          <Skeleton.Input active size="large" block />
        </div>

        {/* Costo */}
        <div>
          <Skeleton.Input
            active
            size="small"
            block
            style={{ width: 60, marginBottom: 8 }}
          />
          <Skeleton.Input active size="large" block />
        </div>

        {/* Switch Active */}
        <div>
          <Skeleton.Input
            active
            size="small"
            block
            style={{ width: 60, marginBottom: 8 }}
          />
          <Skeleton.Button
            active
            size="small"
            style={{ width: 50, height: 24 }}
          />
        </div>

        {/* Descripción */}
        <div>
          <Skeleton.Input
            active
            size="small"
            block
            style={{ width: 100, marginBottom: 8 }}
          />
          <Skeleton.Input active block style={{ height: 100 }} />
        </div>
      </div>

      {/* Columna Derecha - Tabla */}
      <div className="space-y-4">
        {/* Organización */}
        <div>
          <Skeleton.Input
            active
            size="small"
            block
            style={{ width: 120, marginBottom: 8 }}
          />
          <Skeleton.Input active size="large" block />
        </div>
        
        {/* Ubicaciones */}
        <div>
          <Skeleton.Input
            active
            size="small"
            block
            style={{ width: 300, marginBottom: 8 }}
          />

          {/* Buscador */}
          <Skeleton.Input
            active
            size="large"
            block
            style={{ marginBottom: 12 }}
          />

          {/* Tabla simulada */}
          <div className="border border-gray-300 rounded-lg p-4">
            <div className="space-y-3">
              {/* Header de tabla */}
              <div className="flex gap-4 pb-2 border-b">
                <Skeleton.Input
                  active
                  size="small"
                  style={{ width: 20, height: 20 }}
                />
                <Skeleton.Input active size="small" style={{ width: 150 }} />
                <Skeleton.Input active size="small" style={{ width: 100 }} />
              </div>

              {/* Filas de tabla */}
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} className="flex gap-4 items-center">
                  <Skeleton.Input
                    active
                    size="small"
                    style={{ width: 20, height: 20 }}
                  />
                  <Skeleton.Input active size="small" style={{ width: 150 }} />
                  <Skeleton.Button
                    active
                    size="small"
                    style={{ width: 70, height: 24 }}
                  />
                </div>
              ))}
            </div>

            {/* Paginación */}
            <div className="flex justify-between items-center mt-4 pt-3 border-t">
              <Skeleton.Input active size="small" style={{ width: 100 }} />
              <div className="flex gap-2">
                <Skeleton.Button
                  active
                  size="small"
                  style={{ width: 32, height: 32 }}
                />
                <Skeleton.Button
                  active
                  size="small"
                  style={{ width: 32, height: 32 }}
                />
                <Skeleton.Button
                  active
                  size="small"
                  style={{ width: 32, height: 32 }}
                />
                <Skeleton.Button
                  active
                  size="small"
                  style={{ width: 32, height: 32 }}
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Botones */}
      <div className="lg:col-span-2 flex justify-end gap-3 mt-4">
        <Skeleton.Button active size="large" style={{ width: 120 }} />
        <Skeleton.Button active size="large" style={{ width: 150 }} />
      </div>
    </div>
  );
};
