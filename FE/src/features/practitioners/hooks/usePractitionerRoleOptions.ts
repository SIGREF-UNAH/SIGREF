import { useState, useCallback, useRef } from "react";
import { getValueSetListByType } from "../../../api/value-set/value-set";
import type { CatalogType } from "../../../api/models/catalogType";

const PAGE_SIZE = 20;
const ROLE_CATALOG_TYPE: CatalogType = "roles"; 
type Option = { label: string; value: string };

export function usePractitionerRoleOptions() {
  const [options, setOptions] = useState<Option[]>([]);
  const [loading, setLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const pageRef = useRef(1);
  const loadingRef = useRef(false);

  const loadMore = useCallback(async () => {
    if (loadingRef.current || !hasMore) return;

    loadingRef.current = true;
    setLoading(true);

    try {
      const res = await getValueSetListByType(ROLE_CATALOG_TYPE, {
        Page: pageRef.current,
        PageSize: PAGE_SIZE,
      });

      const items = res.items ?? [];
      const mapped: Option[] = items.map((item) => ({
        label: item.display ?? item.code ?? "",
        value: item.code ?? "",
      }));

      setOptions((prev) => [...prev, ...mapped]);

      const total = res.pagination?.totalItems ?? 0;
      const loaded = pageRef.current * PAGE_SIZE;
      setHasMore(loaded < total);
      pageRef.current += 1;
    } catch (err) {
      console.error("Error cargando roles:", err);
    } finally {
      loadingRef.current = false;
      setLoading(false);
    }
  }, [hasMore]);

  const reset = useCallback(() => {
    setOptions([]);
    setHasMore(true);
    pageRef.current = 1;
  }, []);

  return { options, loading, hasMore, loadMore, reset };
}