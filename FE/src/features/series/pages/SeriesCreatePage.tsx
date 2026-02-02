import { useCreateSerie } from "../hooks";
import { SeriesForm } from "../components";
import { PageHeaderTabs } from "../../../shared/components";
import { useAbility } from "../../../config";

export const SeriesCreatePage = () => {
  const { handleFinish, isPending } = useCreateSerie();
  const ability = useAbility();

  return (
    <div>
      {/* Navegación */}
      <PageHeaderTabs
        title="Gestión de Series"
        tabs={[
          ...(ability.can("read", "series")
            ? [
                {
                  key: "list",
                  label: "Lista de Series",
                  path: "/series/list",
                },
              ]
            : []),
          ...(ability.can("create", "series")
            ? [
                {
                  key: "create",
                  label: "Crear Serie",
                  path: "/series/create",
                },
              ]
            : []),
        ]}
        defaultActive="list"
      />

      {/* Formulario */}
      <div className="primary-card">
        <SeriesForm mode="create" onFinish={handleFinish} loading={isPending} />
      </div>
    </div>
  );
};
