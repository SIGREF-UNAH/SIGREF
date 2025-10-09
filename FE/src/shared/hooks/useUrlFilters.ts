import { useCallback, useMemo } from "react";
import { useSearchParams } from "react-router";

interface UseUrlFiltersConfig<T extends Record<string, any>> {
  defaultValues: T;
  serializers?: Partial<Record<keyof T, (value: any) => string>>;
  deserializers?: Partial<Record<keyof T, (value: string) => any>>;
}

/**
 * Hook reutilizable para manejar filtros, búsqueda y paginación en la URL
 * 
 * @example
 * const { filters, setFilter, setFilters, resetFilters } = useUrlFilters({
 *   defaultValues: {
 *     search: "",
 *     department: undefined,
 *     page: 1,
 *     pageSize: 10,
 *   }
 * });
 */

export function useUrlFilters<T extends Record<string, any>>({
  defaultValues,
  serializers = {},
  deserializers = {},
}: UseUrlFiltersConfig<T>) {
  const [searchParams, setSearchParams] = useSearchParams();

  // Parsear los valores de la URL
  const filters = useMemo(() => {
    const result = { ...defaultValues } as T;

    for (const key in defaultValues) {
      const urlValue = searchParams.get(key);
      
      if (urlValue !== null) {
        // Si hay un deserializer personalizado, usarlo
        if (deserializers[key]) {
          result[key] = deserializers[key]!(urlValue);
        } else {
          // Deserialización automática por tipo
          const defaultValue = defaultValues[key];
          
          if (typeof defaultValue === "number") {
            result[key] = Number(urlValue) as any;
          } else if (typeof defaultValue === "boolean") {
            result[key] = (urlValue === "true") as any;
          } else if (defaultValue === undefined) {
            // Para valores opcionales, guardar como string o undefined
            result[key] = (urlValue === "" ? undefined : urlValue) as any;
          } else {
            result[key] = urlValue as any;
          }
        }
      }
    }

    return result;
  }, [searchParams, defaultValues, deserializers]);

  // Actualizar un solo filtro
  const setFilter = useCallback(
    <K extends keyof T>(key: K, value: T[K]) => {
      setSearchParams((prev) => {
        const newParams = new URLSearchParams(prev);

        // Si el valor es undefined, null, o string vacío, remover el parámetro
        if (value === undefined || value === null || value === "") {
          newParams.delete(key as string);
        } else {
          // Si hay un serializer personalizado, usarlo
          const serializedValue = serializers[key]
            ? serializers[key]!(value)
            : String(value);
          
          newParams.set(key as string, serializedValue);
        }

        return newParams;
      });
    },
    [setSearchParams, serializers]
  );

  // Actualizar múltiples filtros a la vez
  const setFilters = useCallback(
    (updates: Partial<T>) => {
      setSearchParams((prev) => {
        const newParams = new URLSearchParams(prev);

        for (const key in updates) {
          const value = updates[key];

          if (value === undefined || value === null || value === "") {
            newParams.delete(key);
          } else {
            const serializedValue = serializers[key]
              ? serializers[key]!(value)
              : String(value);
            
            newParams.set(key, serializedValue);
          }
        }

        return newParams;
      });
    },
    [setSearchParams, serializers]
  );

  // Resetear todos los filtros a valores por defecto
  const resetFilters = useCallback(() => {
    setSearchParams(new URLSearchParams());
  }, [setSearchParams]);

  // Resetear filtros específicos
  const resetSpecificFilters = useCallback(
    (keys: (keyof T)[]) => {
      setSearchParams((prev) => {
        const newParams = new URLSearchParams(prev);
        keys.forEach((key) => newParams.delete(key as string));
        return newParams;
      });
    },
    [setSearchParams]
  );

  return {
    filters,
    setFilter,
    setFilters,
    resetFilters,
    resetSpecificFilters,
  };
}